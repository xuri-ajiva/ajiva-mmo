using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using Client.Renderer.Pipelines;
using Client.Renderer.Resources;
using Client.Wgsl;
using Hexa.NET.ImGui;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using SixLabors.ImageSharp.PixelFormats;

namespace Client.Renderer.Renderers;

public partial class ImGuiPlatform
{
    private MemoryHandle _platformNameHandle = PlatformName.Pin();
    private ImGuiPipeline? _pipeline;
    private AjivaBuffer<ImDrawVert>? _vertexBuffer;
    private readonly List<ImDrawVert> _vertexDataCache = [];
    private readonly List<ushort> _indexDataCache = [];
    private AjivaBuffer<ushort>? _indexBuffer;
    private readonly Dictionary<ImTextureID, (AjivaTexture texture, BindGroupPtr? bindGroup)> _textures = [];

    private unsafe void RenderImDrawData(ImDrawDataPtr data, RenderPassEncoderPtr pass)
    {
        if (data.CmdListsCount == 0)
        {
            return;
        }

        if (_vertexBuffer!.Count < (u64)data.TotalVtxCount)
            _vertexBuffer.ResizeWithoutDataPreserve((u64)data.TotalVtxCount);
        if (_indexDataCache.Count < data.TotalIdxCount)
        {
            _indexDataCache.Capacity = data.TotalIdxCount;
            _indexDataCache.AddRange(Enumerable.Repeat<ushort>(0, data.TotalIdxCount - _indexDataCache.Count));
        }

        if (_indexBuffer!.Count < (u64)data.TotalIdxCount)
            _indexBuffer.ResizeWithoutDataPreserve((u64)data.TotalIdxCount);
        if (_vertexDataCache.Count < data.TotalVtxCount)
        {
            _vertexDataCache.Capacity = data.TotalVtxCount;
            _vertexDataCache.AddRange(Enumerable.Repeat<ImDrawVert>(default,
                data.TotalVtxCount - _vertexDataCache.Count));
        }

        var vertexDataSpan = CollectionsMarshal.AsSpan(_vertexDataCache);
        var indexDataSpan = CollectionsMarshal.AsSpan(_indexDataCache);

        using var guard = loader.Api.EnterQueue();
        var queue = guard.Queue;
        var vertexOffsetInVertices = 0;
        var indexOffsetInElements = 0;
        for (var i = 0; i < data.CmdListsCount; i++)
        {
            var cmdList = data.CmdLists[i];

            new ReadOnlySpan<ImDrawVert>(cmdList.VtxBuffer.Data, cmdList.VtxBuffer.Size)
                .CopyTo(vertexDataSpan.Slice(vertexOffsetInVertices, cmdList.VtxBuffer.Size));
            new ReadOnlySpan<ushort>(cmdList.IdxBuffer.Data, cmdList.IdxBuffer.Size)
                .CopyTo(indexDataSpan.Slice(indexOffsetInElements, cmdList.IdxBuffer.Size));

            vertexOffsetInVertices += cmdList.VtxBuffer.Size;
            indexOffsetInElements += cmdList.IdxBuffer.Size;
        }

        _vertexBuffer.QueuedWrite(vertexDataSpan, 0, queue);
        _indexBuffer.QueuedWrite(indexDataSpan, 0, queue);

        for (var i = 0; i < data.Textures.Size; i++)
        {
            var tex = data.Textures[i];
            UpdateTexture(tex);
        }


        var io = ImGui.GetIO();

        // Setup orthographic projection matrix into our constant buffer
        _pipeline!.UpdateUniform(
            new ImGuiShader.Uniforms
            {
                MvpRef = Matrix4x4.CreateOrthographicOffCenter(0f, io.DisplaySize.X, io.DisplaySize.Y, 0.0f, -1.0f,
                    1.0f),
                Gamma = 1.0f
            }, queue);

        pass.SetVertexBuffer(0, _vertexBuffer!.Buffer, _vertexBuffer.OffsetBytes, _vertexBuffer.Size);
        pass.SetIndexBuffer(_indexBuffer!.Buffer, IndexFormat.Uint16, _indexBuffer.OffsetBytes, _indexBuffer.Size);
        pass.SetPipeline(_pipeline!.Pipeline);
        pass.SetBindGroup(0, _pipeline.BindGroup0, null);

        data.ScaleClipRects(io.DisplayFramebufferScale);

        // Render command lists
        u32 vertexBase = 0;
        u32 indexBase = 0;
        for (var n = 0; n < data.CmdListsCount; n++)
        {
            var cmdList = data.CmdLists[n];
            for (var cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
            {
                var pcmd = cmdList.CmdBuffer[cmdi];
                if (pcmd.UserCallback != null) throw new NotSupportedException();

                var textureBind = GetOrCreateTextureBindGroup(pcmd.TexRef);

                pass.SetBindGroup(1, textureBind, null);

                var clipRect = pcmd.ClipRect;
                pass.SetScissorRect((u32)clipRect.X, (u32)clipRect.Y, (u32)(clipRect.Z - clipRect.X),
                    (u32)(clipRect.W - clipRect.Y));
                pass.DrawIndexed(pcmd.ElemCount, 1, indexBase + pcmd.IdxOffset, (i32)(vertexBase + pcmd.VtxOffset), 0);
            }

            vertexBase += (u32)cmdList.VtxBuffer.Size;
            indexBase += (u32)cmdList.IdxBuffer.Size;
        }
    }

    private bool CreateDeviceObjects(IImGuiShader currentShader)
    {
        if (_pipeline is not null)
            InvalidateDeviceObjects();

        _vertexBuffer ??= loader.CreateBuffer<ImDrawVert>(BufferUsage.Vertex | BufferUsage.CopyDst, 10000,
            label: "ImGui Vertex Input Buffer");
        _indexBuffer ??= loader.CreateBuffer<ushort>(BufferUsage.Index | BufferUsage.CopyDst, 2000,
            label: "ImGui Index Buffer");

        _pipeline = ImGuiPipeline.Create(loader, currentShader, TextureFormat.Bgra8Unorm);

        return true;
    }

    private void InvalidateDeviceObjects()
    {
        _vertexBuffer?.Dispose();
        _vertexBuffer = null;
        _indexBuffer?.Dispose();
        _indexBuffer = null;
        _pipeline?.Dispose();
        _pipeline = null;
        var textures = ImGui.GetPlatformIO().Textures;
        for (int i = 0; i < textures.Size; i++)
        {
            var tex = textures[i];
            if (tex.RefCount == 1)
                DestroyTexture(tex);
        }
    }

    private void DestroyTexture(ImTextureDataPtr tex)
    {
        if (tex.IsNull) return;

        if (_textures.Remove(tex.TexID, out var tuple))
        {
            tuple.texture?.Dispose();
            tuple.bindGroup?.Release();
        }

        tex.SetStatus(ImTextureStatus.Destroyed);
    }

    private unsafe BindGroupPtr GetOrCreateTextureBindGroup(ImTextureRef imTextureRef)
    {
        var textureId = imTextureRef.TexData is not null ? imTextureRef.TexData->TexID : imTextureRef.TexID;

        if (_textures.TryGetValue(textureId, out var tuple))
        {
            if (tuple.bindGroup is not null) return tuple.bindGroup.Value;

            var textureBindGroup = _pipeline!.CreateTextureBindGroup(tuple.texture.View);
            _textures[textureId] = (tuple.texture, textureBindGroup);
            return textureBindGroup;
        }

        {
            logger.LogWarning(
                "Texture with ID {ID} not found in texture map, creating a speculative bind group for it. This may indicate a problem with texture management in ImGui.",
                textureId.Handle);
            var textureViewPtr = new TextureViewPtr(loader.Api.Device.GetAPI(), (TextureView*)textureId);
            var textureBindGroup = _pipeline!.CreateTextureBindGroup(textureViewPtr);
            _textures.Add(textureId, (null!, textureBindGroup));
            return textureBindGroup;
        }
    }

    private unsafe void UpdateTexture(ImTextureDataPtr tex)
    {
        AjivaTexture texture;
        if (tex.Status == ImTextureStatus.WantCreate)
        {
            // Create and upload new texture to graphics system
            logger.LogDebug("UpdateTexture {UniqueID}: WantCreate {Width}x{Height} Format {Format}", tex.UniqueID,
                tex.Width, tex.Height, tex.Format);

            texture = AjivaTexture.FromData(loader.CreateTexture(TextureUsage.TextureBinding | TextureUsage.CopyDst,
                "ImGui Texture", 1,
                new Extent3D { Width = (u32)tex.Width, Height = (u32)tex.Height, DepthOrArrayLayers = 1 },
                TextureFormat.Rgba8Unorm,
                TextureAspect.All));

            ImTextureID id = texture.View.GetIntPtr();
            _textures.Add(id, (texture, null));

            // Store identifiers
            tex.SetTexID(id);
            // We don't set tex.Status to ImTextureStatus_OK to let the code fallthrough below.
        }
        else
        {
            if (!TryGetTexture(tex, out texture!))
            {
                logger.LogError("Texture with ID {ID} not found in texture map", tex.TexID);
                return;
            }
        }

        if (tex.Status is ImTextureStatus.WantCreate or ImTextureStatus.WantUpdates)
        {
            Debug.Assert(tex.Format == ImTextureFormat.Rgba32);

            // todo implement smarter texture updates by only updating the dirty rect instead of the whole texture
            var data = new Span<Rgba32>(tex.GetPixels(), tex.GetSizeInBytes());
            texture.WriteTexture(data, new Extent3D
            {
                Width = (u32)tex.Width,
                Height = (u32)tex.Height,
                DepthOrArrayLayers = 1
            });
            tex.SetStatus(ImTextureStatus.Ok);
        }

        if (tex is { Status: ImTextureStatus.Destroyed, UnusedFrames: > 10 })
            DestroyTexture(tex);
    }

    private bool TryGetTexture(ImTextureDataPtr tex, [NotNullWhen(true)] out AjivaTexture? texture)
    {
        texture = null;
        if (tex is { IsNull: true, TexID.IsNull: true }) return false;
        var id = tex.TexID;
        if (!_textures.TryGetValue(id, out var bindGroup)) return false;
        texture = bindGroup.texture;
        return true;
    }

    public void Render(RenderPassEncoderPtr pass)
    {
        ImGui.Render();
        ImGui.EndFrame();
        RenderImDrawData(ImGui.GetDrawData(), pass);
    }
}
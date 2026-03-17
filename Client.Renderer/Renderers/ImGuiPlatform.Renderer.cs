using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using Client.Renderer.Pipelines;
using Client.Renderer.Resources;
using Client.Wgsl;
using GlmSharp;
using Hexa.NET.ImGui;
using Hexa.NET.SDL3;
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
    private readonly List<u16> _indexDataCache = [];
    private AjivaBuffer<u16>? _indexBuffer;

    internal record TextureBindCacheEntry(AjivaTexture Texture, BindGroupPtr? BindGroup)
    {
        public u64 LastUsedTick { get; set; } = 0;
    }

    private readonly Dictionary<ImTextureID, TextureBindCacheEntry> _textures = [];

    private unsafe void RenderImDrawData(ImDrawDataPtr drawData, RenderPassEncoderPtr pass)
    {
        if (drawData.CmdListsCount == 0) return;

        if (_vertexBuffer!.Count < (u64)drawData.TotalVtxCount)
            _vertexBuffer.ResizeWithoutDataPreserve((u64)drawData.TotalVtxCount);
        if (_indexDataCache.Count < drawData.TotalIdxCount)
        {
            _indexDataCache.Capacity = drawData.TotalIdxCount;
            _indexDataCache.AddRange(Enumerable.Repeat<u16>(0, drawData.TotalIdxCount - _indexDataCache.Count));
        }

        if (_indexBuffer!.Count < (u64)drawData.TotalIdxCount)
            _indexBuffer.ResizeWithoutDataPreserve((u64)drawData.TotalIdxCount);
        if (_vertexDataCache.Count < drawData.TotalVtxCount)
        {
            _vertexDataCache.Capacity = drawData.TotalVtxCount;
            _vertexDataCache.AddRange(Enumerable.Repeat<ImDrawVert>(default,
                drawData.TotalVtxCount - _vertexDataCache.Count));
        }

        var vertexDataSpan = CollectionsMarshal.AsSpan(_vertexDataCache);
        var indexDataSpan = CollectionsMarshal.AsSpan(_indexDataCache);

        using var guard = loader.Api.EnterQueue();
        var queue = guard.Queue;
        var vertexOffsetInVertices = 0;
        var indexOffsetInElements = 0;
        for (var i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdLists[i];

            new ReadOnlySpan<ImDrawVert>(cmdList.VtxBuffer.Data, cmdList.VtxBuffer.Size)
                .CopyTo(vertexDataSpan.Slice(vertexOffsetInVertices, cmdList.VtxBuffer.Size));
            new ReadOnlySpan<u16>(cmdList.IdxBuffer.Data, cmdList.IdxBuffer.Size)
                .CopyTo(indexDataSpan.Slice(indexOffsetInElements, cmdList.IdxBuffer.Size));

            vertexOffsetInVertices += cmdList.VtxBuffer.Size;
            indexOffsetInElements += cmdList.IdxBuffer.Size;
        }

        _vertexBuffer.QueuedWrite(vertexDataSpan, 0, queue);
        _indexBuffer.QueuedWrite(indexDataSpan, 0, queue);

        for (var i = 0; i < drawData.Textures.Size; i++)
        {
            var tex = drawData.Textures[i];
            UpdateTexture(tex);
        }

        var io = ImGui.GetIO();

        _pipeline!.UpdateUniform(
            new ImGuiShader.Uniforms
            {
                // flipped Y as per wgpu coordinate system
                Scale = new vec2(2.0f / drawData.DisplaySize.X, -2.0f / drawData.DisplaySize.Y),
                Translate = new vec2(-1.0f - drawData.DisplayPos.X * (2.0f / drawData.DisplaySize.X),
                    1.0f + drawData.DisplayPos.Y * (2.0f / drawData.DisplaySize.Y))
            }, queue);

        pass.SetVertexBuffer(0, _vertexBuffer!.Buffer, _vertexBuffer.OffsetBytes, _vertexBuffer.Size);
        pass.SetIndexBuffer(_indexBuffer!.Buffer, IndexFormat.Uint16, _indexBuffer.OffsetBytes, _indexBuffer.Size);
        pass.SetPipeline(_pipeline!.Pipeline);
        pass.SetBindGroup(0, _pipeline.BindGroup0, null);

        drawData.ScaleClipRects(io.DisplayFramebufferScale);

        // Will project scissor/clipping rectangles into framebuffer space
        var clipOff = drawData.DisplayPos; // (0,0) unless using multi-viewports
        var clipScale = drawData.FramebufferScale; // (1,1) unless using retina display which are often (2,2)

        // Render command lists
        u32 vertexBase = 0;
        u32 indexBase = 0;
        for (var n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdLists[n];
            for (var cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
            {
                var pcmd = cmdList.CmdBuffer[cmdi];
                if (pcmd.UserCallback != null) throw new NotSupportedException();

                var bindCacheEntry = GetOrCreateTextureBindGroup(pcmd.TexRef);
                bindCacheEntry.LastUsedTick = _currentTick;
                pass.SetBindGroup(1, bindCacheEntry.BindGroup!.Value, null);

                // Project scissor/clipping rectangles into framebuffer space
                // Clamp to viewport as SetScissorRect() won't accept values that are off bounds
                var clipMin = Vector2.Max((new Vector2(pcmd.ClipRect.X, pcmd.ClipRect.Y) - clipOff) * clipScale,
                    Vector2.Zero);
                var clipMax = Vector2.Min((new Vector2(pcmd.ClipRect.Z, pcmd.ClipRect.W) - clipOff) * clipScale,
                    io.DisplaySize * io.DisplayFramebufferScale);

                if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y)
                    continue;

                pass.SetScissorRect((u32)clipMin.X, (u32)clipMin.Y, (u32)(clipMax.X - clipMin.X),
                    (u32)(clipMax.Y - clipMin.Y));
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
        _indexBuffer ??= loader.CreateBuffer<u16>(BufferUsage.Index | BufferUsage.CopyDst, 2000,
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
        for (var i = 0; i < textures.Size; i++)
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
            tuple.Texture?.Dispose();
            tuple.BindGroup?.Release();
        }

        tex.SetStatus(ImTextureStatus.Destroyed);
    }

    private unsafe TextureBindCacheEntry GetOrCreateTextureBindGroup(ImTextureRef imTextureRef)
    {
        var textureId = imTextureRef.TexData is not null ? imTextureRef.TexData->TexID : imTextureRef.TexID;

        if (_textures.TryGetValue(textureId, out var tuple))
        {
            if (tuple.BindGroup is not null) return tuple;

            return _textures[textureId] =
                new TextureBindCacheEntry(tuple.Texture, _pipeline!.CreateTextureBindGroup(tuple.Texture.View));
        }

        TextureViewPtr textureViewPtr;
        if (loader.IsKnownTextureId(textureId, out var texture))
        {
            textureViewPtr = texture.View;
        }
        else
        {
            logger.LogWarning(
                "Texture with ID {ID} not found in texture map, creating a speculative bind group for it. This may indicate a problem with texture management in ImGui.",
                textureId.Handle);
            textureViewPtr = new TextureViewPtr(loader.Api.Wgpu, (TextureView*)(nint)textureId.Handle);
            //loader.DefaultTexture.View;
        }

        return _textures[textureId] =
            new TextureBindCacheEntry(texture!, _pipeline!.CreateTextureBindGroup(textureViewPtr));
    }

    private unsafe void UpdateTexture(ImTextureDataPtr tex)
    {
        AjivaTexture texture;
        if (tex.Status == ImTextureStatus.WantCreate)
        {
            // Create and upload new texture to graphics system
            logger.LogDebug("UpdateTexture {UniqueID}: WantCreate {Width}x{Height} Format {Format}", tex.UniqueID,
                tex.Width, tex.Height, tex.Format);

            texture = AjivaTexture.FromData(loader.CreateTexture(
                TextureUsage.TextureBinding | TextureUsage.CopyDst | TextureUsage.RenderAttachment,
                "ImGui Texture", 1,
                new Extent3D { Width = (u32)tex.Width, Height = (u32)tex.Height, DepthOrArrayLayers = 1 },
                TextureFormat.Rgba8Unorm,
                TextureAspect.All));

            ImTextureID id = texture.View.GetIntPtr();
            _textures.Add(id, new TextureBindCacheEntry(texture, null));

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

        if (tex is { Status: ImTextureStatus.Destroyed })
            DestroyTexture(tex);
    }

    private bool TryGetTexture(ImTextureDataPtr tex, [NotNullWhen(true)] out AjivaTexture? texture)
    {
        texture = null;
        if (tex is { IsNull: true, TexID.IsNull: true }) return false;
        var id = tex.TexID;
        if (!_textures.TryGetValue(id, out var bindGroup)) return false;
        texture = bindGroup.Texture;
        return true;
    }
}
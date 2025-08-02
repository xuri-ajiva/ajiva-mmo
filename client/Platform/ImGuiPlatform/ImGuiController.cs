using System.Numerics;
using Client.Renderer;
using Hexa.NET.ImGui;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.ImGui;
using Silk.NET.WebGPU.Safe;
using static Silk.NET.WebGPU.Safe.BindGroupEntries;
using static Silk.NET.WebGPU.Safe.BindGroupLayoutEntries;

namespace Client.Platform.ImGuiPlatform
{
    record struct BufferRange(BufferPtr Buffer, ulong Offset, ulong Size);

    public class ImGuiController : IDisposable
    {
        internal RendererApi _api;
        private IImGuiPlatform _platform;

        // Device objects
        private BufferRange _vertexBuffer;
        private BufferRange _indexBuffer;
        private BufferPtr _projMatrixBuffer;
        private TexturePtr? _fontTexture;
        private ShaderModulePtr _shaderModule;
        private BindGroupLayoutPtr _layout;
        private BindGroupLayoutPtr _textureLayout;
        private RenderPipelinePtr _pipeline;
        private BindGroupPtr _mainBindGroup;
        private BindGroupPtr? _fontTextureResourceSet;
        private IntPtr _fontAtlasID = (IntPtr)1;

        public ImGuiContextPtr Context;

        private readonly Dictionary<TextureViewPtr, BindGroupPtr> _bindGroupsByView = new();
        private bool _frameBegun;

        private static ulong NextValidBufferSize(ulong size) => (size + 15) / 16 * 16;

        public unsafe ImGuiController(RendererApi api, TextureFormat colorOutputFormat, IImGuiPlatform platform, ImGuiFontConfig? imGuiFontConfig = null, Action? onConfigureIO = null)
        {
            _api = api;
            _platform = platform;

            Context = ImGui.CreateContext();
            ImGui.SetCurrentContext(Context);
            ImGui.StyleColorsDark();

            _platform.Init(Context);

            var io = ImGui.GetIO();
            onConfigureIO?.Invoke();

            CreateDeviceResources(colorOutputFormat);

            _platform.Update(1f / 60f);

            BeginFrame();
        }

        public void MakeCurrent()
        {
            ImGui.SetCurrentContext(Context);
        }

        private void BeginFrame()
        {
            _platform.BeginFrame();
            _frameBegun = true;
        }

        public void Render(RenderPassEncoderPtr pass)
        {
            if (_frameBegun)
            {
                var oldCtx = ImGui.GetCurrentContext();

                if (oldCtx != Context)
                {
                    ImGui.SetCurrentContext(Context);
                }

                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData(), pass);

                if (oldCtx != Context)
                {
                    ImGui.SetCurrentContext(oldCtx);
                }
            }
        }

        public void Update(float deltaSeconds)
        {
            var oldCtx = ImGui.GetCurrentContext();

            if (oldCtx != Context)
            {
                ImGui.SetCurrentContext(Context);
            }

            if (_frameBegun)
            {
                ImGui.Render();
            }

            _platform.Update(deltaSeconds);

            _frameBegun = true;
            ImGui.NewFrame();

            if (oldCtx != Context)
            {
                ImGui.SetCurrentContext(oldCtx);
            }
        }

        private void CreateDeviceResources(TextureFormat colorOutputFormat)
        {
            _vertexBuffer = new(_api.Device.CreateBuffer(BufferUsage.Vertex | BufferUsage.CopyDst, 10000, label: "ImGui.NET Vertex Buffer"),
                                            Offset: 0, Size: 10000);
            _indexBuffer = new(_api.Device.CreateBuffer(BufferUsage.Index | BufferUsage.CopyDst, 2000, label: "ImGui.NET Index Buffer"),
                                        Offset: 0, Size: 2000);

            _projMatrixBuffer = _api.Device.CreateBuffer(BufferUsage.Uniform | BufferUsage.CopyDst,  64, label: "ImGui.NET Projection Buffer");

            Silk.NET.WebGPU.Safe.VertexBufferLayout[] vertexLayouts = new Silk.NET.WebGPU.Safe.VertexBufferLayout[]
            {
                new Silk.NET.WebGPU.Safe.VertexBufferLayout
                {
                    ArrayStride = 5*sizeof(float),
                    StepMode = VertexStepMode.Vertex,
                    Attributes = new VertexAttribute[]
                    {
                        new(VertexFormat.Float32x2, offset: 0, shaderLocation: 0),
                        new(VertexFormat.Float32x2, offset: 2*sizeof(float), shaderLocation: 1),
                        new(VertexFormat.Unorm8x4, offset: 4*sizeof(float), shaderLocation: 2)
                    }
                }
            };

            _layout = _api.Device.CreateBindGroupLayout(
                new BindGroupLayoutEntry[]
                {
                    Buffer(0, ShaderStage.Vertex, BufferBindingType.Uniform, 64),
                    Sampler(1, ShaderStage.Fragment, SamplerBindingType.Filtering)
                },
                label: "ImGui.NET Resource Layout"
            );

            _textureLayout = _api.Device.CreateBindGroupLayout(
                new BindGroupLayoutEntry[]
                {
                    Texture(2, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D, multisampled: false)
                },
                label: "ImGui.NET Texture Layout"
            );

            SamplerPtr pointSampler = _api.Device.CreateSampler(
                AddressMode.Repeat, AddressMode.Repeat, AddressMode.Repeat,
                FilterMode.Nearest, FilterMode.Nearest, MipmapFilterMode.Nearest,
                0, 0, default, 1, 
                label: "ImGui.NET PointSampler");

            var pipelineLayout = _api.Device.CreatePipelineLayout(
                new Silk.NET.WebGPU.Safe.BindGroupLayoutPtr[]
                {
                    _layout,
                    _textureLayout
                }
            );

            var shaderCode = """
                                struct Uniforms {
                    u_Matrix: mat4x4<f32>
                };

                struct VertexInput {
                    @location(0) a_Pos: vec2<f32>,
                    @location(1) a_UV: vec2<f32>,
                    @location(2) a_Color: vec4<f32>
                };

                struct VertexOutput {
                    @location(0) v_UV: vec2<f32>,
                    @location(1) v_Color: vec4<f32>,
                    @builtin(position) v_Position: vec4<f32>
                };

                @group(0)
                @binding(0)
                var<uniform> uniforms: Uniforms;

                @vertex
                fn vs_main(in: VertexInput) -> VertexOutput {
                    var out: VertexOutput;
                    out.v_UV = in.a_UV;
                    out.v_Color = in.a_Color;
                    out.v_Position = uniforms.u_Matrix * vec4<f32>(in.a_Pos.xy, 0.0, 1.0);
                    return out;
                }

                struct FragmentOutput {
                    @location(0) o_Target: vec4<f32>
                };

                @group(0)
                @binding(1)
                var u_Sampler: sampler;
                @group(1)
                @binding(2)
                var u_Texture: texture_2d<f32>;

                @fragment
                fn fs_main(in: VertexOutput) -> FragmentOutput {
                    let color = in.v_Color;

                    return FragmentOutput(color * textureSample(u_Texture, u_Sampler, in.v_UV));
                }
                """u8;

            _shaderModule = _api.Device.CreateShaderModuleWGSL(shaderCode,
                new Silk.NET.WebGPU.Safe.ShaderModuleCompilationHint[]{
                    new Silk.NET.WebGPU.Safe.ShaderModuleCompilationHint("vs_main", pipelineLayout),
                    new Silk.NET.WebGPU.Safe.ShaderModuleCompilationHint("fs_main", pipelineLayout),
                });

            _pipeline = _api.Device.CreateRenderPipeline(
                pipelineLayout,
                vertex: new Silk.NET.WebGPU.Safe.VertexState
                {
                    Module = _shaderModule,
                    EntryPoint = "vs_main",
                    Buffers = vertexLayouts,
                    Constants = Array.Empty<(string, double)>()
                },
                primitive: new Silk.NET.WebGPU.Safe.PrimitiveState()
                {
                    Topology = PrimitiveTopology.TriangleList,
                    StripIndexFormat = IndexFormat.Undefined,
                    FrontFace = FrontFace.Ccw,
                    CullMode = CullMode.None
                },
                depthStencil: null,
                multisample: new MultisampleState
                {
                    Count = 1,
                    Mask = uint.MaxValue,
                    AlphaToCoverageEnabled = false
                },
                fragment: new Silk.NET.WebGPU.Safe.FragmentState
                {
                    Module = _shaderModule,
                    EntryPoint = "fs_main",
                    Targets = new Silk.NET.WebGPU.Safe.ColorTargetState[]
                    {
                        new Silk.NET.WebGPU.Safe.ColorTargetState(colorOutputFormat, (
                            new BlendComponent(BlendOperation.Add, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha),
                            new BlendComponent(BlendOperation.Add, BlendFactor.SrcAlpha, BlendFactor.DstAlpha)),
                            ColorWriteMask.All)
                    },
                    Constants = Array.Empty<(string, double)>()
                }, 
                label: "ImGui.NET Pipeline"
            );

            _mainBindGroup = _api.Device.CreateBindGroup(_layout,
                new BindGroupEntry[]
                {
                    Buffer(0, _projMatrixBuffer, 0, 64),
                    Sampler(1, pointSampler)
                },
                label: "ImGui.NET Main Resource Set");

            RecreateFontDeviceTexture(_api.Device);
        }

        public unsafe IntPtr CreateTextureBindGroup(TextureViewPtr textureView)
        {
            if (!_bindGroupsByView.TryGetValue(textureView, out BindGroupPtr bindGroup))
            {
                bindGroup = CreateTextureBindGroupInternal(textureView);
            }

            return bindGroup.GetIntPtr();
        }

        private unsafe BindGroupPtr CreateTextureBindGroupInternal(TextureViewPtr textureView)
        {
            var entry = Texture(2, textureView);
            var bindGroup = _api.Device.CreateBindGroup(_textureLayout,
                    new ReadOnlySpan<BindGroupEntry>(&entry, 1));

            _bindGroupsByView.Add(textureView, bindGroup);

            return bindGroup;
        }

        public void RemoveTextureBindGroup(TextureViewPtr textureView)
        {
            if (_bindGroupsByView.TryGetValue(textureView, out BindGroupPtr bindGroup))
            {
                _bindGroupsByView.Remove(textureView);
                bindGroup.Release();
            }
        }

        public void ClearCachedImageResources()
        {
            foreach (BindGroupPtr bindGroup in _bindGroupsByView.Values)
            {
                bindGroup.Release();
            }

            _bindGroupsByView.Clear();
        }

        public unsafe void RecreateFontDeviceTexture(DevicePtr device)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            //io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int bytesPerPixel);

            var data = io.Fonts.TexData;
            var pixels = io.Fonts.TexData.Pixels;
            var width = data.Width;
            var height = data.Height;
            var bytesPerPixel = data.BytesPerPixel;
            data.TexID = _fontAtlasID;
            //io.Fonts.SetTexID(_fontAtlasID);

            _fontTexture?.Destroy();
            _fontTexture = device.CreateTexture(
                TextureUsage.TextureBinding | TextureUsage.CopyDst, TextureDimension.Dimension2D, new Extent3D(
                (uint)width,
                (uint)height, 
                depthOrArrayLayers: 1), TextureFormat.Rgba8Unorm,
                mipLevelCount: 1,
                sampleCount: 1,
                viewFormats: new TextureFormat[]
                {
                    TextureFormat.Rgba8Unorm
                },
                label: "ImGui.NET Font Texture");

            device.GetQueue().WriteTexture<byte>(
                new Silk.NET.WebGPU.Safe.ImageCopyTexture{
                    Aspect = TextureAspect.All,
                    MipLevel = 0,
                    Origin = new(0, 0, 0),
                    Texture = _fontTexture.Value
                },

                data: new Span<byte>(pixels,
                (int)(bytesPerPixel * width * height)),
                dataLayout: new TextureDataLayout
                {
                    BytesPerRow = (uint)(bytesPerPixel * width),
                    RowsPerImage = (uint)height,
                    Offset = 0
                },
                writeSize: new Extent3D
                {
                    Width = (uint)width,
                    Height = (uint)height,
                    DepthOrArrayLayers = 1
                });

            var fontTextureView = _fontTexture.Value.CreateView(TextureFormat.Rgba8Unorm, TextureViewDimension.Dimension2D, TextureAspect.All,
                baseMipLevel: 0, mipLevelCount: 1, baseArrayLayer: 0, arrayLayerCount: 1, label: "ImGui.NET Font Texture - View");

            _fontTextureResourceSet?.Release();
            _fontTextureResourceSet = device.CreateBindGroup(_textureLayout, 
                new BindGroupEntry[]
                {
                    Texture(2, fontTextureView)
                }, 
                label: "ImGui.NET Font Texture Resource Set");

            //io.Fonts.ClearTexData();
        }

        public unsafe void RenderImDrawData(ImDrawDataPtr draw_data, RenderPassEncoderPtr pass)
        {
            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            uint totalVBSize = (uint)(draw_data.TotalVtxCount * sizeof(ImDrawVert));
            if (totalVBSize > _vertexBuffer.Size)
            {
                _vertexBuffer.Buffer.Destroy();
                ulong size = NextValidBufferSize((ulong)(totalVBSize * 1.5f));
                _vertexBuffer = new(_api.Device.CreateBuffer(_vertexBuffer.Buffer.GetUsage(), size, label: "ImGui.NET Vertex Buffer"),
                    Offset: 0, size);
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * sizeof(ushort));
            if (totalIBSize > _indexBuffer.Size)
            {
                _indexBuffer.Buffer.Destroy();
                ulong size = NextValidBufferSize((ulong)(totalIBSize * 1.5f));
                _indexBuffer = new(_api.Device.CreateBuffer(_indexBuffer.Buffer.GetUsage(), size, label: "ImGui.NET Index Buffer"),
                    Offset: 0, size);
            }

            var queue = _api.Device.GetQueue();

            Span<ImDrawVert> vertexData = stackalloc ImDrawVert[draw_data.TotalVtxCount];
            Span<ushort> indexData = stackalloc ushort[draw_data.TotalIdxCount];

            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdLists[i];

                new ReadOnlySpan<ImDrawVert>((void*)cmd_list.VtxBuffer.Data, cmd_list.VtxBuffer.Size)
                .CopyTo(vertexData.Slice((int)vertexOffsetInVertices, cmd_list.VtxBuffer.Size));

                new ReadOnlySpan<ushort>((void*)cmd_list.IdxBuffer.Data, cmd_list.IdxBuffer.Size)
                .CopyTo(indexData.Slice((int)indexOffsetInElements, cmd_list.IdxBuffer.Size));

                vertexOffsetInVertices += (uint)cmd_list.VtxBuffer.Size;
                indexOffsetInElements += (uint)cmd_list.IdxBuffer.Size;
            }


            queue.WriteBufferAligned<ImDrawVert>(
                _vertexBuffer.Buffer,
                _vertexBuffer.Offset,
                data: vertexData
            );

            queue.WriteBufferAligned<ushort>(
                _indexBuffer.Buffer,
                _indexBuffer.Offset,
                data: indexData
            );

            {
                var io = ImGui.GetIO();

                Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                    0f,
                    io.DisplaySize.X,
                    io.DisplaySize.Y,
                    0.0f,
                    -1.0f,
                    1.0f);

                queue.WriteBuffer(_projMatrixBuffer, 0, new ReadOnlySpan<Matrix4x4>(&mvp, 1));
            }

            pass.SetVertexBuffer(0, _vertexBuffer.Buffer, _vertexBuffer.Offset, _vertexBuffer.Size);
            pass.SetIndexBuffer(_indexBuffer.Buffer, IndexFormat.Uint16, _indexBuffer.Offset, _indexBuffer.Size);
            pass.SetPipeline(_pipeline);
            pass.SetBindGroup(0, _mainBindGroup, dynamicOffsets: null);

            draw_data.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdLists[n];
                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    var pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallbackData != IntPtr.Zero.ToPointer())
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if (pcmd.TexRef.TexID != IntPtr.Zero)
                        {
                            if (pcmd.TexRef.TexID == _fontAtlasID)
                            {
                                pass.SetBindGroup(1, _fontTextureResourceSet!.Value, dynamicOffsets: null);
                            }
                            else
                            {
                                var textureView = new TextureViewPtr(_api.Device.GetAPI(), (TextureView*)pcmd.TexRef.TexID);

                                if (!_bindGroupsByView.TryGetValue(textureView, out BindGroupPtr bindGroup))
                                    bindGroup = CreateTextureBindGroupInternal(textureView);

                                pass.SetBindGroup(1, bindGroup, dynamicOffsets: null);
                            }
                        }

                        var io = ImGui.GetIO();
                        var framebufferScale = io.DisplayFramebufferScale;
                        var scissorRectWidth  = (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X * framebufferScale.X);
                        var scissorRectHeight = (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y * framebufferScale.Y);

                        if (scissorRectWidth <= 0 || scissorRectHeight <= 0)
                            continue;

                        pass.SetScissorRect(
                            (uint)(pcmd.ClipRect.X * framebufferScale.X),
                            (uint)(pcmd.ClipRect.Y * framebufferScale.Y),
                            scissorRectWidth,
                            scissorRectHeight
                            );

                        pass.DrawIndexed(pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)idx_offset, (int)(pcmd.VtxOffset + vtx_offset), 0);
                    }
                }

                idx_offset += cmd_list.IdxBuffer.Size;
                vtx_offset += cmd_list.VtxBuffer.Size;
            }
        }

        public void Dispose()
        {
            _vertexBuffer.Buffer.Destroy();
            _indexBuffer.Buffer.Destroy();
            _projMatrixBuffer.Destroy();
            _fontTexture?.Destroy();
            _shaderModule.Release();
            _layout.Release();
            _textureLayout.Release();
            _pipeline.Release();
            _mainBindGroup.Release();
            _fontTextureResourceSet?.Release();

            foreach (BindGroupPtr bindGroup in _bindGroupsByView.Values)
            {
                bindGroup.Release();
            }
            
            _platform.Dispose();
        }
    }
}

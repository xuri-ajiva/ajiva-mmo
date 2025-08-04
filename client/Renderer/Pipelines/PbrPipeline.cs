using System.Runtime.InteropServices;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Renderer.VirtualFramebuffers;
using Client.Wgsl;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using Safe = Silk.NET.WebGPU.Safe;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;
using RenderPassDepthStencilAttachment = Silk.NET.WebGPU.Safe.RenderPassDepthStencilAttachment;

namespace Client.Renderer.Pipelines;

public class PbrPipeline : IDisposable
{
    private readonly RendererApi _api;
    private readonly ResourceManager _loader;
    private readonly GBuffer _gBuffer;
    private readonly IPbrGBufferShader _gBufferShader;
    private readonly IPbrDeferredLightingShader _deferredShader;

    private readonly RenderPipelinePtr _gBufferPipeline;
    private readonly RenderPipelinePtr _deferredLightingPipeline;
    private readonly BindGroupLayoutPtr _gBufferSceneLayout; // Group 0 (GBuffer Pass)
    private readonly BindGroupLayoutPtr _gBufferMaterialLayout; // Group 1 (GBuffer Pass - Material Uniform)
    private readonly BindGroupLayoutPtr _gBufferTextureLayout; // Group 2 (GBuffer Pass - Textures)
    private readonly BindGroupLayoutPtr _deferredSceneLayout; // Group 0 (Deferred Pass)
    private readonly BindGroupLayoutPtr _deferredTextureLayout; // Group 1 (Deferred Pass - GBuffer Textures)
    private readonly SamplerPtr _materialSampler; // Linear/Aniso for materials

    private readonly AjivaBuffer<PbrGBufferShader.SceneUniform> _gBufferSceneUniformBuffer;
    private readonly AjivaBuffer<PbrDeferredLightingShader.SceneUniforms> _deferredSceneUniformBuffer;

    private readonly BindGroupPtr _gBufferSceneBindGroup; // Group 0 (GBuffer)
    private BindGroupPtr? _deferredSceneBindGroup; // Group 0 (Deferred) - Updated per frame
    private BindGroupPtr? _deferredTextureBindGroup; // Group 1 (Deferred) - Updated per frame


    private PbrPipeline(
        RendererApi api, ResourceManager loader, GBuffer gBuffer, IPbrGBufferShader gBufferShader,
        IPbrDeferredLightingShader deferredShader, RenderPipelinePtr gBufferPipeline,
        RenderPipelinePtr deferredLightingPipeline, BindGroupLayoutPtr gBufferSceneLayout,
        BindGroupLayoutPtr gBufferMaterialLayout, BindGroupLayoutPtr gBufferTextureLayout,
        BindGroupLayoutPtr deferredSceneLayout, BindGroupLayoutPtr deferredTextureLayout, SamplerPtr materialSampler,
        AjivaBuffer<PbrGBufferShader.SceneUniform> gBufferSceneUniformBuffer,
        AjivaBuffer<PbrDeferredLightingShader.SceneUniforms> deferredSceneUniformBuffer,
        BindGroupPtr gBufferSceneBindGroup)
    {
        _api = api;
        _loader = loader;
        _gBuffer = gBuffer;
        _gBufferShader = gBufferShader;
        _deferredShader = deferredShader;
        _gBufferPipeline = gBufferPipeline;
        _deferredLightingPipeline = deferredLightingPipeline;
        _gBufferSceneLayout = gBufferSceneLayout;
        _gBufferMaterialLayout = gBufferMaterialLayout;
        _gBufferTextureLayout = gBufferTextureLayout;
        _deferredSceneLayout = deferredSceneLayout;
        _deferredTextureLayout = deferredTextureLayout;
        _materialSampler = materialSampler;
        _gBufferSceneUniformBuffer = gBufferSceneUniformBuffer;
        _deferredSceneUniformBuffer = deferredSceneUniformBuffer;
        _gBufferSceneBindGroup = gBufferSceneBindGroup;
    }

    public static PbrPipeline Create(RendererApi api, ResourceManager loader, IPbrGBufferShader gBufferShader,
        IPbrDeferredLightingShader deferredShader, GBuffer gBuffer)
    {
        var device = api.Device;

        var gBufferSceneLayout = gBufferShader.CreateBindGroupLayout0(device);
        var gBufferMaterialLayout = gBufferShader.CreateBindGroupLayout1(device);
        var gBufferTextureLayout = gBufferShader.CreateBindGroupLayout2(device);
        var gBufferPipelineLayout =
            gBufferShader.CreatePipelineLayout(device, gBufferSceneLayout, gBufferMaterialLayout, gBufferTextureLayout);

        var deferredSceneLayout = deferredShader.CreateBindGroupLayout0(device, lightsBufferMinSize: 0);
        var deferredTextureLayout = deferredShader.CreateBindGroupLayout1(device);
        var deferredPipelineLayout =
            deferredShader.CreatePipelineLayout(device, deferredSceneLayout, deferredTextureLayout);

        var materialSampler = device.CreateSampler(AddressMode.Repeat, AddressMode.Repeat, AddressMode.ClampToEdge,
            FilterMode.Linear, FilterMode.Linear, MipmapFilterMode.Linear, // Consider Anisotropy
            lodMinClamp: 0, lodMaxClamp: 16, compare: CompareFunction.Undefined);
        /*var gBufferSampler = device.CreateSampler(AddressMode.ClampToEdge, AddressMode.ClampToEdge,
            AddressMode.ClampToEdge, FilterMode.Nearest, FilterMode.Nearest, MipmapFilterMode.Nearest,
            lodMinClamp: 0, lodMaxClamp: 1, compare: CompareFunction.Undefined); */

        var gBufferSceneUniformBuffer = loader.CreateBuffer<PbrGBufferShader.SceneUniform>(
            BufferUsage.Uniform | BufferUsage.CopyDst, 1, "PBR GBuffer Scene Uniforms");
        var deferredSceneUniformBuffer = loader.CreateBuffer<PbrDeferredLightingShader.SceneUniforms>(
            BufferUsage.Uniform | BufferUsage.CopyDst, 1, "PBR Deferred Scene Uniforms");

        var gBufferSceneBindGroup =
            gBufferShader.CreateBindGroup0(device, gBufferSceneLayout, gBufferSceneUniformBuffer.Buffer);

        var gBufferModule = gBufferShader.CreateShaderModule(device, gBufferPipelineLayout);
        var deferredModule = deferredShader.CreateShaderModule(device, deferredPipelineLayout);

        var gBufferPipeline = device.CreateRenderPipeline(
            label: "PBR_GBufferPipeline",
            layout: gBufferPipelineLayout,
            vertex: new Safe.VertexState
            {
                Module = gBufferModule,
                EntryPoint = gBufferShader.VertexEntryPoint,
                Buffers = gBufferShader.GetVertexBufferLayout(VertexStepMode.Vertex,
                    VertexStepMode.Instance), // Assuming shader provides this
                Constants = []
            },
            primitive: new Safe.PrimitiveState
            {
                Topology = PrimitiveTopology.TriangleList,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.Back // Standard culling for opaque
            },
            depthStencil: new DepthStencilState
            {
                Format = gBuffer.DepthStencilFormat,
                DepthCompare = CompareFunction.LessEqual,
                DepthWriteEnabled = true,
                StencilFront = new StencilFaceState { Compare = CompareFunction.Always }, // Configure stencil if needed
                StencilBack = new StencilFaceState { Compare = CompareFunction.Always }
            },
            multisample: new MultisampleState { Count = 1, Mask = u32.MaxValue },
            fragment: new Safe.FragmentState
            {
                Module = gBufferModule,
                EntryPoint = gBufferShader.FragmentEntryPoint,
                // Define G-Buffer outputs using formats from the GBuffer object
                Targets = gBufferShader.ColorTarget(
                    baseColorAlpha: new Safe.ColorTargetState(gBuffer.BaseColorAlphaFormat, null, ColorWriteMask.All),
                    worldNormal: new Safe.ColorTargetState(gBuffer.WorldNormalFormat, null, ColorWriteMask.All),
                    metallicRoughnessAO: new Safe.ColorTargetState(gBuffer.MetallicRoughnessAoFormat, null,
                        ColorWriteMask.All),
                    emissive: new Safe.ColorTargetState(gBuffer.EmissiveFormat, null, ColorWriteMask.All),
                    objectAndInstanceId: new Safe.ColorTargetState(gBuffer.ObjectAndInstanceIdFormat, null,
                        ColorWriteMask.All)
                ),
                Constants = []
            }
        );

        var deferredLightingPipeline = device.CreateRenderPipeline(
            label: "PBR_DeferredLightingPipeline",
            layout: deferredPipelineLayout,
            vertex: new Safe.VertexState
            {
                Module = deferredModule,
                EntryPoint = deferredShader.VertexEntryPoint,
                Buffers = [],
                Constants = []
            },
            primitive: new Safe.PrimitiveState
            {
                Topology = PrimitiveTopology.TriangleList,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.None // No culling for fullscreen pass
            },
            depthStencil: null,
            multisample: new MultisampleState { Count = 1, Mask = u32.MaxValue },
            fragment: new Safe.FragmentState
            {
                Module = deferredModule,
                EntryPoint = deferredShader.FragmentEntryPoint,
                Targets =
                [
                    new Safe.ColorTargetState(gBuffer.EmissiveFormat, blendState: (
                        color: new BlendComponent(BlendOperation.Add, BlendFactor.One, BlendFactor.One),
                        alpha: new BlendComponent(BlendOperation.Add, BlendFactor.One, BlendFactor.Zero)
                    ), writeMask: ColorWriteMask.All)
                ],
                Constants = []
            }
        );

        // Release intermediate resources
        gBufferModule.Release();
        deferredModule.Release();
        gBufferPipelineLayout.Release();
        deferredPipelineLayout.Release();

        return new PbrPipeline(api, loader, gBuffer, gBufferShader, deferredShader, gBufferPipeline,
            deferredLightingPipeline, gBufferSceneLayout, gBufferMaterialLayout, gBufferTextureLayout,
            deferredSceneLayout, deferredTextureLayout, materialSampler, gBufferSceneUniformBuffer,
            deferredSceneUniformBuffer, gBufferSceneBindGroup);
    }

    public void WriteGBufferSceneUniforms(QueuePtr queue, PbrGBufferShader.SceneUniform sceneUniform)
    {
        queue.WriteBuffer(_gBufferSceneUniformBuffer.Buffer, _gBufferSceneUniformBuffer.OffsetBytes,
            new ReadOnlySpan<PbrGBufferShader.SceneUniform>(ref sceneUniform));
    }

    public void WriteDeferredSceneUniforms(QueuePtr queue, PbrDeferredLightingShader.SceneUniforms sceneUniform)
    {
        queue.WriteBuffer(_deferredSceneUniformBuffer.Buffer, _deferredSceneUniformBuffer.OffsetBytes,
            new ReadOnlySpan<PbrDeferredLightingShader.SceneUniforms>(ref sceneUniform));
    }

    public void RecreateMaterialBindGroup(PbrInstancedMeshBuffer materialData)
    {
        var textureHash = HashCode.Combine(_gBufferTextureLayout, materialData.BaseColorTexture?.Version ?? 0,
            materialData.NormalTexture?.Version ?? 0, materialData.MetallicRoughnessAoTexture?.Version ?? 0,
            materialData.EmissiveTexture?.Version ?? 0);

        if (materialData.TextureBindGroupHash != textureHash)
        {
            materialData.TextureBindGroupHash = textureHash;
            materialData.TextureBindGroup?.Release();

            materialData.TextureBindGroup = _gBufferShader.CreateBindGroup2(_api.Device,
                _gBufferTextureLayout,
                _materialSampler,
                materialData.BaseColorTexture?.View ?? _loader.DefaultTexture.View,
                materialData.NormalTexture?.View ?? _loader.DefaultNormal.View,
                materialData.MetallicRoughnessAoTexture?.View ?? _loader.DefaultMetallicRoughnessAo.View,
                materialData.EmissiveTexture?.View ?? _loader.DefaultEmissive.View
                // Add occlusion texture view if separate
            );
        }

        var materialHash = HashCode.Combine(_gBufferMaterialLayout, materialData.MaterialUniformBuffer.Version);

        if (materialData.MaterialBindGroupHash != materialHash)
        {
            materialData.MaterialBindGroupHash = materialHash;
            materialData.MaterialBindGroup?.Release();

            materialData.MaterialBindGroup = _gBufferShader.CreateBindGroup1(_api.Device,
                _gBufferMaterialLayout,
                materialData.MaterialUniformBuffer.Buffer,
                ubMaterialOffset: materialData.MaterialUniformBuffer.OffsetBytes,
                ubMaterialSize: materialData.MaterialUniformBuffer.Size
            );
        }
    }


    private CommandEncoderPtr _cmdEncoder;

    public RenderPassEncoderPtr? BeginGBufferPass()
    {
        // Get the G-Buffer texture views for the current frame
        if (!_gBuffer.TryGetPbrGBufferPassViews(out var baseColor, out var normal, out var mrao,
                out var emissive, out var objectId, out var depthStencil))
        {
            return null;
        }

        _cmdEncoder = _api.Device.CreateCommandEncoder(label: "PBR_GBuffer_Encoder");

        var pass = _cmdEncoder.BeginRenderPass(
            label: "PBR_GBuffer_Pass",
            colorAttachments:
            [
                new RenderPassColorAttachment(baseColor.View, null, LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0, 0)),
                new RenderPassColorAttachment(normal.View, null, LoadOp.Clear, StoreOp.Store,
                    new Color(0.5, 0.5, 0.5, 0)),
                new RenderPassColorAttachment(mrao.View, null, LoadOp.Clear, StoreOp.Store,
                    new Color(0, 0.5, 1, 0)), // Clear M=0, R=0.5, AO=1
                new RenderPassColorAttachment(emissive.View, null, LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0, 0)),
                new RenderPassColorAttachment(objectId.View, null, LoadOp.Clear, StoreOp.Store, new Color(0, 0, 0, 0))
            ],
            depthStencilAttachment: new RenderPassDepthStencilAttachment(
                view: depthStencil.View,
                depthLoadOp: LoadOp.Clear, depthStoreOp: StoreOp.Store, depthClearValue: 1.0f, depthReadOnly: false,
                stencilLoadOp: LoadOp.Undefined, stencilStoreOp: StoreOp.Discard, stencilClearValue: 0,
                stencilReadOnly: true // Adjust stencil if used
            ),
            occlusionQuerySet: null, maxDrawCount: null, timestampWrites: null
        );

        pass.SetPipeline(_gBufferPipeline);
        pass.SetBindGroup(0, _gBufferSceneBindGroup, ReadOnlySpan<u32>.Empty);
        return pass;
    }

    public void DrawMesh(RenderPassEncoderPtr pass, PbrInstancedMeshBuffer instance)
    {
        if (instance.InstanceInfos.Length == 0) return;

        RecreateMaterialBindGroup(instance);

        var (vertexBuffer, indexBuffer, instanceBuffer, textureBindGroup, materialBindGroup, instances) = instance;
        if (textureBindGroup is null || materialBindGroup is null)
        {
            // Handle error: bind group creation failed?
            _loader.Logger.LogWarning("Failed to create/retrieve material bind group for mesh.");
            return;
        }


        pass.SetBindGroup(1, materialBindGroup.Value, ReadOnlySpan<u32>.Empty);
        pass.SetBindGroup(2, textureBindGroup.Value, ReadOnlySpan<u32>.Empty);


        pass.SetVertexBuffer(0, vertexBuffer.Buffer, vertexBuffer.OffsetBytes, vertexBuffer.Size);
        pass.SetVertexBuffer(1, instanceBuffer!.Buffer, instanceBuffer.OffsetBytes, instanceBuffer.Size);

        pass.SetIndexBuffer(indexBuffer.Buffer, IndexFormat.Uint32, indexBuffer.OffsetBytes, indexBuffer.Size);
        pass.DrawIndexed((u32)(indexBuffer.Size / sizeof(u32)), (u32)instances.Count, 0, 0, 0);
    }


    public CommandBufferPtr EndGBufferPass(RenderPassEncoderPtr pass)
    {
        pass.End();
        var cmdBuf = _cmdEncoder.Finish(label: "PBR_GBuffer_CommandBuffer");
        _cmdEncoder.Release();
        _cmdEncoder = default;
        return cmdBuf;
    }


    public void ApplyDeferredLighting(QueuePtr queue, BufferPtr lightsBuffer, u32 lightCount)
    {
        // Get G-Buffer views needed as input for the lighting pass
        if (!_gBuffer.TryGetPbrDeferredInputViews(out var baseColor, out var normal, out var mrao,
                out var emissive, out var depth))
        {
            _loader.Logger.LogInformation("Warning: GBuffer views not available for deferred lighting pass.");
            return;
        }

        _deferredSceneBindGroup?.Release();
        _deferredTextureBindGroup?.Release();

        // Create BindGroup 0 (Scene Uniforms, Lights)
        _deferredSceneBindGroup = _deferredShader.CreateBindGroup0(_api.Device, _deferredSceneLayout,
            _deferredSceneUniformBuffer.Buffer, lightsBuffer, ubSceneOffset: _deferredSceneUniformBuffer.OffsetBytes,
            ubSceneSize: _deferredSceneUniformBuffer.Size,
            lightsBufferSize: (u64)Marshal.SizeOf<PbrDeferredLightingShader.LightData>() * lightCount);

        // Create BindGroup 1 (G-Buffer Textures + Sampler)
        _deferredTextureBindGroup = _deferredShader.CreateBindGroup1(_api.Device, _deferredTextureLayout,
            baseColor.View, normal.View, mrao.View, depth.View // Pass IBL textures here if used
        );

        var cmd = _api.Device.CreateCommandEncoder(label: "PBR_Deferred_Encoder");
        var pass = cmd.BeginRenderPass(
            label: "PBR_Deferred_Pass",
            colorAttachments:
            [
                new RenderPassColorAttachment(emissive.View, null, LoadOp.Load, StoreOp.Store, default)
            ],
            depthStencilAttachment: null,
            occlusionQuerySet: null, maxDrawCount: null, timestampWrites: null
        );

        pass.SetPipeline(_deferredLightingPipeline);
        pass.SetBindGroup(0, _deferredSceneBindGroup.Value, ReadOnlySpan<u32>.Empty);
        pass.SetBindGroup(1, _deferredTextureBindGroup.Value, ReadOnlySpan<u32>.Empty);
        pass.Draw(3, 1, 0, 0); // Draw fullscreen triangle
        pass.End();

        queue.Submit(cmd.Finish(label: "PBR_Deferred_CommandBuffer"));
        cmd.Release();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        // Release owned resources
        _deferredSceneBindGroup?.Release();
        _deferredTextureBindGroup?.Release();
        _gBufferSceneBindGroup.Release(); // Was created with layout
        _gBufferPipeline.Release();
        _deferredLightingPipeline.Release();
        _gBufferSceneLayout.Release();
        _gBufferMaterialLayout.Release();
        _gBufferTextureLayout.Release();
        _deferredSceneLayout.Release();
        _deferredTextureLayout.Release();
        _materialSampler.Release();
    }

    public bool IsUpToDate(IPbrGBufferShader gBufferShader, IPbrDeferredLightingShader deferredShader)
    {
        return _gBufferShader == gBufferShader && _deferredShader == deferredShader;
    }
}

using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Renderer.VirtualFramebuffers;
using Client.Wgsl;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;
using Safe = Silk.NET.WebGPU.Safe;
using RenderPassColorAttachment = Silk.NET.WebGPU.Safe.RenderPassColorAttachment;

namespace Client.Renderer.Pipelines;

public class GizmoPipeline
{
    private readonly RendererApi _api;
    private readonly ResourceManager _loader;
    private readonly GBuffer _gBuffer;
    private readonly IGizmoShader _shader;

    private readonly RenderPipelinePtr _pipeline;
    private readonly BindGroupLayoutPtr _sceneLayout; // Group 0

    private readonly AjivaBuffer<GizmoShader.SceneUniform> _sceneUniformBuffer;
    private readonly BindGroupPtr _sceneBindGroup; // Group 0

    private GizmoPipeline(
        RendererApi api, ResourceManager loader, GBuffer gBuffer, IGizmoShader shader,
        RenderPipelinePtr pipeline, BindGroupLayoutPtr sceneLayout,
        AjivaBuffer<GizmoShader.SceneUniform> sceneUniformBuffer,
        BindGroupPtr sceneBindGroup)
    {
        _api = api;
        _loader = loader;
        _gBuffer = gBuffer;
        _shader = shader;
        _pipeline = pipeline;
        _sceneLayout = sceneLayout;
        _sceneUniformBuffer = sceneUniformBuffer;
        _sceneBindGroup = sceneBindGroup;
    }

    public static GizmoPipeline Create(RendererApi api, ResourceManager loader, IGizmoShader shader, GBuffer gBuffer)
    {
        var device = api.Device;

        var sceneLayout = shader.CreateBindGroupLayout0(device);
        var pipelineLayout = shader.CreatePipelineLayout(device, sceneLayout);

        var sceneUniformBuffer = loader.CreateBuffer<GizmoShader.SceneUniform>(
            BufferUsage.Uniform | BufferUsage.CopyDst, 1, "Gizmo Scene Uniforms");

        var sceneBindGroup = shader.CreateBindGroup0(device, sceneLayout, sceneUniformBuffer.Buffer,
            ubSceneSize: sceneUniformBuffer.Size);

        var shaderModule = shader.CreateShaderModule(device, pipelineLayout);

        var pipeline = device.CreateRenderPipeline(
            label: "GizmoPipeline",
            layout: pipelineLayout,
            vertex: new Safe.VertexState
            {
                Module = shaderModule,
                EntryPoint = shader.VertexEntryPoint,
                Buffers = shader.GetVertexBufferLayout(VertexStepMode.Vertex, VertexStepMode.Instance),
                Constants = []
            },
            primitive: new Safe.PrimitiveState
            {
                Topology = PrimitiveTopology.TriangleList,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.None // Gizmos might be visible from inside
            },
            depthStencil: null, // Always on top, no depth test/write
            multisample: new MultisampleState { Count = 1, Mask = u32.MaxValue },
            fragment: new Safe.FragmentState
            {
                Module = shaderModule,
                EntryPoint = shader.FragmentEntryPoint,
                Targets = shader.ColorTarget(
                    color: new Safe.ColorTargetState(gBuffer.OverlayChannelFormat, blendState: (
                        color: new BlendComponent(BlendOperation.Add, BlendFactor.OneMinusSrcAlpha,
                            BlendFactor.OneMinusDstAlpha),
                        alpha: new BlendComponent(BlendOperation.Add, BlendFactor.One, BlendFactor.Zero)
                    ), writeMask: ColorWriteMask.All),
                    objectAndInstanceId: new Safe.ColorTargetState(gBuffer.ObjectAndInstanceIdFormat, blendState: null,
                        writeMask: ColorWriteMask.All)
                ),
                Constants = []
            }
        );

        shaderModule.Release();
        pipelineLayout.Release();

        return new GizmoPipeline(api, loader, gBuffer, shader, pipeline, sceneLayout, sceneUniformBuffer,
            sceneBindGroup);
    }

    public void WriteSceneUniforms(QueuePtr queue, GizmoShader.SceneUniform sceneUniform)
    {
        queue.WriteBuffer(_sceneUniformBuffer.Buffer, _sceneUniformBuffer.OffsetBytes,
            new ReadOnlySpan<GizmoShader.SceneUniform>(ref sceneUniform));
    }

    private CommandEncoderPtr _cmdEncoder;

    public RenderPassEncoderPtr? BeginGizmoPass(Extent3D sceneViewport)
    {
        if (!_gBuffer.TryGetOverlayPass(out var baseColor, out var objectId))
        {
            return null;
        }

        _cmdEncoder = _api.Device.CreateCommandEncoder("Gizmo Pass Encoder");

        var pass = _cmdEncoder.BeginRenderPass(
            colorAttachments:
            [
                new RenderPassColorAttachment { View = baseColor.View, LoadOp = LoadOp.Clear, StoreOp = StoreOp.Store },
                new RenderPassColorAttachment { View = objectId.View, LoadOp = LoadOp.Load, StoreOp = StoreOp.Store },
            ],
            timestampWrites: null,
            depthStencilAttachment: null, // No depth attachment
            occlusionQuerySet: null
        );
        pass.SetViewport(0, 0, sceneViewport.Width, sceneViewport.Height, 0, 1);
        pass.SetPipeline(_pipeline);
        pass.SetBindGroup(0, _sceneBindGroup, ReadOnlySpan<u32>.Empty);
        return pass;
    }


    public void DrawGizmos(RenderPassEncoderPtr pass, SimpleInstancedMeshBBuffer<GizmoShader.Attributes, GizmoShader.Instance> instances)
    {
        pass.SetVertexBuffer(0, instances.Mesh.VertexBuffer.Buffer,
            instances.Mesh.VertexBuffer.OffsetBytes, instances.Mesh.VertexBuffer.Size);
        pass.SetVertexBuffer(1, instances.InstanceBuffer!.Buffer,
            instances.InstanceBuffer.OffsetBytes, instances.InstanceBuffer.Size);

        pass.SetIndexBuffer(instances.Mesh.IndexBuffer.Buffer, IndexFormat.Uint32,
            instances.Mesh.IndexBuffer.OffsetBytes, instances.Mesh.IndexBuffer.Size);

        pass.DrawIndexed((u32)(instances.Mesh.IndexBuffer.Size / sizeof(u32)),
            (u32)instances.InstanceInfos.Length, 0, 0, 0);
    }

    public CommandBufferPtr EndGizmoPass(RenderPassEncoderPtr pass)
    {
        pass.End();
        return _cmdEncoder.Finish("Gizmo Pass Encoder");
    }

    public void Dispose()
    {
        _pipeline.Release();
        _sceneLayout.Release();
        _sceneBindGroup.Release();
        _sceneUniformBuffer.Dispose();
    }

    public bool IsUpToDate(IGizmoShader shader)
    {
        return _shader == shader;
    }
}
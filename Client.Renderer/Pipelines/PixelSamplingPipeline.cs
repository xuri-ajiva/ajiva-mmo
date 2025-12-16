using System.Runtime.CompilerServices;
using Client.Wgsl;
using GlmSharp;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.Pipelines;

using static ObjectSamplingShader;

public class PixelSamplingPipeline : IDisposable
{
    private readonly RendererApi _api;
    private readonly IObjectSamplingShader _shader;
    private readonly PipelineLayoutPtr _pipelineLayout;
    private readonly ComputePipelinePtr _computePipeline;

    private readonly BufferPtr _inputBuffer;
    private readonly BufferPtr _resultStorageBuffer;
    private readonly BufferPtr _resultStagingBuffer;
    private readonly BindGroupLayoutPtr _bindGroupLayout;
    private BindGroupPtr? _bindGroup;

    private PixelSamplingPipeline(RendererApi api, IObjectSamplingShader shader, ComputePipelinePtr computePipeline,
        PipelineLayoutPtr pipelineLayout, BufferPtr inputBuffer, BufferPtr resultStorageBuffer,
        BufferPtr resultStagingBuffer, BindGroupLayoutPtr bindGroupLayout)
    {
        _api = api;
        _shader = shader;
        _computePipeline = computePipeline;
        _pipelineLayout = pipelineLayout;
        _inputBuffer = inputBuffer;
        _resultStorageBuffer = resultStorageBuffer;
        _resultStagingBuffer = resultStagingBuffer;
        _bindGroupLayout = bindGroupLayout;
    }

    public static PixelSamplingPipeline Create(RendererApi api, IObjectSamplingShader shader)
    {
        var device = api.Device;


        var inputBuffer =
            device.CreateBuffer(BufferUsage.CopyDst | BufferUsage.Uniform, (u32)Unsafe.SizeOf<Input>());

        // todo warp double buffer
        var resultStorageBuffer = device.CreateBuffer(BufferUsage.Storage | BufferUsage.CopySrc,
            (u32)Unsafe.SizeOf<Result>(),
            label: "PixelReader.ResultStorageBuffer");
        var resultStagingBuffer = device.CreateBuffer(BufferUsage.CopyDst | BufferUsage.MapRead,
            (u32)Unsafe.SizeOf<Result>(),
            label: "PixelReader.ResultStagingBuffer");

        var bindGroupLayout = shader.CreateBindGroupLayout0(device);

        var pipelineLayout = shader.CreatePipelineLayout(device, bindGroupLayout);

        var module = shader.CreateShaderModule(device, pipelineLayout);

        var computePipeline = device.CreateComputePipeline(layout: pipelineLayout,
            stage: new ProgrammableStage
            {
                Constants = [],
                Module = module,
                EntryPoint = "main"
            });

        module.Release();
        pipelineLayout.Release();
        return new PixelSamplingPipeline(api, shader, computePipeline, pipelineLayout, inputBuffer,
            resultStorageBuffer, resultStagingBuffer, bindGroupLayout);
    }

    public void DispatchPixelRead(QueuePtr queue, uvec2 pixelPosition, TextureViewPtr depthTexture,
        TextureViewPtr objecTextureViewPtr)
    {
        var ubInput = new Input
        {
            PixelPos = pixelPosition,
        };

        queue.WriteBuffer(_inputBuffer, 0, new ReadOnlySpan<Input>(ref ubInput));

        _bindGroup?.Release();
        _bindGroup = _shader.CreateBindGroup0(_api.Device, _bindGroupLayout, _inputBuffer, _resultStorageBuffer,
            depthTexture, objecTextureViewPtr);

        var cmd = _api.Device.CreateCommandEncoder("PixelReaderCommandEncoder");
        var pass = cmd.BeginComputePass(null, "PixelReaderPass");
        pass.SetPipeline(_computePipeline);
        pass.SetBindGroup(0, _bindGroup.Value, null);
        pass.DispatchWorkgroups(1, 1, 1);
        pass.End();

        cmd.CopyBufferToBuffer(_resultStorageBuffer, 0, _resultStagingBuffer, 0, (u32)Unsafe.SizeOf<Result>());

        queue.Submit(cmd.Finish());
    }

    public Task RequestResultBufferMapping()
    {
        return _resultStagingBuffer.MapAsync(MapMode.Read, 0, (u32)Unsafe.SizeOf<Result>());
    }

    public Result? ReadResult(Task? mappingTask)
    {
        if (mappingTask is not { IsCompleted: true, IsFaulted: false })
            return null;
        try
        {
            var resultSpan = _resultStagingBuffer
                .GetConstMappedRange<Result>(0, (u32)Unsafe.SizeOf<Result>());
            var result = resultSpan[0];
            _resultStagingBuffer.Unmap();
            return result;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _bindGroup?.Release();
        _computePipeline.Release();
        _bindGroupLayout.Release();
        _inputBuffer.Destroy();
        _resultStorageBuffer.Destroy();
        _resultStagingBuffer.Destroy();
    }

    public bool IsUpToDate(IObjectSamplingShader objectSamplingShader) => _shader == objectSamplingShader;
}

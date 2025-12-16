using Silk.NET.WebGPU.Safe;

namespace Client.Renderer.PbrRenderer;

public record struct BufferRange(BufferPtr Buffer, u64 Offset, u64 Size);
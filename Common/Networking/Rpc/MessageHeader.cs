using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Common.Networking.Rpc;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MessageHeader
{
    public u16 MethodId;
    public u32 PayloadLength;
    public static i32 SizeOf = Unsafe.SizeOf<MessageHeader>();

    public void WriteTo(PipeWriter bufferWriter)
    {
        // TODO use VAR INT encoding
        /*bufferWriter.PackVarU64(MethodId);
        bufferWriter.PackVarU64(PayloadLength);*/
        Span<u8> buffer = stackalloc u8[SizeOf];
        MemoryMarshal.Write(buffer, in this);
        bufferWriter.Write(buffer);
    }

    public static bool TryReadFrom(ReadOnlySequence<u8> readOnlySequence,
        out MessageHeader messageHeader, out i32 bytesRead)
    {
        messageHeader = default;
        bytesRead = 0;
        if (readOnlySequence.Length < SizeOf) return false;
        Span<u8> buffer = stackalloc u8[SizeOf];
        readOnlySequence.Slice(0, SizeOf).CopyTo(buffer);
        bytesRead = SizeOf;
        return MemoryMarshal.TryRead(buffer, out messageHeader);
    }
}
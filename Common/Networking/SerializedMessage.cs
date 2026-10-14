using MemoryPack;
using MemoryPack.Internal;

namespace Common.Networking;

public ref struct SerializedMessage() : IDisposable
{
    public ReusableLinkedArrayBufferWriter Writer = ReusableLinkedArrayBufferWriterPool.Rent();
    public readonly MemoryPackWriterOptionalState State = MemoryPackWriterOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);

    public void Dispose()
    {
        ReusableLinkedArrayBufferWriterPool.Return(Writer);
        ((IDisposable)State).Dispose();
    }
              
    public ReusableLinkedArrayBufferWriter.Enumerator GetEnumerator()
    {
        return new ReusableLinkedArrayBufferWriter.Enumerator(Writer);
    }
              
    public ulong Length => (ulong)Writer.TotalWritten;
}
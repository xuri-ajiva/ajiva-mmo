namespace Client.Renderer.Resources;

interface IAjivaVersionedObject : IDisposable
{
    u64 Version { get; }

    const u64 Sentinel = 0ul;
}
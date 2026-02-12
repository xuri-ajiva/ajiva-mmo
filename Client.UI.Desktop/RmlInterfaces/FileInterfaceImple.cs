using Common.Ecs;
using RmlUiNet;

namespace Client.UI.Desktop.RmlInterfaces;

internal class FileInterfaceImple : FileInterface
{
    private FileStream[] _openFiles = [];
    private FreeList _freeList = new(2);

    public override IntPtr Open(string path)
    {
        var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        var handle = _freeList.GetFreeId();
        lock (this)
        {
            Array.Resize(ref _openFiles, handle + 1);
        }

        _openFiles[handle] = fs;
        return handle;
    }
    
    public override void Close(IntPtr file)
    {
        var handle = file.ToInt32();
        lock (this)
        {
            _openFiles[handle]?.Dispose();
            _openFiles[handle] = null;
            _freeList.ReleaseId(handle);
        }
    }

    public override u64 Read(u64 size, IntPtr file, out byte[] bytes)
    {
        var handle = file.ToInt32();
        var fs = _openFiles[handle];
        bytes = new byte[size];
        var read = fs.Read(bytes, 0, (i32)size);
        return (u64)read;
    }

    public override bool Seek(IntPtr file, u32 offset, i32 origin)
    {
        var handle = file.ToInt32();
        var fs = _openFiles[handle];
        SeekOrigin seekOrigin = origin switch
        {
            0 => SeekOrigin.Begin,
            1 => SeekOrigin.Current,
            2 => SeekOrigin.End,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), "Invalid seek origin")
        };
        fs.Seek(offset, seekOrigin);
        return true;
    }

    public override u64 Tell(IntPtr file)
    {
        var handle = file.ToInt32();
        var fs = _openFiles[handle];
        return (u64)fs.Position;
    }

    public override u64 Length(IntPtr file)
    {
        var handle = file.ToInt32();
        var fs = _openFiles[handle];
        return (u64)fs.Length;
    }

    public override string LoadFile(string path)
    {
        return File.ReadAllText(path);
    }
}
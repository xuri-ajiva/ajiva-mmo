using System.Diagnostics;

namespace Composer.ProcessOrchestrator;

public class ProcessConfiguration
{
    public int Port { get; set; }

    public string ExecutablePath { get; set; } = Process.GetCurrentProcess().MainModule?.FileName ?? "compositor.exe";
    public List<ConfigurationBase> Configuration { get; set; } = new();
}

public class ClientConfiguration : ConfigurationBase
{
    public WindowConfiguration? WindowConfiguration { get; set; }
}

public class ConfigurationBase
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ServerConfiguration : ConfigurationBase
{
    public int MaxConnections { get; set; }
}

public class WindowConfiguration
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Fullscreen { get; set; }
    public int Monitor { get; set; } = 0;
    public WindowSplitMode SplitMode { get; set; } = WindowSplitMode.None;
}

public enum WindowSplitMode
{
    None,
    LeftHalf,
    RightHalf,
    TopHalf,
    BottomHalf,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

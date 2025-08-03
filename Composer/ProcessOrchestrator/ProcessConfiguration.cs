using System.Diagnostics;

namespace Composer.ProcessOrchestrator;

public class ProcessConfiguration
{
    public string ExecutablePath { get; set; } = Process.GetCurrentProcess().MainModule?.FileName ?? "compositor.exe";
    public int BaseDebugPort { get; set; } = 5000;
    public List<ClientConfiguration> ClientConfigurations { get; set; } = new();
    public List<ServerConfiguration> ServerConfigurations { get; set; } = new();
}

public class ClientConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public WindowConfiguration WindowConfiguration { get; set; } = new();
}

public class ServerConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Port { get; set; }
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


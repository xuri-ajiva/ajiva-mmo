namespace Client.Config;

public class WindowConfig
{
    public string Title { get; set; } = "Ajiva MMO";
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public bool VSync { get; set; } = true;
    public bool Resizable { get; set; } = true;
    public bool FullscreenBorderless { get; set; } = false;
}

namespace Client.Config;

public class WindowConfig
{
    public string Title { get; set; } = "Ajiva MMO";
    public i32? Width { get; set; } = 1920;
    public i32? Height { get; set; } = 1080;
    public i32? X { get; set; } = 0;
    public i32? Y { get; set; } = 0;
    public bool VSync { get; set; } = true;
    public bool Resizable { get; set; } = true;
    public bool FullscreenBorderless { get; set; } = false;
}

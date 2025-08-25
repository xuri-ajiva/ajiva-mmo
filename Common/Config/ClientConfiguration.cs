namespace Common.Config;

public class ClientConfiguration
{
    public bool HeadLess { get; set; } = false;
    public bool AutoConnect { get; set; } = false;
    public string? AutoConnectAddress { get; set; }
    public string? WindowConfiguration { get; set; }
}

namespace Client.Platform;

public class PlatformSystem
{
    private readonly ILogger<PlatformSystem> _logger;

    public PlatformSystem(ILogger<PlatformSystem> logger)
    {
        _logger = logger;
    }

    public void Init()
    {
        Silk.NET.Input.Glfw.GlfwInput.RegisterPlatform();
        Silk.NET.Windowing.Glfw.GlfwWindowing.Use();
        _logger.LogInformation("PlatformSystem Init");
    }

    public void Shotdown()
    {
        _logger.LogInformation("PlatformSystem Shotdown");
    }
}

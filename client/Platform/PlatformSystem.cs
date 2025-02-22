namespace client.Platform;

public class PlatformSystem
{
    private readonly ILogger<PlatformSystem> _logger;

    public PlatformSystem(ILogger<PlatformSystem> logger)
    {
        _logger = logger;
    }

    public void Init()
    {
        _logger.LogInformation("PlatformSystem Init");
    }

    public void Shotdown()
    {
        _logger.LogInformation("PlatformSystem Shotdown");
    }
}

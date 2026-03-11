using Common.Systems;
using Microsoft.Extensions.Logging;

namespace Client.UI.Desktop;

/// <summary>
/// Example game HUD layer for desktop
/// Shows how to create a custom UI layer using the UI engine
/// </summary>
public class ExampleGameHudLayer : IUiRenderLayer
{
    private readonly ILogger<ExampleGameHudLayer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUiTextureRenderer _renderer;
    private i32 _playerHealth = 100;
    private i32 _playerScore = 0;
    private float _updateTimer = 0f;

    public ExampleGameHudLayer(
        ILogger<ExampleGameHudLayer> logger,
        IServiceProvider serviceProvider,
        IUiTextureRenderer renderer)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _renderer = renderer;
    }

    public void Attach()
    {
        _logger.LogInformation("Attaching Game HUD");

        // Load the HUD HTML
        _renderer.LoadHTML("""
                           <div id='hud-container'>
                               <div class='stat-panel top-left'>
                                   <div id='health' class='stat-item'>Health: 100</div>
                                   <div id='mana' class='stat-item'>Mana: 100</div>
                               </div>
                               
                               <div class='stat-panel top-right'>
                                   <div id='score' class='stat-item'>Score: 0</div>
                                   <div id='fps' class='stat-item'>FPS: 60</div>
                               </div>
                               
                               <div class='center-message' id='centerMessage' style='display: none;'>
                                   <h1>Game Message</h1>
                               </div>
                               
                               <div class='bottom-bar'>
                                   <div class='hotbar'>
                                       <div class='hotbar-slot' data-slot='1'>1</div>
                                       <div class='hotbar-slot' data-slot='2'>2</div>
                                       <div class='hotbar-slot' data-slot='3'>3</div>
                                       <div class='hotbar-slot' data-slot='4'>4</div>
                                       <div class='hotbar-slot' data-slot='5'>5</div>
                                   </div>
                               </div>
                           </div>
                           """
        );

        // Apply styling
        LoadCSS();
    }

    public void Update(UpdateInfo info)
    {
    }

    public void Detach()
    {
    }

    public ref bool Enabled => ref _enabled;
    private bool _enabled = true;

    public void RenderUi(UpdateInfo info)
    {
        _updateTimer += info.DeltaTime;
        if (_updateTimer >= 1.0f)
        {
            _updateTimer = 0f;
            UpdateHudStats();
        }
    }

    private void UpdateHudStats()
    {
        // Example: Update health and score
        _playerScore += 10;

        _renderer.ExecuteJavaScript($@"
            if (document.getElementById('health')) {{
                document.getElementById('health').innerText = 'Health: {_playerHealth}';
                document.getElementById('score').innerText = 'Score: {_playerScore}';
            }}
        ");
    }

    public void SetHealth(i32 health)
    {
        _playerHealth = Math.Clamp(health, 0, 100);
        UpdateHudStats();
    }

    private void LoadCSS()
    {
        _renderer.ExecuteJavaScript(@"
const style = document.createElement('style');
style.textContent = `
    
`;
document.head.appendChild(style);
        ");
    }
}
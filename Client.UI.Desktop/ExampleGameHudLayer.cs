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
                           """,
            """
            * {
                    margin: 0;
                    padding: 0;
                    box-sizing: border-box;
                }
                
                body {
                    color: white;
                    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
                    overflow: hidden;
                }
                
                #hud-container {
                    width: 100vw;
                    height: 100vh;
                    position: relative;
                }
                
                .stat-panel {
                    position: absolute;
                    background: rgba(0, 0, 0, 0.5);
                    padding: 15px;
                    border-radius: 8px;
                    min-width: 200px;
                }
                
                .top-left {
                    top: 20px;
                    left: 20px;
                }
                
                .top-right {
                    top: 20px;
                    right: 20px;
                    text-align: right;
                }
                
                .stat-item {
                    font-size: 18px;
                    margin: 5px 0;
                    font-weight: bold;
                }
                
                .center-message {
                    position: absolute;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%);
                    background: rgba(0, 0, 0, 0.8);
                    padding: 30px;
                    border-radius: 10px;
                    text-align: center;
                }
                
                .bottom-bar {
                    position: absolute;
                    bottom: 20px;
                    left: 50%;
                    transform: translateX(-50%);
                }
                
                .hotbar {
                    display: flex;
                    gap: 10px;
                }
                
                .hotbar-slot {
                    width: 60px;
                    height: 60px;
                    background: rgba(0, 0, 0, 0.6);
                    border: 2px solid rgba(255, 255, 255, 0.3);
                    border-radius: 8px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    font-size: 20px;
                    font-weight: bold;
                    transition: all 0.2s;
                }
                
                .hotbar-slot:hover {
                    background: rgba(255, 255, 255, 0.2);
                    border-color: rgba(255, 255, 255, 0.6);
                    transform: scale(1.1);
                }
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
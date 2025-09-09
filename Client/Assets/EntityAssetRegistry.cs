using System.Numerics;
using Common.Ecs;

namespace Client.Assets;

/// <summary>
/// Maps entity types to their render assets.
/// This is client-side configuration that determines how entities should be rendered.
/// </summary>
public class EntityAssetRegistry
{
    private readonly Dictionary<u32, EntityAssetDefinition> _entityAssets = new();

    public record EntityAssetDefinition(
        string MeshPath,
        string MaterialPath,
        Vector3 Scale = default,
        Vector3 PositionOffset = default
    );

    public EntityAssetRegistry()
    {
        LoadDefaultAssets();
    }

    private void LoadDefaultAssets()
    {
        // Map entity types to their render assets
        // TODO: Load this from a config file or server-provided manifest
        
        // TypeId 1: Generic crate/box
        _entityAssets[1] = new EntityAssetDefinition(
            MeshPath: "meshes/crate.obj",
            MaterialPath: "materials/wood.mat",
            Scale: new Vector3(1.0f)
        );

        // TypeId 2: Player
        _entityAssets[2] = new EntityAssetDefinition(
            MeshPath: "meshes/player.obj", 
            MaterialPath: "materials/character.mat",
            Scale: new Vector3(1.0f)
        );

        // TypeId 3: Tree (example)
        _entityAssets[3] = new EntityAssetDefinition(
            MeshPath: "meshes/tree.obj",
            MaterialPath: "materials/bark.mat",
            Scale: new Vector3(2.0f),
            PositionOffset: new Vector3(0, -0.5f, 0)
        );
    }

    public EntityAssetDefinition? GetAssetDefinition(u32 entityTypeId)
    {
        return _entityAssets.TryGetValue(entityTypeId, out var definition) ? definition : null;
    }

    public bool HasAssetDefinition(u32 entityTypeId)
    {
        return _entityAssets.ContainsKey(entityTypeId);
    }

    /// <summary>
    /// Get all unique asset paths that might be needed for a set of entity types.
    /// Used for preloading assets when a chunk becomes visible.
    /// </summary>
    public IEnumerable<string> GetRequiredAssetPaths(IEnumerable<u32> entityTypes)
    {
        var paths = new HashSet<string>();
        
        foreach (var typeId in entityTypes)
        {
            if (_entityAssets.TryGetValue(typeId, out var asset))
            {
                paths.Add(asset.MeshPath);
                paths.Add(asset.MaterialPath);
            }
        }
        
        return paths;
    }

    /// <summary>
    /// Load asset definitions from a scene manifest or config file.
    /// This would be called when loading a new level.
    /// </summary>
    public void LoadSceneAssets(string sceneName)
    {
        // TODO: Load scene-specific asset mappings
        // For example, "Forest" scene might have different tree variants
        // "Dungeon" scene might have different monster types
        
        switch (sceneName.ToLower())
        {
            case "sponza":
                LoadSponzaAssets();
                break;
            case "forest":
                LoadForestAssets();
                break;
            default:
                // Keep default assets
                break;
        }
    }

    private void LoadSponzaAssets()
    {
        // Sponza-specific assets could override defaults
        _entityAssets[1] = new EntityAssetDefinition(
            MeshPath: "meshes/sponza_pillar.obj",
            MaterialPath: "materials/stone.mat"
        );
    }

    private void LoadForestAssets()
    {
        // Forest-specific assets
        _entityAssets[3] = new EntityAssetDefinition(
            MeshPath: "meshes/oak_tree.obj",
            MaterialPath: "materials/oak_bark.mat",
            Scale: new Vector3(3.0f)
        );
        
        _entityAssets[4] = new EntityAssetDefinition(
            MeshPath: "meshes/pine_tree.obj", 
            MaterialPath: "materials/pine_bark.mat",
            Scale: new Vector3(4.0f)
        );
    }
}

using System.Numerics;
using Ajiva.Ecs;
using MemoryPack;

namespace Common.Ecs;

/// <summary>
/// Portal component for 5D non-Euclidean routing
/// Allows viewing and traversing to different regions of the 4D space-time
/// at different scales and orientations
/// </summary>
[EcsComponent(ComponentScope.Shared, ComponentSync.ServerToClient,
    ComponentFlags.Sparse | ComponentFlags.ReliableSync)]
public partial struct Portal : IComponent
{
    // Target 4D coordinates
    public Vector3 TargetPosition; // X, Y, Z
    public i32 TargetW; // Target multiverse

    // Target tree level (allows zoom in/out through portals)
    public i32 TargetLevel;

    // Rotation matrix for orientation transformation
    public Matrix4x4 RotationMatrix;

    // Portal visual properties
    public f32 PortalRadius; // Size of the portal opening
    public bool IsActive;
}

/// <summary>
/// Component marking an entity as visible through a portal
/// Used for client-side rendering optimization
/// </summary>
[EcsComponent(ComponentScope.Client, ComponentSync.None, ComponentFlags.Sparse)]
public partial struct PortalView : IComponent
{
    public EntityId PortalEntityId; // Which portal is showing this entity
    public Matrix4x4 ViewTransform; // Transform applied for rendering
    public i32 ViewLevel; // LOD level for this view
}

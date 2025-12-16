using System.Numerics;
using System.Runtime.CompilerServices;
using Ajiva.Ecs;
using Client.LayerSystem;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Wgsl;
using Common.Ecs;
using Interception;
using Silk.NET.WebGPU;

namespace Client.Renderer.Instancing;

public class UnifiedRenderableInstanceCollectionBuilder(NClientEcsWorld world, ResourceManager loader)
    : IRenderableInstanceCollectionBuilder
{
    private readonly NClientEcsWorld _world = world;
    private readonly ResourceManager _loader = loader;

    private u64 _renderableMeshVersion;
    private readonly Dictionary<u32, List<EntityId>> _entitiesByMesh = [];

    [TraceInterception]
    public void PrepareDrawEntities(RenderableInstanceCollection target)
    {
        if (_renderableMeshVersion == _world.RenderEntityVersion) return;
        _renderableMeshVersion = _world.RenderEntityVersion;

        // Group entities by mesh
        _entitiesByMesh.Clear();
        ForwardScan(_world);

        // Clear the old dynamic meshes
        foreach (var mesh in target.OpaqueMeshes) mesh.Dispose();
        target.OpaqueMeshes.Clear();
        // Create a mesh buffer for each mesh
        foreach (var (_, enitites) in _entitiesByMesh)
        {
            if (enitites.Count > 0)
            {
                var dynamicMesh = new PbrInstancedMeshBuffer(_loader.DefaultPbrCubeMesh,
                    ModelHelper.PbrMaterialExamples.WornLeather(),
                    _loader.CreateBuffer<PbrGBufferShader.PbrMaterialUniform>(
                        BufferUsage.Uniform | BufferUsage.CopyDst, 1, label: "ModelUniformBuffer"));
                target.OpaqueMeshes.Add(dynamicMesh);
            }
        }
    }

    internal void ForwardScan(NClientEcsWorld ecs) => _ForwardScan(ecs);

    [ExcludeFromInterception]
    private void _ForwardScan(NClientEcsWorld ecs)
    {
        foreach (var kv in ecs.RenderMesh)
        {
            if (!_entitiesByMesh.TryGetValue(kv.Value.MeshId, out var list))
            {
                list = [];
                _entitiesByMesh[kv.Value.MeshId] = list;
            }

            list.Add(kv.Key);
        }
    }

    [TraceInterception]
    public void UpdateDrawEntities(UpdateInfo info, RenderableInstanceCollection target)
    {
        //UpdateScene(queue, _sceneRenderCollection.StaticMeshes);
        f32 sFrame = 1f - MathF.Pow(0.01f, info.DeltaTime * _smoothingFrames);
        var index = 0;
        foreach (var (_, entities) in _entitiesByMesh)
        {
            var dynamicMesh = target.OpaqueMeshes[index++];
            dynamicMesh.MaterialUniform.ObjectId = (u32)index;

            if (dynamicMesh._instanceInfos.Count < entities.Count)
            {
                dynamicMesh._instanceInfos.AddRange(Enumerable.Repeat(new PbrGBufferShader.Instance(),
                    entities.Count - dynamicMesh._instanceInfos.Count));
            }
            else if (dynamicMesh._instanceInfos.Count > entities.Count)
            {
                dynamicMesh._instanceInfos.RemoveRange(entities.Count,
                    dynamicMesh._instanceInfos.Count - entities.Count);
            }

            CoreLoop(entities, dynamicMesh, _world, sFrame);
        }
    }

    internal void CoreLoop(List<EntityId> entities, PbrInstancedMeshBuffer dynamicMesh, NClientEcsWorld ecs,
        f32 sFrame) => _CoreLoopV2(entities, dynamicMesh, ecs, sFrame);

    [ExcludeFromInterception]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void _CoreLoop(List<EntityId> entities, PbrInstancedMeshBuffer dynamicMesh, NClientEcsWorld ecs,
        f32 sFrame)
    {
        for (var i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            ref var instance = ref dynamicMesh.InstanceInfos[i];
            if (ecs.Position.TryGet(entity, out var pos) &&
                ecs.Rotation.TryGet(entity, out var rot) &&
                ecs.Scale.TryGet(entity, out var scale))
            {
                var cc = world.Grid!.GetChunkCoord(pos.Value, 0);
                const f32 max = 10.0f;
                instance.HighlightColorRef = new Vector4(cc.X / max, cc.Y / max, cc.Z / max, 1f);
                var currentTransform = instance.GetTransform();
                var blended = SmoothStep(currentTransform, scale.Value, rot.Value, pos.Value, sFrame);
                instance.SetTransform(blended);
            }
            else
            {
                instance.SetTransform(default);
            }
        }
    }

    [ExcludeFromInterception]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void _CoreLoopV2(List<EntityId> entities, PbrInstancedMeshBuffer dynamicMesh, NClientEcsWorld ecs,
        f32 sFrame)
    {
        for (var i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            ref var instance = ref dynamicMesh.InstanceInfos[i];
            if (ecs.Transform.TryGet(entity, out var transform))
            {
                var cc = world.Grid!.GetChunkCoord(transform.Position, 0);
                const f32 max = 10.0f;
                instance.HighlightColorRef = new Vector4(cc.X / max, cc.Y / max, cc.Z / max, 1f);
                var currentTransform = instance.GetTransform();
                var blended = SmoothStep(currentTransform, transform.Scale, transform.Rotation,
                    transform.Position, sFrame);
                instance.SetTransform(blended);
            }
            else
            {
                instance.SetTransform(default);
            }
        }
    }
    
    private f32 _smoothingFrames = 10f;
    public ref f32 SmoothingFrames => ref _smoothingFrames;

    [ExcludeFromInterception]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static Matrix4x4 SmoothStep(Matrix4x4 current, Vector3 tarScale, Quaternion tarRot, Vector3 tarPos, f32 t)
    {
        if (current == default)
            return Matrix4x4.CreateScale(tarScale) *
                   Matrix4x4.CreateFromQuaternion(tarRot) *
                   Matrix4x4.CreateTranslation(tarPos);
        // Decompose matrices into components
        Matrix4x4.Decompose(current, out var curScale, out var curRot, out var curPos);

        // Interpolate position, scale, and rotation
        var newPos = Vector3.Lerp(curPos, tarPos, t);
        var newScale = Vector3.Lerp(curScale, tarScale, t);
        var newRot = Quaternion.Slerp(curRot, tarRot, t);

        // Recompose
        return Matrix4x4.CreateScale(newScale) *
               Matrix4x4.CreateFromQuaternion(newRot) *
               Matrix4x4.CreateTranslation(newPos);
    }

    public void RequestReBuild()
    {
        _renderableMeshVersion = 0;
    }

    public string Stats()
    {
        return
            $"Groups: {_entitiesByMesh.Count}, Meshes: {string.Join(", ", _entitiesByMesh.Select(kv => kv.Value.Count))}";
    }

    public EntityId GetEntityId(u32 selectedObjectId, u32 selectedInstanceId)
    {
        return _entitiesByMesh.TryGetValue(selectedObjectId, out var entityIds) && selectedInstanceId < entityIds.Count
            ? entityIds[(i32)selectedInstanceId]
            : EntityId.Invalid;
    }
}
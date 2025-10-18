using System.Numerics;
using System.Runtime.CompilerServices;
using Ajiva.Ecs;
using Client.Game;
using Client.LayerSystem;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Wgsl;
using Common;
using Common.Ecs;
using Common.SpacialPartitioning;
using Interception;
using Silk.NET.WebGPU;

namespace Client.Renderer.Instancing;

public class ChunkedRenderableInstanceCollectionBuilder(ClientGameLogic gameLogic, ResourceManager loader)
    : IRenderableInstanceCollectionBuilder
{
    private readonly ClientGameLogic _gameLogic = gameLogic;
    private readonly ResourceManager _loader = loader;

    private u64 _renderableMeshVersion;
    private readonly Dictionary<(ChunkCoord chunk, u32 meshId), i32> _chunkToInstance = [];
    private Queue<i32> _freeList = [];

    public void PrepareDrawEntities(RenderableInstanceCollection target)
    {
        var ecs = _gameLogic.World;
        if (_gameLogic.Grid is null) return;
        //if (_renderableMeshVersion == _gameLogic.RenderEntityVersion) return;
        _renderableMeshVersion = _gameLogic.RenderEntityVersion;
        List<(ChunkCoord chunk, u32 meshId)> seenChunks = [];
        var max = 0;
        using (var activity = Instrumentation.NamedTrace("Prepare"))
            foreach (var (coord, chunk) in _gameLogic.Grid.Chunks)
            {
                if (chunk.Entities.Count < 0) continue;
                using var _ = chunk.UseLock();
                foreach (var entityId in chunk.Entities)
                {
                    if (!ecs.TryGetRenderMesh(entityId, out var renderMesh)) continue;

                    var key = (coord, renderMesh.MeshId);
                    seenChunks.Add(key);
                    if (!_chunkToInstance.ContainsKey(key))
                    {
                        if (_freeList.Count > 0)
                        {
                            var instanceIndex = _freeList.Dequeue();
                            _chunkToInstance[key] = instanceIndex;
                        }
                        else
                        {
                            var instanceIndex = _chunkToInstance.Count;
                            _chunkToInstance[key] = instanceIndex;
                        }
                    }

                    var index = _chunkToInstance[key];
                    max = Math.Max(max, index);
                    if (index < target.OpaqueMeshes.Count) continue;

                    var dynamicMesh = new PbrInstancedMeshBuffer(_loader.DefaultPbrCubeMesh,
                        ModelHelper.PbrMaterialExamples.WornLeather(),
                        _loader.CreateBuffer<PbrGBufferShader.PbrMaterialUniform>(
                            BufferUsage.Uniform | BufferUsage.CopyDst, 1, label: "ModelUniformBuffer"));
                    target.OpaqueMeshes.Add(dynamicMesh);
                }
            }

        // Remove old chunks that are no longer seen
        using (var activity = Instrumentation.NamedTrace("Prune"))
        {
            var keysToRemove = _chunkToInstance.Keys.Except(seenChunks).ToList();
            foreach (var key in keysToRemove)
            {
                if (!_chunkToInstance.TryGetValue(key, out var index)) continue;
                var mesh = target.OpaqueMeshes[index];
                mesh._instanceInfos.Clear();
                _freeList.Enqueue(index);
                _chunkToInstance.Remove(key);
            }
        }

        // Optionally shrink the target.OpaqueMeshes list if there are many free slots
        var maxOversize = (i32)(max * 1.5f); // Allow up to 50% oversize
        if (target.OpaqueMeshes.Count < 10 || target.OpaqueMeshes.Count <= maxOversize) return;
        using var activity2 = Instrumentation.NamedTrace("Cleanup Oversubscription");

        // Start from the end and work backwards
        var countToRemove = target.OpaqueMeshes.Count - maxOversize;
        for (var idx = target.OpaqueMeshes.Count - 1; idx >= maxOversize && countToRemove > 0; idx--)
        {
            if (_chunkToInstance.ContainsValue(idx)) continue;
            var mesh = target.OpaqueMeshes[idx];
            mesh.Dispose();
            target.OpaqueMeshes.RemoveAt(idx);
            countToRemove--;
        }
    }

    public void UpdateDrawEntities(UpdateInfo info, RenderableInstanceCollection target)
    {
        var ecs = _gameLogic.World;
        var sFrame = 1f - MathF.Pow(0.01f, info.DeltaTime * _smoothingFrames);
        if (_gameLogic.Grid is null) return;
        var dirtyChunks = _gameLogic.Grid.Chunks.Where(x => x.Value.IsDirty).ToDictionary();
        if (dirtyChunks.Count == 0) return;
        _renderingCount = 0;

        List<EntityId> tmpEntities = [];
        foreach (var ((chunk, meshId), instanceIndex) in _chunkToInstance)
        {
            if (!dirtyChunks.TryGetValue(chunk, out var chunkData)) continue;

            using (var _ = chunkData.UseLock())
            {
                tmpEntities.Clear();
                tmpEntities.AddRange(chunkData.Entities);
            }

            var entitiesCount = tmpEntities.Count;
            var meshBuffer = target.OpaqueMeshes[instanceIndex];
            var instances = meshBuffer._instanceInfos;
            if (entitiesCount == 0)
            {
                instances.Clear();
                continue;
            }

            bool nonSlide = false;
            if (instances.Count < entitiesCount)
            {
                instances.AddRange(Enumerable.Repeat(new PbrGBufferShader.Instance(),
                    entitiesCount - instances.Count));
                nonSlide = true;
            }
            else if (instances.Count > entitiesCount)
            {
                instances.RemoveRange(entitiesCount, instances.Count - entitiesCount);
                nonSlide = true;
            }

            nonSlide = true;

            CoreLoop(tmpEntities, meshBuffer, ecs, nonSlide, sFrame);
        }
    }

    public void CoreLoop(List<EntityId> tmpEntities, PbrInstancedMeshBuffer meshBuffer, ClientEcsWorld ecs,
        bool nonSlide,
        f32 sFrame) => CoreLoop2(tmpEntities, meshBuffer, ecs, nonSlide, sFrame);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    [ExcludeFromInterception]
    private void CoreLoop2(List<EntityId> tmpEntities, PbrInstancedMeshBuffer meshBuffer, ClientEcsWorld ecs,
        bool nonSlide,
        f32 sFrame)
    {
        var i = 0;
        foreach (var entity in tmpEntities)
        {
            ref var instance = ref meshBuffer.InstanceInfos[i];
            if (ecs.TryGetPosition(entity, out var pos) &&
                ecs.TryGetRotation(entity, out var rot) &&
                ecs.TryGetScale(entity, out var scale))
            {
                var currentTransform = instance.GetTransform();
                Matrix4x4 matrix;
                if (nonSlide || currentTransform == default)
                {
                    matrix = Matrix4x4.CreateScale(scale.Value) *
                             Matrix4x4.CreateFromQuaternion(rot.Value) * Matrix4x4.CreateTranslation(pos.Value);
                }
                else
                {
                    matrix = SmoothStep(currentTransform, scale.Value, rot.Value, pos.Value, sFrame);
                }

                instance.SetTransform(matrix);
                _renderingCount++;
            }
            else
            {
                instance.SetTransform(default);
            }

            i++;
        }
    }

    private f32 _smoothingFrames = 10f;
    public ref f32 SmoothingFrames => ref _smoothingFrames;
    private i32 _renderingCount = 0;

    [ExcludeFromInterception]
    public static Matrix4x4 SmoothStep(Matrix4x4 current, Vector3 tarScale, Quaternion tarRot, Vector3 tarPos, f32 t)
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
        return $"Groups: {_chunkToInstance.Count}, Free: {_freeList.Count}, Rendering {_renderingCount}";
    }
}
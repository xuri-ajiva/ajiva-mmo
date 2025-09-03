using System.Numerics;
using Client.Game;
using Client.LayerSystem;
using Client.Renderer.PbrRenderer;
using Client.Renderer.Resources;
using Client.Wgsl;
using Common.Ecs;
using Interception;
using Silk.NET.WebGPU;

namespace Client.Renderer.Instancing;

public class UnifiedRenderableInstanceCollectionBuilder(ClientGameLogic gameLogic, ResourceManager loader)
    : IRenderableInstanceCollectionBuilder
{
    private readonly ClientGameLogic _gameLogic = gameLogic;
    private readonly ResourceManager _loader = loader;

    private u64 _renderableMeshVersion;
    private readonly Dictionary<u32, List<EntityId>> _entitiesByMesh = [];

    [TraceInterception]
    public void PrepareDrawEntities(RenderableInstanceCollection target)
    {
        var ecs = _gameLogic.World;
        if (_renderableMeshVersion != _gameLogic.RenderEntityVersion)
        {
            _renderableMeshVersion = _gameLogic.RenderEntityVersion;

            // Group entities by mesh
            _entitiesByMesh.Clear();
            foreach (var ent in ecs.GetEntitiesWithRenderMesh())
            {
                if (ent == _gameLogic.LocalPlayerEntityId) continue; // Dont Render Local Player
                if (!ecs.TryGetRenderMesh(ent, out var renderable)) continue;
                if (!_entitiesByMesh.TryGetValue(renderable.MeshId, out var list))
                {
                    list = [];
                    _entitiesByMesh[renderable.MeshId] = list;
                }

                list.Add(ent);
            }

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
    }

    [TraceInterception]
    public void UpdateDrawEntities(UpdateInfo info, RenderableInstanceCollection target)
    {
        var ecs = _gameLogic.World;
        //UpdateScene(queue, _sceneRenderCollection.StaticMeshes);
        float sFrame = 1f - MathF.Pow(0.01f, info.DeltaTime * _smoothingFrames);
        var index = 0;
        foreach (var (_, entities) in _entitiesByMesh)
        {
            var dynamicMesh = target.OpaqueMeshes[index++];
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

            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                if (ecs.TryGetPosition(entity, out var pos) &&
                    ecs.TryGetRotation(entity, out var rot) &&
                    ecs.TryGetScale(entity, out var scale))
                {
                    var currentTransform = dynamicMesh.InstanceInfos[i].GetTransform();
                    var blended = SmoothStep(currentTransform, scale.Value, rot.Value, pos.Value, sFrame);
                    dynamicMesh.InstanceInfos[i].SetTransform(blended);
                }
                else
                {
                    dynamicMesh.InstanceInfos[i].SetTransform(default);
                }
            }
        }
    }

    private f32 _smoothingFrames = 10f;
    public ref f32 SmoothingFrames => ref _smoothingFrames;

    public static Matrix4x4 SmoothStep(Matrix4x4 current, Vector3 tarScale, Quaternion tarRot, Vector3 tarPos, float t)
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
}
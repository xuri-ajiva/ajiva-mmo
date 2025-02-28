using System.Numerics;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.WebGPU.Safe;

namespace client.Renderer;

public record struct BufferRange(BufferPtr Buffer, u64 Offset, u64 Size);

public class InstancedMeshBuffer<TModelUniform, TInstance>(
    BufferRange vertexBuffer,
    BufferRange indexBuffer,
    TModelUniform modelUniform,
    TInstance[] instanceInfos
)
{
    // buffers
    public BufferRange VertexBuffer { get; set; } = vertexBuffer;
    public BufferRange IndexBuffer { get; set; } = indexBuffer;
    public BufferRange? InstanceBuffer;
    public BufferRange ModelUniformBuffer { get; set; }

    // bind groups
    public BindGroupPtr? ModelBindGroup { get; set; }

    // values
    public TModelUniform ModelUniform = modelUniform;
    public TInstance[] InstanceInfos = instanceInfos;
    public TextureAndView AlbedoTexture;
    public TextureAndView NormalTexture;
    public TextureAndView EmissionTexture;

    public void Deconstruct(out BufferRange vertexBuffer, out BufferRange indexBuffer, out BufferRange? instanceBuffer,
        out BindGroupPtr? modelBindGroup, out TInstance[] instanceInfos)
    {
        vertexBuffer = VertexBuffer;
        indexBuffer = IndexBuffer;
        instanceBuffer = InstanceBuffer;
        modelBindGroup = ModelBindGroup;
        instanceInfos = InstanceInfos;
    }
}

public record struct TextureAndView(TexturePtr Texture, TextureViewPtr View);

public class DragCamera
{
    private static f32 _yaw = 0;
    private static f32 _pitch = 0;
    private static Quaternion _camRot = Quaternion.Identity;
    private static Vector3 _camTarget = Vector3.Zero;

    public Vector3 Position => _camTarget; //todo fix maybe

    public Matrix4x4 GetViewMatrix(f32 aspectRatio)
    {
        return (
            Matrix4X4.CreateTranslation(-_camTarget.ToGeneric()) *
            Matrix4X4.CreateFromQuaternion(Quaternion.Inverse(_camRot).ToGeneric()) *
            Matrix4X4.CreateTranslation(0f, 0f, -15) *
            Matrix4X4.CreatePerspectiveFieldOfView((f32)(2 * Math.PI / 5),
                aspectRatio, 0.01f, 100f)
        ).ToSystem();
    }

    public void Update()
    {
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            _camTarget += Vector3.Transform(new Vector3(
                -ImGui.GetIO().MouseDelta.X * 0.02f,
                ImGui.GetIO().MouseDelta.Y * 0.02f,
                0), _camRot);
        }

        if (ImGui.IsMouseDragging(ImGuiMouseButton.Right))
        {
            _yaw -= ImGui.GetIO().MouseDelta.X * 0.005f;
            _pitch -= ImGui.GetIO().MouseDelta.Y * 0.005f;
            _camRot =
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, _yaw) *
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, _pitch);
        }

        // allow zoom in
        var scroll = ImGui.GetIO().MouseWheel;
        if (scroll != 0)
        {
            _camTarget += Vector3.Transform(new Vector3(0, 0, scroll * -0.5f), _camRot);
        }
    }
}

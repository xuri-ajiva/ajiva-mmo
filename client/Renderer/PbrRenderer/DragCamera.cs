using System.Numerics;
using Client.LayerSystem;
using GlmSharp;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace Client.Renderer.PbrRenderer;

public interface ICamera
{
    Vector3 Position { get; }
    f32 AspectRatio {  set; }
    Matrix4x4 ViewMatrix { get; }
    Matrix4x4 ProjectionMatrix { get; }
    Matrix4x4 ViewProjectionMatrix { get; }
    IInputContext? InputContext { set; }
    void Update(UpdateInfo info);
    void Attach();
}

public class DragCamera : ICamera
{
    private f32 _yaw;
    private f32 _pitch;
    private Quaternion _camRot = Quaternion.Identity;
    private Vector3 _camTarget = Vector3.Zero;

    public Vector3 Position => _camTarget; //todo fix maybe
    public f32 AspectRatio { get; set; } = 1;

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateTranslation(-_camTarget) *
                                   Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(_camRot));

    public Matrix4x4 ProjectionMatrix => Matrix4x4.CreatePerspectiveFieldOfView((f32)(2 * Math.PI / 5),
        AspectRatio, 0.01f, 1000f);

    public Matrix4x4 ViewProjectionMatrix => ViewMatrix * ProjectionMatrix;
    public IInputContext? InputContext { get; set; }

    public void Update(UpdateInfo info)
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

    public void Attach()
    {
    }
}

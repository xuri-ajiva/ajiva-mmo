using System.Numerics;
using GlmSharp;
using ImGuiNET;
using Silk.NET.Maths;

namespace Client.Renderer.PbrRenderer;

public class DragCamera
{
    private f32 _yaw;
    private f32 _pitch;
    private Quaternion _camRot = Quaternion.Identity;
    private Vector3 _camTarget = Vector3.Zero;

    public Vector3 Position => _camTarget; //todo fix maybe
    public f32 AspectRatio { get; set; } = 1;
    public Matrix4x4 ViewProjectionMatrix => GetViewProjectionMatrix(AspectRatio);

    public Matrix4x4 GetViewProjectionMatrix(f32 aspectRatio)
    {
        return Matrix4x4.CreateTranslation(-_camTarget) *
               Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(_camRot)) *
               Matrix4x4.CreatePerspectiveFieldOfView((f32)(2 * Math.PI / 5),
                   aspectRatio, 0.01f, 1000f);
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

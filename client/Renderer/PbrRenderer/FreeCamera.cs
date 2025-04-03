using System.Numerics;
using Client.LayerSystem;
using ImGuiNET;
using Silk.NET.Input;

namespace Client.Renderer.PbrRenderer;

public class FreeCamera
{
    public f32 Fov { get; set; } = 45.0f;
    public f32 Near { get; set; } = 0.1f;
    public f32 Far { get; set; } = 100.0f;
    public f32 AspectRatio { get; set; } = 1.0f;
    public Vector3 Position { get; set; } = new(0, 0, 0);
    public Vector3 Target { get; set; } = new(0, 0, -1);
    public Vector3 Up { get; set; } = new(0, 1, 0);

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Target, Up);

    public Matrix4x4 ProjectionMatrix => Matrix4x4.CreatePerspectiveFieldOfView(
        MathF.PI * Fov / 180.0f,
        AspectRatio,
        Near,
        Far
    );

    public Matrix4x4 ViewProjectionMatrix => ViewMatrix * ProjectionMatrix;

    public void Update()
    {
        // take ImGui input and update the camera position and target
        // this is just a placeholder, you can implement your own camera control logic

        var keyboard = AjivaImGuiLayerSystem._input.Keyboards[0];
        if (keyboard.IsKeyPressed(Key.W))
        {
            Position += Vector3.Normalize(Target - Position) * 0.1f;
        }

        if (keyboard.IsKeyPressed(Key.S))
        {
            Position -= Vector3.Normalize(Target - Position) * 0.1f;
        }

        if (keyboard.IsKeyPressed(Key.A))
        {
            Position -= Vector3.Normalize(Vector3.Cross(Target - Position, Up)) * 0.1f;
        }

        if (keyboard.IsKeyPressed(Key.D))
        {
            Position += Vector3.Normalize(Vector3.Cross(Target - Position, Up)) * 0.1f;
        }

        if (keyboard.IsKeyPressed(Key.Space))
        {
            Position += Up * 0.1f;
        }

        if (keyboard.IsKeyPressed(Key.ShiftLeft))
        {
            Position -= Up * 0.1f;
        }

        // update rotation based on mouse movement
        if (ImGui.IsMouseDragging(ImGuiMouseButton.Right))
        {
            var mouseDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Right);
            ImGui.ResetMouseDragDelta(ImGuiMouseButton.Right);

            var rotationSpeed = 0.005f;
            var yaw = rotationSpeed * mouseDelta.X;
            var pitch = rotationSpeed * mouseDelta.Y;

            var rotationMatrix = Matrix4x4.CreateRotationX(pitch) * Matrix4x4.CreateRotationY(yaw);
            Target = Vector3.Transform(Target - Position, rotationMatrix) + Position;
        }
    }
}

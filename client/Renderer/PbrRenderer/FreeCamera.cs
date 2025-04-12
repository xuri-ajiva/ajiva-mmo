using System.Numerics;
using Client.LayerSystem;
using Client.Platform;
using GlmSharp;
using ImGuiNET;
using Silk.NET.Input;

namespace Client.Renderer.PbrRenderer;

public class FreeCamera(AjivaWindow window) : ICamera
{
    private Vector3 _position = new(0.0f, 0.0f, 3.0f); // Default position
    private f32 _aspectRatio = 16.0f / 9.0f; // Default aspect ratio
    private f32 _fieldOfViewRadians = MathHelper.DegreesToRadians(75.0f); // Default FOV
    private f32 _nearPlane = 0.01f;
    private f32 _farPlane = 1000.0f;

    private f32 _yaw = -MathF.PI / 2.0f; // Initialize looking along -Z
    private f32 _pitch;

    private Vector3 _front = -Vector3.UnitZ;
    private Vector3 _up = Vector3.UnitY;
    private Vector3 _right = Vector3.UnitX;
    private readonly Vector3 _worldUp = Vector3.UnitY;

    private Matrix4x4 _viewMatrix;
    private Matrix4x4 _projectionMatrix;
    private Matrix4x4 _viewProjectionMatrix;

    private bool _needsViewUpdate = true;
    private bool _needsProjectionUpdate = true;

    private IInputContext? _inputContext;
    private Vector2 _lastMousePosition = Vector2.Zero;

    public f32 MovementSpeed { get; set; } = 3.0f;
    public f32 SprintMultiplier { get; set; } = 2.5f;
    public f32 MouseSensitivity { get; set; } = 0.002f;
    public f32 MinPitchRadians { get; set; } = -MathF.PI / 2.0f + 0.01f; // Limit looking straight down
    public f32 MaxPitchRadians { get; set; } = MathF.PI / 2.0f - 0.01f; // Limit looking straight up


    public Vector3 Position => _position;

    public f32 AspectRatio
    {
        set
        {
            if (_aspectRatio != value && value > 0)
            {
                _aspectRatio = value;
                _needsProjectionUpdate = true;
            }
        }
    }

    public Matrix4x4 ViewMatrix => _viewMatrix;
    public Matrix4x4 ProjectionMatrix => _projectionMatrix;
    public Matrix4x4 ViewProjectionMatrix => _viewProjectionMatrix;

    public IInputContext? InputContext
    {
        set => _inputContext = value;
    }

    public void Update(UpdateInfo info)
    {
        if (ProcessInput(info.DeltaTime))
        {
            _needsViewUpdate = true;
        }

        // Update matrices if orientation, position, or projection params changed
        if (_needsViewUpdate || _needsProjectionUpdate)
        {
            UpdateMatrices();
        }
    }

    public void Attach()
    {
        UpdateMatrices();
        _inputContext = window.Input;
    }


    public void SetPosition(Vector3 newPosition)
    {
        if (_position != newPosition)
        {
            _position = newPosition;
            _needsViewUpdate = true;
        }
    }

    public void SetLookDirection(f32 yawDegrees, f32 pitchDegrees)
    {
        _yaw = MathHelper.DegreesToRadians(yawDegrees);
        _pitch = MathHelper.Clamp(MathHelper.DegreesToRadians(pitchDegrees), MinPitchRadians, MaxPitchRadians);
        UpdateVectors(); // This sets _needsViewUpdate = true
    }

    public void SetFieldOfView(f32 fovDegrees)
    {
        f32 newFovRad = MathHelper.DegreesToRadians(fovDegrees);
        if (_fieldOfViewRadians != newFovRad)
        {
            _fieldOfViewRadians = newFovRad;
            _needsProjectionUpdate = true;
        }
    }

    private bool ProcessInput(f32 deltaTime)
    {
        if (_inputContext == null) return false;

        var positionChanged = false;
        var moveDirection = Vector3.Zero;

        var keyboard = _inputContext.Keyboards[0];
        if (keyboard.IsKeyPressed(Key.W))
            moveDirection += _front;
        if (keyboard.IsKeyPressed(Key.S))
            moveDirection -= _front;
        if (keyboard.IsKeyPressed(Key.A))
            moveDirection -= _right;
        if (keyboard.IsKeyPressed(Key.D))
            moveDirection += _right;
        if (keyboard.IsKeyPressed(Key.Q))
            moveDirection += _worldUp; // Use World Up for consistent vertical movement
        if (keyboard.IsKeyPressed(Key.E))
            moveDirection -= _worldUp;

        if (moveDirection != Vector3.Zero)
        {
            moveDirection = Vector3.Normalize(moveDirection);
            var currentSpeed = MovementSpeed;
            if (keyboard.IsKeyPressed(Key.ShiftLeft))
            {
                currentSpeed *= SprintMultiplier;
            }

            _position += moveDirection * currentSpeed * deltaTime;
            positionChanged = true;
        }


        var mouse = _inputContext.Mice[0];
        if (mouse.IsButtonPressed(MouseButton.Right))
        {
            Vector2 mouseDelta = mouse.Position - _lastMousePosition;
            if (mouseDelta.X != 0 || mouseDelta.Y != 0)
            {
                _yaw += mouseDelta.X * MouseSensitivity;
                _pitch -= mouseDelta.Y * MouseSensitivity; // Reversed Y

                // Clamp pitch
                _pitch = MathHelper.Clamp(_pitch, MinPitchRadians, MaxPitchRadians);

                UpdateVectors(); // Recalculate Front, Right, Up vectors and flag view matrix update
            }
        }

        _lastMousePosition = mouse.Position;

        return positionChanged;
    }

    /// <summary>
    /// Recalculates the Front, Right, and Up vectors based on Yaw and Pitch.
    /// Flags the View matrix for update.
    /// </summary>
    private void UpdateVectors()
    {
        // Calculate the new Front vector
        Vector3 front;
        front.X = MathF.Cos(_yaw) * MathF.Cos(_pitch);
        front.Y = MathF.Sin(_pitch);
        front.Z = MathF.Sin(_yaw) * MathF.Cos(_pitch);
        _front = Vector3.Normalize(front);

        // Recalculate the Right and Up vector
        _right = Vector3.Normalize(Vector3.Cross(_front, _worldUp));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));

        _needsViewUpdate = true;
    }

    /// <summary>
    /// Recalculates the View and/or Projection matrices if needed.
    /// Also recalculates the combined ViewProjection matrix.
    /// </summary>
    private void UpdateMatrices()
    {
        bool viewProjNeedsUpdate = false;

        if (_needsViewUpdate)
        {
            _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + _front, _up);
            _needsViewUpdate = false;
            viewProjNeedsUpdate = true;
        }

        if (_needsProjectionUpdate)
        {
            _projectionMatrix =
                Matrix4x4.CreatePerspectiveFieldOfView(_fieldOfViewRadians, _aspectRatio, _nearPlane, _farPlane);
            _needsProjectionUpdate = false;
            viewProjNeedsUpdate = true;
        }

        if (viewProjNeedsUpdate)
        {
            // Important: Matrix multiplication order is Projection * View
            _viewProjectionMatrix = _viewMatrix * _projectionMatrix;
        }
    }
}

public static class MathHelper
{
    public static f32 DegreesToRadians(f32 degrees)
    {
        return degrees * (MathF.PI / 180.0f);
    }

    public static f32 RadiansToDegrees(f32 radians)
    {
        return radians * (180.0f / MathF.PI);
    }

    public static f32 Clamp(f32 value, f32 min, f32 max)
    {
        return Math.Max(min, Math.Min(value, max));
    }
}

using System.Numerics;
using Client.Common;
using Common.Helpers;
using Common.Systems;
using Hexa.NET.SDL3;

namespace Client.Renderer.PbrRenderer;

public class FreeCamera : ICamera, IEventHandler<AjivaMouseEvent>, IEventHandler<AjivaKeyboardEvent>
{
    private Vector3 _position = new(0.0f, 0.0f, 3.0f); // Default position
    private f32 _aspectRatio = 16.0f / 9.0f; // Default aspect ratio
    private f32 _fieldOfViewRadians = MathHelper.ToRadians(75.0f); // Default FOV
    private f32 _nearPlane = 0.01f;
    private f32 _farPlane = 100000.0f;

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

    private Vector2 _lastMousePosition = Vector2.Zero;

    public f32 MovementSpeed { get; set; } = 10.0f;
    public f32 SprintMultiplier { get; set; } = 25f;
    public f32 MouseSensitivity { get; set; } = 0.002f;
    public f32 MinPitchRadians { get; set; } = -MathF.PI / 2.0f + 0.01f; // Limit looking straight down
    public f32 MaxPitchRadians { get; set; } = MathF.PI / 2.0f - 0.01f; // Limit looking straight up

    private bool _inputEnabled = true;

    private bool _moveForward;
    private bool _moveBackward;
    private bool _moveLeft;
    private bool _moveRight;
    private bool _moveUp;
    private bool _moveDown;
    private bool _sprint;
    private bool _isLooking;
    private bool _hasMouseAnchor;
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

    public bool InputEnabled => _inputEnabled;

    public void SetInputEnabled(bool enabled)
    {
        if (_inputEnabled == enabled)
        {
            return;
        }

        _inputEnabled = enabled;
        if (!enabled)
        {
            ClearInputState();
        }
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
        _yaw = MathHelper.ToRadians(yawDegrees);
        _pitch = Math.Clamp(MathHelper.ToRadians(pitchDegrees), MinPitchRadians, MaxPitchRadians);
        UpdateVectors(); // This sets _needsViewUpdate = true
    }

    public void SetFieldOfView(f32 fovDegrees)
    {
        var newFovRad = MathHelper.ToRadians(fovDegrees);
        if (_fieldOfViewRadians != newFovRad)
        {
            _fieldOfViewRadians = newFovRad;
            _needsProjectionUpdate = true;
        }
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

    private void ClearInputState()
    {
        _moveForward = false;
        _moveBackward = false;
        _moveLeft = false;
        _moveRight = false;
        _moveUp = false;
        _moveDown = false;
        _sprint = false;

        _isLooking = false;
        _hasMouseAnchor = false;
        _lastMousePosition = Vector2.Zero;
    }

    public void OnEvent(ref AjivaEvent<AjivaKeyboardEvent> inputEvent)
    {
        if (!_inputEnabled)
        {
            return;
        }

        var keyDown = inputEvent.EventData.IsPressed == AjivaKeyboardEventType.KeyDown;

        switch (inputEvent.EventData.KeyCode)
        {
            case SDLScancode.W:
                _moveForward = keyDown;
                break;
            case SDLScancode.S:
                _moveBackward = keyDown;
                break;
            case SDLScancode.A:
                _moveLeft = keyDown;
                break;
            case SDLScancode.D:
                _moveRight = keyDown;
                break;
            case SDLScancode.E:
                _moveUp = keyDown;
                break;
            case SDLScancode.Q:
                _moveDown = keyDown;
                break;
            case SDLScancode.Lshift:
            case SDLScancode.Rshift:
                _sprint = keyDown;
                break;
        }
    }

    private bool ProcessInput(f32 deltaTime)
    {
        var moveDirection = Vector3.Zero;

        if (_moveForward) moveDirection += _front;
        if (_moveBackward) moveDirection -= _front;
        if (_moveLeft) moveDirection -= _right;
        if (_moveRight) moveDirection += _right;
        if (_moveUp) moveDirection += _worldUp;
        if (_moveDown) moveDirection -= _worldUp;

        if (moveDirection == Vector3.Zero)
        {
            return false;
        }

        moveDirection = Vector3.Normalize(moveDirection);
        var currentSpeed = MovementSpeed;
        if (_sprint)
        {
            currentSpeed *= SprintMultiplier;
        }

        _position += moveDirection * currentSpeed * deltaTime;
        return true;
    }

    public void OnEvent(ref AjivaEvent<AjivaMouseEvent> inputEvent)
    {
        if (!_inputEnabled)
        {
            return;
        }

        var data = inputEvent.EventData;

        // NOTE:
        // Adjust these property names/enums if your AjivaMouseEvent differs.
        // The pattern is: right button gates look, motion rotates camera.
        switch (data.Type)
        {
            case AjivaMouseEventType.ButtonDown:
                if (data.Button == AjivaMouseButton.Right)
                {
                    _isLooking = true;
                    _hasMouseAnchor = false; // avoid first-frame jump
                }

                break;

            case AjivaMouseEventType.ButtonUp:
                if (data.Button == AjivaMouseButton.Right)
                {
                    _isLooking = false;
                    _hasMouseAnchor = false;
                }

                break;

            case AjivaMouseEventType.Move:
                if (!_isLooking)
                {
                    return;
                }

                var current = new Vector2(data.X, data.Y);

                if (!_hasMouseAnchor)
                {
                    _lastMousePosition = current;
                    _hasMouseAnchor = true;
                    return;
                }

                var delta = current - _lastMousePosition;
                _lastMousePosition = current;

                if (delta.X == 0 && delta.Y == 0)
                {
                    return;
                }

                _yaw += delta.X * MouseSensitivity;
                _pitch -= delta.Y * MouseSensitivity; // invert Y for FPS feel
                _pitch = Math.Clamp(_pitch, MinPitchRadians, MaxPitchRadians);

                UpdateVectors();
                break;
        }
    }
}
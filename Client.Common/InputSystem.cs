using System.Numerics;
using System.Threading.Channels;
using Client.Common.Accessors;
using Common;
using Common.Systems;
using Silk.NET.Input;

namespace Client.Common;

public enum AjivaMouseEventType : u8
{
    Click,
    DoubleClick,
    ButtonUp,
    ButtonDown,
    Move,
    Scroll
}

public readonly record struct AjivaMouseEvent(
    AjivaMouseEventType Type,
    MouseButton Button,
    f32 X,
    f32 Y,
    f32 DeltaX,
    f32 DeltaY);

public enum AjivaKeyboardEventType : u8
{
    KeyDown,
    KeyUp,
    Key,
}

public readonly record struct AjivaKeyboardEvent(Key KeyCode, AjivaKeyboardEventType IsPressed);

public struct AjivaEvent<T>(T eventData, bool handled)
{
    public T EventData { get; } = eventData;
    public bool Handled { get; set; } = handled;
}

public interface IEventHandler<T>
{
    void OnEvent(AjivaEvent<T> inputEvent);
}

public class InputSystem(IInputAccessor accessor, LayerStack layerStack) : IAjivaLayer
{
    private readonly LayerStack _layerStack = layerStack;

    public void Attach()
    {
        foreach (var context in accessor.InputContext.Mice)
        {
            InitDevice(context);
        }

        foreach (var context in accessor.InputContext.Keyboards)
        {
            InitDevice(context);
        }

        foreach (var context in accessor.InputContext.Gamepads)
        {
            InitDevice(context);
        }

        accessor.InputContext.ConnectionChanged += (device, connected) =>
        {
            if (connected)
            {
                InitDevice(device);
            }
        };
    }

    private Channel<AjivaMouseEvent> MouseEventChannel { get; } =
        Channel.CreateUnbounded<AjivaMouseEvent>(new UnboundedChannelOptions
            { SingleReader = true, SingleWriter = false });

    private Channel<AjivaKeyboardEvent> KeyboardEventChannel { get; } =
        Channel.CreateUnbounded<AjivaKeyboardEvent>(new UnboundedChannelOptions
            { SingleReader = true, SingleWriter = false });

    private void InitDevice(IInputDevice context)
    {
        switch (context)
        {
            case IMouse mouse:
                mouse.Click += OnMouseOnClick;
                mouse.MouseDown += OnMouseOnMouseDown;
                mouse.MouseUp += OnMouseOnMouseUp;
                mouse.MouseMove += OnMouseOnMouseMove;
                mouse.Scroll += OnMouseOnScroll;
                mouse.DoubleClick += OnMouseOnDoubleClick;
                break;
            case IKeyboard keyboard:
                keyboard.KeyDown += OnKeyboardOnKeyDown;
                keyboard.KeyUp += OnKeyboardOnKeyUp;
                keyboard.KeyChar += OnKeyboardOnKeyChar;
                break;
            case IGamepad gamepad:
                break;
            default:
                break;
        }
    }

    private void WriteMouseEvent(AjivaMouseEvent ajivaMouseEvent) => MouseEventChannel.Writer.TryWrite(ajivaMouseEvent);

    private void WriteKeyboardEvent(AjivaKeyboardEvent ajivaKeyboardEvent) =>
        KeyboardEventChannel.Writer.TryWrite(ajivaKeyboardEvent);

    public void Update(UpdateInfo info)
    {
        using var activity = Instrumentation.NamedTrace("InputSystem.Update");
        if (!_layerStack.Attached) return;
        using (Instrumentation.NamedTrace("InputSystem.KeyboardEvents"))
            while (KeyboardEventChannel.Reader.TryRead(out var ajivaKeyboardEvent))
            {
                var evnt = new AjivaEvent<AjivaKeyboardEvent>(ajivaKeyboardEvent, false);
                foreach (var layerStackLayer in _layerStack.Layers.OfType<IEventHandler<AjivaKeyboardEvent>>())
                {
                    layerStackLayer.OnEvent(evnt);
                    if (evnt.Handled)
                    {
                        break;
                    }
                }
            }

        using (Instrumentation.NamedTrace("InputSystem.MouseEvents"))
            while (MouseEventChannel.Reader.TryRead(out var ajivaMouseEvent))
            {
                var evnt = new AjivaEvent<AjivaMouseEvent>(ajivaMouseEvent, false);
                foreach (var layerStackLayer in _layerStack.Layers.OfType<IEventHandler<AjivaMouseEvent>>())
                {
                    layerStackLayer.OnEvent(evnt);
                    if (evnt.Handled)
                    {
                        break;
                    }
                }
            }
    }

    public void Detach()
    {
        //todo: Unsubscribe from events
    }

    public ref bool Enabled => ref _enabled;
    private bool _enabled = true;

    void OnKeyboardOnKeyDown(IKeyboard iKeyboard, Key key, int arg3) =>
        WriteKeyboardEvent(new AjivaKeyboardEvent(key, AjivaKeyboardEventType.KeyDown));

    void OnKeyboardOnKeyUp(IKeyboard iKeyboard, Key key, int arg3) =>
        WriteKeyboardEvent(new AjivaKeyboardEvent(key, AjivaKeyboardEventType.KeyUp));

    void OnKeyboardOnKeyChar(IKeyboard iKeyboard, char chr) =>
        WriteKeyboardEvent(new AjivaKeyboardEvent((Key)chr, AjivaKeyboardEventType.Key));

    void OnMouseOnClick(IMouse iMouse, MouseButton button, Vector2 arg3) =>
        WriteMouseEvent(new AjivaMouseEvent(AjivaMouseEventType.Click, button, arg3.X, arg3.Y, 0, 0));

    void OnMouseOnMouseDown(IMouse iMouse, MouseButton button) => WriteMouseEvent(
        new AjivaMouseEvent(AjivaMouseEventType.ButtonDown, button, iMouse.Position.X, iMouse.Position.Y, 0, 0));

    void OnMouseOnMouseUp(IMouse iMouse, MouseButton button) => WriteMouseEvent(
        new AjivaMouseEvent(AjivaMouseEventType.ButtonUp, button, iMouse.Position.X, iMouse.Position.Y, 0, 0));

    void OnMouseOnMouseMove(IMouse iMouse, Vector2 position) =>
        WriteMouseEvent(new AjivaMouseEvent(AjivaMouseEventType.Move, MouseButton.Left, position.X, position.Y, 0, 0));

    void OnMouseOnScroll(IMouse iMouse, ScrollWheel scrollWheel) => WriteMouseEvent(
        new AjivaMouseEvent(AjivaMouseEventType.Scroll, MouseButton.Left, iMouse.Position.X, iMouse.Position.Y,
            scrollWheel.X,
            scrollWheel.Y));

    void OnMouseOnDoubleClick(IMouse iMouse, MouseButton button, Vector2 arg3) =>
        WriteMouseEvent(new AjivaMouseEvent(AjivaMouseEventType.DoubleClick, button, arg3.X, arg3.Y, 0, 0));
}
using System.Numerics;
using System.Threading.Channels;
using Common;
using Common.Systems;
using Hexa.NET.SDL3;

namespace Client.Common;

public enum AjivaMouseEventType : u8
{
    DoubleClick,
    ButtonUp,
    ButtonDown,
    Move,
    Scroll
}

public enum AjivaMouseButton : u8
{
    Left = 0,
    Right = 1,
    Middle = 2,
    Button4 = 3,
    Button5 = 4,
}

public readonly record struct AjivaMouseEvent(
    AjivaMouseEventType Type,
    AjivaMouseButton Button,
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

public readonly record struct AjivaKeyboardEvent(
    SDLScancode KeyCode,
    SDLKeymod Modifiers,
    AjivaKeyboardEventType IsPressed);

public ref struct AjivaEvent<T>(ref T eventData, bool handled)
{
    public readonly ref T EventData = ref eventData;
    public bool Handled  = handled;
}

public interface IEventHandler<T>
{
    void OnEvent(ref AjivaEvent<T> inputEvent);
}

public class EventSystem(LayerStack layerStack) : IAjivaLayer
{
    private readonly LayerStack _layerStack = layerStack;

    public void Attach()
    {
        /*foreach (var context in accessor.InputContext.Mice)
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
        };*/
    }

    private Channel<AjivaMouseEvent> MouseEventChannel { get; } =
        Channel.CreateUnbounded<AjivaMouseEvent>(new UnboundedChannelOptions
            { SingleReader = true, SingleWriter = false });

    private Channel<AjivaKeyboardEvent> KeyboardEventChannel { get; } =
        Channel.CreateUnbounded<AjivaKeyboardEvent>(new UnboundedChannelOptions
            { SingleReader = true, SingleWriter = false });
    
    private Channel<SDLEvent> SdlEvents { get; } =
        Channel.CreateUnbounded<SDLEvent>(new UnboundedChannelOptions
            { SingleReader = true, SingleWriter = false });

    /*private void InitDevice(IInputDevice context)
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
    }*/

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
                var evnt = new AjivaEvent<AjivaKeyboardEvent>(ref ajivaKeyboardEvent, false);
                foreach (var layerStackLayer in _layerStack.Layers.OfType<IEventHandler<AjivaKeyboardEvent>>())
                {
                    layerStackLayer.OnEvent(ref evnt);
                    if (evnt.Handled)
                    {
                        break;
                    }
                }
            }

        using (Instrumentation.NamedTrace("InputSystem.MouseEvents"))
            while (MouseEventChannel.Reader.TryRead(out var ajivaMouseEvent))
            {
                var evnt = new AjivaEvent<AjivaMouseEvent>(ref ajivaMouseEvent, false);
                foreach (var layerStackLayer in _layerStack.Layers.OfType<IEventHandler<AjivaMouseEvent>>())
                {
                    layerStackLayer.OnEvent(ref evnt);
                    if (evnt.Handled)
                    {
                        break;
                    }
                }
            }
        
        using (Instrumentation.NamedTrace("InputSystem.SdlEvents"))
            while (SdlEvents.Reader.TryRead(out var sdlEvent))
            {
                var evnt = new AjivaEvent<SDLEvent>(ref sdlEvent, false);
                foreach (var layerStackLayer in _layerStack.Layers.OfType<IEventHandler<SDLEvent>>())
                {
                    layerStackLayer.OnEvent(ref evnt);
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

    /*void OnKeyboardOnKeyDown(IKeyboard iKeyboard, Key key, int arg3) =>
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
        WriteMouseEvent(new AjivaMouseEvent(AjivaMouseEventType.DoubleClick, button, arg3.X, arg3.Y, 0, 0));*/
    public void Dispatch(AjivaMouseEvent ajivaMouseEvent) => WriteMouseEvent(ajivaMouseEvent);

    public void Dispatch(AjivaKeyboardEvent ajivaKeyboardEvent) => WriteKeyboardEvent(ajivaKeyboardEvent);
    public void Dispatch(SDLEvent sdlEvent) => SdlEvents.Writer.TryWrite(sdlEvent);
}
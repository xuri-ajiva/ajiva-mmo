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
}

public readonly record struct AjivaKeyboardEvent(
    SDLScancode KeyCode,
    SDLKeymod Modifiers,
    AjivaKeyboardEventType IsPressed);

public ref struct AjivaEvent<T>(ref T eventData, bool handled)
{
    public readonly ref T EventData = ref eventData;
    public bool Handled = handled;
}

public interface IEventHandler<T>
{
    void OnEvent(ref AjivaEvent<T> inputEvent);
}

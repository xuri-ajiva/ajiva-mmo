using System.Diagnostics;
using RmlUiNet.Input;
using Silk.NET.Input;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Client.UI.Desktop;

public static class SilkRmlExtensions
{
    extension(Key k)
    {
        public KeyIdentifier ToRmlKeyEvent()
        {
            return k switch
            {
                Key.Backspace => KeyIdentifier.KI_BACK,
                Key.Tab => KeyIdentifier.KI_TAB,
                Key.Enter => KeyIdentifier.KI_RETURN,
                Key.ShiftLeft => KeyIdentifier.KI_LSHIFT,
                Key.ShiftRight => KeyIdentifier.KI_RSHIFT,
                Key.ControlLeft => KeyIdentifier.KI_LCONTROL,
                Key.ControlRight => KeyIdentifier.KI_RCONTROL,
                Key.AltLeft => KeyIdentifier.KI_LMENU,
                Key.AltRight => KeyIdentifier.KI_RMENU,
                Key.Pause => KeyIdentifier.KI_PAUSE,
                Key.CapsLock => KeyIdentifier.KI_CAPITAL,
                Key.Escape => KeyIdentifier.KI_ESCAPE,
                Key.Space => KeyIdentifier.KI_SPACE,
                Key.PageUp => KeyIdentifier.KI_PRIOR,
                Key.PageDown => KeyIdentifier.KI_NEXT,
                Key.End => KeyIdentifier.KI_END,
                Key.Home => KeyIdentifier.KI_HOME,
                Key.Left => KeyIdentifier.KI_LEFT,
                Key.Up => KeyIdentifier.KI_UP,
                Key.Right => KeyIdentifier.KI_RIGHT,
                Key.Down => KeyIdentifier.KI_DOWN,
                Key.Insert => KeyIdentifier.KI_INSERT,
                Key.Delete => KeyIdentifier.KI_DELETE,
                Key.Number0 => KeyIdentifier.KI_0,
                Key.Number1 => KeyIdentifier.KI_1,
                Key.Number2 => KeyIdentifier.KI_2,
                Key.Number3 => KeyIdentifier.KI_3,
                Key.Number4 => KeyIdentifier.KI_4,
                Key.Number5 => KeyIdentifier.KI_5,
                Key.Number6 => KeyIdentifier.KI_6,
                Key.Number7 => KeyIdentifier.KI_7,
                Key.Number8 => KeyIdentifier.KI_8,
                Key.Number9 => KeyIdentifier.KI_9,
                Key.A => KeyIdentifier.KI_A,
                Key.B => KeyIdentifier.KI_B,
                Key.C => KeyIdentifier.KI_C,
                Key.D => KeyIdentifier.KI_D,
                Key.E => KeyIdentifier.KI_E,
                Key.F => KeyIdentifier.KI_F,
                Key.G => KeyIdentifier.KI_G,
                Key.H => KeyIdentifier.KI_H,
                Key.I => KeyIdentifier.KI_I,
                Key.J => KeyIdentifier.KI_J,
                Key.K => KeyIdentifier.KI_K,
                Key.L => KeyIdentifier.KI_L,
                Key.M => KeyIdentifier.KI_M,
                Key.N => KeyIdentifier.KI_N,
                Key.O => KeyIdentifier.KI_O,
                Key.P => KeyIdentifier.KI_P,
                Key.Q => KeyIdentifier.KI_Q,
                Key.R => KeyIdentifier.KI_R,
                Key.S => KeyIdentifier.KI_S,
                Key.T => KeyIdentifier.KI_T,
                Key.U => KeyIdentifier.KI_U,
                Key.V => KeyIdentifier.KI_V,
                Key.W => KeyIdentifier.KI_W,
                Key.X => KeyIdentifier.KI_X,
                Key.Y => KeyIdentifier.KI_Y,
                Key.Z => KeyIdentifier.KI_Z,
                Key.F1 => KeyIdentifier.KI_F1,
                Key.F2 => KeyIdentifier.KI_F2,
                Key.F3 => KeyIdentifier.KI_F3,
                Key.F4 => KeyIdentifier.KI_F4,
                Key.F5 => KeyIdentifier.KI_F5,
                Key.F6 => KeyIdentifier.KI_F6,
                Key.F7 => KeyIdentifier.KI_F7,
                Key.F8 => KeyIdentifier.KI_F8,
                Key.F9 => KeyIdentifier.KI_F9,
                Key.F10 => KeyIdentifier.KI_F10,
                Key.F11 => KeyIdentifier.KI_F11,
                Key.F12 => KeyIdentifier.KI_F12,
                Key.F13 => KeyIdentifier.KI_F13,
                Key.F14 => KeyIdentifier.KI_F14,
                Key.F15 => KeyIdentifier.KI_F15,
                Key.F16 => KeyIdentifier.KI_F16,
                Key.F17 => KeyIdentifier.KI_F17,
                Key.F18 => KeyIdentifier.KI_F18,
                Key.F19 => KeyIdentifier.KI_F19,
                Key.F20 => KeyIdentifier.KI_F20,
                Key.F21 => KeyIdentifier.KI_F21,
                Key.F22 => KeyIdentifier.KI_F22,
                Key.F23 => KeyIdentifier.KI_F23,
                Key.F24 => KeyIdentifier.KI_F24,
                Key.NumLock => KeyIdentifier.KI_NUMLOCK,
                Key.ScrollLock => KeyIdentifier.KI_SCROLL,
                Key.Unknown => KeyIdentifier.KI_UNKNOWN,
                Key.Apostrophe => KeyIdentifier.KI_OEM_7,
                Key.Comma => KeyIdentifier.KI_OEM_COMMA,
                Key.Minus => KeyIdentifier.KI_OEM_MINUS,
                Key.Period => KeyIdentifier.KI_OEM_PERIOD,
                Key.Slash => KeyIdentifier.KI_OEM_2,
                Key.Semicolon => KeyIdentifier.KI_OEM_1,
                Key.Equal => KeyIdentifier.KI_OEM_NEC_EQUAL,
                Key.LeftBracket => KeyIdentifier.KI_OEM_4,
                Key.BackSlash => KeyIdentifier.KI_OEM_5,
                Key.RightBracket => KeyIdentifier.KI_OEM_6,
                Key.GraveAccent => KeyIdentifier.KI_OEM_3,
                Key.World1 => KeyIdentifier.KI_OEM_8,
                Key.World2 => KeyIdentifier.KI_OEM_102,
                Key.PrintScreen => KeyIdentifier.KI_SNAPSHOT,
                Key.F25 => KeyIdentifier.KI_UNKNOWN,
                Key.Keypad0 => KeyIdentifier.KI_NUMPAD0,
                Key.Keypad1 => KeyIdentifier.KI_NUMPAD1,
                Key.Keypad2 => KeyIdentifier.KI_NUMPAD2,
                Key.Keypad3 => KeyIdentifier.KI_NUMPAD3,
                Key.Keypad4 => KeyIdentifier.KI_NUMPAD4,
                Key.Keypad5 => KeyIdentifier.KI_NUMPAD5,
                Key.Keypad6 => KeyIdentifier.KI_NUMPAD6,
                Key.Keypad7 => KeyIdentifier.KI_NUMPAD7,
                Key.Keypad8 => KeyIdentifier.KI_NUMPAD8,
                Key.Keypad9 => KeyIdentifier.KI_NUMPAD9,
                Key.KeypadDecimal => KeyIdentifier.KI_DECIMAL,
                Key.KeypadDivide => KeyIdentifier.KI_DIVIDE,
                Key.KeypadMultiply => KeyIdentifier.KI_MULTIPLY,
                Key.KeypadSubtract => KeyIdentifier.KI_SUBTRACT,
                Key.KeypadAdd => KeyIdentifier.KI_ADD,
                Key.KeypadEnter => KeyIdentifier.KI_RETURN,
                Key.KeypadEqual => KeyIdentifier.KI_OEM_NEC_EQUAL,
                Key.SuperLeft => KeyIdentifier.KI_LWIN,
                Key.SuperRight => KeyIdentifier.KI_RWIN,
                Key.Menu => KeyIdentifier.KI_APPS,
                _ => throw new ArgumentOutOfRangeException(nameof(k), k, null)
            };
        }
    }

    extension(MouseButton mb)
    {
        //  /// <param name="buttonIndex">The index of the button that was pressed; 0 for the left button, 1 for right, and any others from 2 onwards.</param>
        public i32 ToRmlMouseButton()
        {
            Debug.Assert((i32)MouseButton.Unknown == -1);
            Debug.Assert((i32)MouseButton.Left == 0);
            Debug.Assert((i32)MouseButton.Right == 1);
            Debug.Assert((i32)MouseButton.Middle == 2);

            return (i32)mb;
        }
    }
}
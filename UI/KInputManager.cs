using System.Text;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

[Flags]
public enum KKeyStates : byte
{
    NONE = 0,
    PRESSED = 1,
    HELD = 1 << 1,
    RELEASED = 1 << 2,
    SHIFT = 1 << 3,
    CONTROL = 1 << 4,
    ALT = 1 << 5,
    SYSTEM = 1 << 6
};

[Flags]
public enum KMouseStates : byte
{
    NONE = 0,
    M1_DOWN = 1,
    M2_DOWN = 1 << 1,
    M3_DOWN = 1 << 2,
    M4_DOWN = 1 << 2,
    M5_DOWN = 1 << 3,
}

public class KInputManager //Sanitizes inputs, reduces boilerplate, & reduces delegate usage. 
{
    private bool _readText;
    private int _activeKeyCount;
    private KKeyStates[] _keyStates;
    private Keyboard.Key[] _activeKeys;

    public float MousePosY;
    public float ScrollDelta;
    public float MousePosX;
    public KMouseStates MouseStates;
    public KMouseStates PrevMouseStates;
    public StringBuilder StringBuilder;
    public RenderWindow Window;

    public KInputManager(RenderWindow window)
    {
        _activeKeyCount = 0;
        _activeKeys = new Keyboard.Key[Keyboard.KeyCount];
        _keyStates = new KKeyStates[Keyboard.KeyCount];

        MouseStates = 0;
        PrevMouseStates = 0;
        Array.Fill(_keyStates, KKeyStates.NONE);

        MousePosX = 0;
        MousePosY = 0;
        ScrollDelta = 0;
        StringBuilder = new(128);

        Window = window;
        //Window.SetKeyRepeatEnabled(false);
        Window.MouseMoved += MouseMoved;
        Window.MouseButtonPressed += MouseButtonPressed;
        Window.MouseButtonReleased += MouseButtonReleased;
        Window.MouseWheelScrolled += MouseScrolled;
        Window.KeyPressed += KeyPressed;
        Window.KeyReleased += KeyReleased;
        Window.TextEntered += TextEntered;
        Window.LostFocus += LostFocus;
    }

    ~KInputManager()
    {
        Window.MouseMoved -= MouseMoved;
        Window.MouseButtonPressed -= MouseButtonPressed;
        Window.MouseButtonReleased -= MouseButtonReleased;
        Window.MouseWheelScrolled -= MouseScrolled;
        Window.KeyPressed -= KeyPressed;
        Window.KeyReleased -= KeyReleased;
        Window.TextEntered -= TextEntered;
        Window.LostFocus -= LostFocus;
    }

    public void Update()
    {
        PrevMouseStates = MouseStates;

        for (int i = 0; i < _activeKeyCount; i++)
        {
            var key = (int)_activeKeys[i];

            //Removes inactive keys.
            if (_keyStates[key].HasFlag(KKeyStates.RELEASED))
            {
                //Swaps current index with last index, and decrement count so that it's out of range.
                _activeKeyCount--;
                _activeKeys[i] = _activeKeys[_activeKeyCount];
                _keyStates[key] = KKeyStates.NONE;
                i--;
            }
            else if (_keyStates[key].HasFlag(KKeyStates.PRESSED))
            {
                _keyStates[key] &= ~KKeyStates.PRESSED;
            }
        }
    }

    public Vector2f GetMousePosition(float scale = 1) =>
        new(MousePosX * scale, MousePosY * scale);

    public bool IsMousePressed(KMouseStates state) =>
        !PrevMouseStates.HasFlag(state) && MouseStates.HasFlag(state);

    public bool IsMouseDown(KMouseStates state) =>
        MouseStates.HasFlag(state);

    public bool IsMouseReleased(KMouseStates state) =>
        PrevMouseStates.HasFlag(state) && !MouseStates.HasFlag(state);

    public bool CheckKeyState(Keyboard.Key key, KKeyStates states) =>
        _keyStates[(int)key].HasFlag(states);

    public bool IsKeyPressed(Keyboard.Key key) =>
        _keyStates[(int)key].HasFlag(KKeyStates.PRESSED);

    public bool IsKeyDown(Keyboard.Key key) =>
        _keyStates[(int)key].HasFlag(KKeyStates.PRESSED) ||
        _keyStates[(int)key].HasFlag(KKeyStates.HELD);

    public bool IsKeyReleased(Keyboard.Key key) =>
        _keyStates[(int)key].HasFlag(KKeyStates.RELEASED);

    public void StartTextRead()
    {
        _readText = true;
        StringBuilder.Clear();
    }

    public string ReadText()
    {
        return StringBuilder.ToString();
    }

    public string StopTextRead()
    {
        _readText = false;
        return StringBuilder.ToString();
    }

    private void MouseMoved(object? obj, MouseMoveEventArgs args)
    {
        args.Position.Deconstruct(out int x, out int y);
        MousePosX = x;
        MousePosY = y;
    }
    private void MouseButtonPressed(object? obj, MouseButtonEventArgs args)
    {
        args.Position.Deconstruct(out int x, out int y);
        MousePosX = x;
        MousePosY = y;

        switch (args.Button)
        {
            case Mouse.Button.Left:
                MouseStates |= KMouseStates.M1_DOWN;
                break;

            case Mouse.Button.Right:
                MouseStates |= KMouseStates.M2_DOWN;
                break;

            case Mouse.Button.Middle:
                MouseStates |= KMouseStates.M3_DOWN;
                break;

            case Mouse.Button.Extra1:
                MouseStates |= KMouseStates.M4_DOWN;
                break;

            case Mouse.Button.Extra2:
                MouseStates |= KMouseStates.M5_DOWN;
                break;

            default:
                break;
        }
    }

    public void MouseButtonReleased(object? obj, MouseButtonEventArgs args)
    {
        args.Position.Deconstruct(out int x, out int y);
        MousePosX = x;
        MousePosY = y;

        switch (args.Button)
        {
            case Mouse.Button.Left:
                MouseStates &= ~KMouseStates.M1_DOWN;
                break;

            case Mouse.Button.Right:
                MouseStates &= ~KMouseStates.M2_DOWN;
                break;

            case Mouse.Button.Middle:
                MouseStates &= ~KMouseStates.M3_DOWN;
                break;

            case Mouse.Button.Extra1:
                MouseStates &= ~KMouseStates.M4_DOWN;
                break;

            case Mouse.Button.Extra2:
                MouseStates &= ~KMouseStates.M5_DOWN;
                break;

            default:
                break;
        }
    }

    private void MouseScrolled(object? obj, MouseWheelScrollEventArgs args)
    {
        args.Position.Deconstruct(out int x, out int y);
        MousePosX = x;
        MousePosY = y;
        ScrollDelta = args.Delta;
    }

    private void KeyPressed(object? obj, KeyEventArgs args)
    {
        if (args.Code == Keyboard.Key.Unknown) return;
        if (args.Code == Keyboard.Key.Backspace && StringBuilder.Length > 0)
        {
            StringBuilder.Remove(StringBuilder.Length - 1, 1);
        }

        ref var states = ref _keyStates[(int)args.Code];

        states = KKeyStates.PRESSED | KKeyStates.HELD;
        if (args.Shift) states |= KKeyStates.SHIFT;
        if (args.Control) states |= KKeyStates.CONTROL;
        if (args.Alt) states |= KKeyStates.ALT;
        if (args.System) states |= KKeyStates.SYSTEM;

        for (int i = 0; i < _activeKeyCount; i++)
        {
            //Ensures the key isn't already an active key.
            if (_activeKeys[i] == args.Code) return;
        }

        if (_activeKeyCount < _activeKeys.Length)
        {
            _activeKeys[_activeKeyCount] = args.Code;
            _activeKeyCount++;
        }
    }

    private void KeyReleased(object? obj, KeyEventArgs args)
    {
        if (args.Code == Keyboard.Key.Unknown) return;
        _keyStates[(int)args.Code] = KKeyStates.RELEASED;
    }

    private void TextEntered(object? obj, TextEventArgs args)
    {
        if (!_readText) return;
        var text = args.Unicode;

        switch (args.Unicode)
        {
            case "\b":
                break;

            default:
                StringBuilder.Append(text);
                break;
        }
    }

    private void LostFocus(object? obj, EventArgs args)
    {
        MouseStates = KMouseStates.NONE;

        for (int i = 0; i < _activeKeyCount; i++)
        {
            var key = (int)_activeKeys[i];
            _keyStates[key] = KKeyStates.RELEASED;
        }
    }
}
#if HAS_INPUTSYSTEM
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace ImGuiNET.Unity;

// Implemented features:
// [x] Platform: Clipboard support.
// [x] Platform: Mouse cursor shape and visibility. Disable with io.ConfigFlags |= ImGuiConfigFlags.NoMouseCursorChange.
// [x] Platform: Keyboard arrays indexed using InputSystem.Key codes, e.g. ImGui.IsKeyPressed(Key.Space).
// [x] Platform: Gamepad support. Enabled with io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad.
// [~] Platform: IME support.
// [~] Platform: INI settings support.

/// <summary>
/// Platform bindings for ImGui in Unity in charge of: mouse/keyboard/gamepad inputs, cursor shape, timing, windowing.
/// </summary>
sealed class ImGuiPlatformInputSystem : IImGuiPlatform
{
    Key[] _allKeys = (Key[])Enum.GetValues(typeof(Key));                    // main keys
    readonly List<char> _textInput = new List<char>();                      // accumulate text input

    readonly DearImGui _cursorShapes;                               // cursor shape definitions
    ImGuiMouseCursor _lastCursor = ImGuiMouseCursor.COUNT;                  // last cursor requested by ImGui
    Keyboard _keyboard = null;                                              // currently setup keyboard, need to reconfigure on changes

    readonly IniSettingsAsset _iniSettings;                                 // ini settings data

    readonly PlatformCallbacks _callbacks = new PlatformCallbacks
    {
        GetClipboardText = (_) => GUIUtility.systemCopyBuffer,
        SetClipboardText = (_, text) => GUIUtility.systemCopyBuffer = text,
#if IMGUI_FEATURE_CUSTOM_ASSERT
        LogAssert = (condition, file, line) => Debug.LogError($"[DearImGui] Assertion failed: '{condition}', file '{file}', line: {line}."),
        DebugBreak = () => System.Diagnostics.Debugger.Break(),
#endif
    };

    public ImGuiPlatformInputSystem(DearImGui cursorShapes, IniSettingsAsset iniSettings)
    {
        _cursorShapes = cursorShapes;
        _iniSettings = iniSettings;
        _callbacks.ImeSetInputScreenPos = (x, y) => _keyboard.SetIMECursorPosition(new Vector2(x, y));
    }

    public bool Initialize(ImGuiIOPtr io)
    {
        InputSystem.onDeviceChange += OnDeviceChange;                       // listen to keyboard device and layout changes

        io.SetBackendPlatformName("Unity Input System");                    // setup backend info and capabilities
        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;               // can honor GetMouseCursor() values
        io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;                // can honor io.WantSetMousePos requests
        // io.BackendFlags |= ImGuiBackendFlags.HasGamepad;                 // set by UpdateGamepad()

        _callbacks.Assign(io);                                              // assign platform callbacks
        io.ClipboardUserData = IntPtr.Zero;

        if (_iniSettings != null)                                           // ini settings
        {
            io.SetIniFilename(null);                                        // handle ini saving manually
            ImGui.LoadIniSettingsFromMemory(_iniSettings.Load());           // call after CreateContext(), before first call to NewFrame()
        }

        SetupKeyboard(io, Keyboard.current);                                // sets key mapping, text input, and IME

        return true;
    }

    public void Shutdown(ImGuiIOPtr io)
    {
        _callbacks.Unset(io);
        io.SetBackendPlatformName(null);

        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    public void PrepareFrame(ImGuiIOPtr io, Rect displayRect)
    {
        Assert.IsTrue(io.Fonts.IsBuilt(), "Font atlas not built! Generally built by the renderer. Missing call to renderer NewFrame() function?");

        io.DisplaySize = new System.Numerics.Vector2(displayRect.width, displayRect.height);// setup display size (every frame to accommodate for window resizing)
        // TODO: dpi aware, scale, etc

        io.DeltaTime = Time.unscaledDeltaTime;                              // setup timestep

        // input
        UpdateKeyboard(io, Keyboard.current);                               // update keyboard state
        UpdateMouse(io, Mouse.current);                                     // update mouse state
        UpdateCursor(io, ImGui.GetMouseCursor());                           // update Unity cursor with the cursor requested by ImGui
        UpdateGamepad(io, Gamepad.current);                                 // update game controllers (if enabled and available)

        // ini settings
        if (_iniSettings != null && io.WantSaveIniSettings)
        {
            _iniSettings.Save(ImGui.SaveIniSettingsToMemory());
            io.WantSaveIniSettings = false;
        }
    }

    void SetupKeyboard(ImGuiIOPtr io, Keyboard kb)
    {
        if (_keyboard != null)
        {
            _keyboard.onTextInput -= (c) => io.AddInputCharacter(c);
        }
        _keyboard = kb;
        _keyboard.onTextInput += (c) => io.AddInputCharacter(c);
    }

    void UpdateKeyboard(ImGuiIOPtr io, Keyboard keyboard)
    {
        if (keyboard == null)
            return;

        foreach (var key in _allKeys)
        {
            if (TryMapKeys(key, out ImGuiKey imguikey))
            {
                io.AddKeyEvent(imguikey, keyboard[key].isPressed);
            }
        }

        io.AddKeyEvent(ImGuiKey.ModShift, keyboard[Key.LeftShift].isPressed || keyboard[Key.RightShift].isPressed);
        io.AddKeyEvent(ImGuiKey.ModCtrl, keyboard[Key.LeftCtrl].isPressed || keyboard[Key.RightCtrl].isPressed);
        io.AddKeyEvent(ImGuiKey.ModAlt, keyboard[Key.LeftAlt].isPressed || keyboard[Key.RightAlt].isPressed);
        io.AddKeyEvent(ImGuiKey.ModSuper, keyboard[Key.LeftMeta].isPressed || keyboard[Key.RightMeta].isPressed);
    }

    static void UpdateMouse(ImGuiIOPtr io, Mouse mouse)
    {
        if (mouse == null)
            return;

        if (io.WantSetMousePos) // set Unity mouse position if requested
            mouse.WarpCursorPosition(io.MousePos.ToUnity());

        Vector2 mouseScroll = mouse.scroll.ReadValue() / 120f;
        var pos = ImGuiUn.ScreenToImGui(mouse.position.ReadValue());

        io.AddMousePosEvent(pos.X, pos.Y);
        io.AddMouseButtonEvent(0, mouse.leftButton.isPressed);
        io.AddMouseButtonEvent(1, mouse.rightButton.isPressed);
        io.AddMouseButtonEvent(2, mouse.middleButton.isPressed);
        io.AddMouseButtonEvent(3, mouse.forwardButton.isPressed);
        io.AddMouseButtonEvent(4, mouse.backButton.isPressed);

        io.AddMouseWheelEvent(mouseScroll.x, mouseScroll.y);
    }

    static void UpdateGamepad(ImGuiIOPtr io, Gamepad gamepad)
    {
        io.BackendFlags = gamepad == null
            ? io.BackendFlags & ~ImGuiBackendFlags.HasGamepad
            : io.BackendFlags | ImGuiBackendFlags.HasGamepad;

        if (gamepad == null || (io.ConfigFlags & ImGuiConfigFlags.NavEnableGamepad) == 0)
            return;

        io.AddKeyEvent(ImGuiKey.GamepadStart, gamepad.startButton.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadBack, gamepad.selectButton.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadFaceLeft, gamepad.buttonWest.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadFaceRight, gamepad.buttonEast.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadFaceUp, gamepad.buttonNorth.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadFaceDown, gamepad.buttonSouth.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadDpadLeft, gamepad.dpad.left.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadDpadRight, gamepad.dpad.right.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadDpadUp, gamepad.dpad.up.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadDpadDown, gamepad.dpad.down.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadL1, gamepad.leftShoulder.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadR1, gamepad.rightShoulder.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadL2, gamepad.leftTrigger.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadR2, gamepad.rightTrigger.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadL3, gamepad.leftStickButton.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadR3, gamepad.rightStickButton.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadLStickLeft, gamepad.leftStick.left.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadLStickRight, gamepad.leftStick.right.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadLStickUp, gamepad.leftStick.up.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadLStickDown, gamepad.leftStick.down.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadRStickLeft, gamepad.rightStick.left.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadRStickRight, gamepad.rightStick.right.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadRStickUp, gamepad.rightStick.up.isPressed);
        io.AddKeyEvent(ImGuiKey.GamepadRStickDown, gamepad.rightStick.down.isPressed);
    }

    void UpdateCursor(ImGuiIOPtr io, ImGuiMouseCursor cursor)
    {
        if (io.MouseDrawCursor)
            cursor = ImGuiMouseCursor.None;

        if (_lastCursor == cursor)
            return;
        if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0)
            return;

        _lastCursor = cursor;
        Cursor.visible = cursor != ImGuiMouseCursor.None;                   // hide cursor if ImGui is drawing it or if it wants no cursor
        if (_cursorShapes != null)
            Cursor.SetCursor(_cursorShapes[cursor].texture, _cursorShapes[cursor].hotspot, CursorMode.Auto);
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Keyboard kb)
        {
            if (change == InputDeviceChange.ConfigurationChanged)           // keyboard layout change, remap main keys
                SetupKeyboard(ImGui.GetIO(), kb);
            if (Keyboard.current != _keyboard)                              // keyboard device changed, setup again
                SetupKeyboard(ImGui.GetIO(), Keyboard.current);
        }
    }

    private bool TryMapKeys(Key key, out ImGuiKey imguikey)
    {
        if (key <= 0 || ((int)key - 1) >= Keyboard.KeyCount)
        {
            imguikey = ImGuiKey.None; return false;
        }
        imguikey = key switch
        {
            Key.Space => ImGuiKey.Space,
            Key.Enter => ImGuiKey.Enter,
            Key.Tab => ImGuiKey.Tab,
            Key.Backquote => ImGuiKey.GraveAccent,
            Key.Quote => ImGuiKey.Apostrophe,
            Key.Semicolon => ImGuiKey.Semicolon,
            Key.Comma => ImGuiKey.Comma,
            Key.Period => ImGuiKey.Period,
            Key.Slash => ImGuiKey.Slash,
            Key.Backslash => ImGuiKey.Backslash,
            Key.LeftBracket => ImGuiKey.LeftBracket,
            Key.RightBracket => ImGuiKey.RightBracket,
            Key.Minus => ImGuiKey.Minus,
            Key.Equals => ImGuiKey.Equal,
            >= Key.A and <= Key.Z => ImGuiKey.A + (key - Key.A),
            >= Key.Digit1 and <= Key.Digit9 => ImGuiKey._1 + (key - Key.Digit1),
            Key.Digit0 => ImGuiKey._0,
            Key.LeftShift => ImGuiKey.LeftShift,  // Map to actual shift keys?
            Key.RightShift => ImGuiKey.RightShift,
            Key.LeftAlt => ImGuiKey.LeftAlt,
            Key.RightAlt => ImGuiKey.RightAlt,
            // Key.AltGr // no one likes u alt gr
            Key.LeftCtrl => ImGuiKey.LeftCtrl,
            Key.RightCtrl => ImGuiKey.RightCtrl,
            Key.LeftMeta => ImGuiKey.LeftSuper,
            Key.RightMeta => ImGuiKey.RightSuper,
            Key.ContextMenu => ImGuiKey.Menu,
            Key.Escape => ImGuiKey.Escape,
            Key.LeftArrow => ImGuiKey.LeftArrow,
            Key.RightArrow => ImGuiKey.RightArrow,
            Key.UpArrow => ImGuiKey.UpArrow,
            Key.DownArrow => ImGuiKey.DownArrow,
            Key.Backspace => ImGuiKey.Backspace,
            Key.PageDown => ImGuiKey.PageDown,
            Key.PageUp => ImGuiKey.PageUp,
            Key.Home => ImGuiKey.Home,
            Key.End => ImGuiKey.End,
            Key.Insert => ImGuiKey.Insert,
            Key.Delete => ImGuiKey.Delete,
            Key.CapsLock => ImGuiKey.CapsLock,
            Key.NumLock => ImGuiKey.NumLock,
            Key.PrintScreen => ImGuiKey.PrintScreen,
            Key.ScrollLock => ImGuiKey.ScrollLock,
            Key.Pause => ImGuiKey.Pause,
            Key.NumpadEnter => ImGuiKey.KeypadEnter,
            Key.NumpadDivide => ImGuiKey.KeypadDivide,
            Key.NumpadMultiply => ImGuiKey.KeypadMultiply,
            Key.NumpadPlus => ImGuiKey.KeypadAdd,
            Key.NumpadMinus => ImGuiKey.KeypadSubtract,
            Key.NumpadPeriod => ImGuiKey.KeypadDecimal,
            Key.NumpadEquals => ImGuiKey.KeypadEqual,
            >= Key.Numpad0 and <= Key.Numpad9 => ImGuiKey.Keypad0 + (key - Key.Numpad0),
            >= Key.F1 and <= Key.F12 => ImGuiKey.F1 + (key - Key.F1),

            // Key.OEM1-5
            // Key.IMESelected

            _ => ImGuiKey.None,
        };

        return imguikey != ImGuiKey.None;
    }
}
#endif

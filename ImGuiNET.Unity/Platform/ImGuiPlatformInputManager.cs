using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace ImGuiNET.Unity
{
    // Implemented features:
    // [x] Platform: Clipboard support.
    // [x] Platform: Mouse cursor shape and visibility. Disable with io.ConfigFlags |= ImGuiConfigFlags.NoMouseCursorChange.
    // [x] Platform: Keyboard arrays indexed using KeyCode codes, e.g. ImGui.IsKeyPressed(KeyCode.Space).
    // [ ] Platform: Gamepad support. Enabled with io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad.
    // [~] Platform: IME support.
    // [~] Platform: INI settings support.

    /// <summary>
    /// Platform bindings for ImGui in Unity in charge of: mouse/keyboard/gamepad inputs, cursor shape, timing, windowing.
    /// </summary>
    sealed class ImGuiPlatformInputManager : IImGuiPlatform
    {
        KeyCode[] _allKeys = (KeyCode[])Enum.GetValues(typeof(KeyCode));        // main keys
        readonly Event _e = new Event();                                        // to get text input

        readonly DearImGui _cursorShapes;                               // cursor shape definitions
        ImGuiMouseCursor _lastCursor = ImGuiMouseCursor.COUNT;                  // last cursor requested by ImGui

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

        public ImGuiPlatformInputManager(DearImGui cursorShapes, IniSettingsAsset iniSettings)
        {
            _cursorShapes = cursorShapes;
            _iniSettings = iniSettings;
            _callbacks.ImeSetInputScreenPos = (x, y) => Input.compositionCursorPos = new Vector2(x, y);
        }

        public unsafe bool Initialize(ImGuiIOPtr io)
        {
            io.SetBackendPlatformName("Unity Input Manager");                   // setup backend info and capabilities
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;               // can honor GetMouseCursor() values
            io.BackendFlags &= ~ImGuiBackendFlags.HasSetMousePos;               // can't honor io.WantSetMousePos requests
            // io.BackendFlags |= ImGuiBackendFlags.HasGamepad;                 // set by UpdateGamepad()

            _callbacks.Assign(io);                                              // assign platform callbacks

            if (_iniSettings != null)                                           // ini settings
            {
                io.SetIniFilename(null);                                        // handle ini saving manually
                ImGui.LoadIniSettingsFromMemory(_iniSettings.Load());           // call after CreateContext(), before first call to NewFrame()
            }

            return true;
        }

        public void Shutdown(ImGuiIOPtr io)
        {
            _callbacks.Unset(io);
            io.SetBackendPlatformName(null);
        }

        public void PrepareFrame(ImGuiIOPtr io, Rect displayRect)
        {
            Assert.IsTrue(io.Fonts.IsBuilt(), "Font atlas not built! Generally built by the renderer. Missing call to renderer NewFrame() function?");

            io.DisplaySize = new System.Numerics.Vector2(displayRect.width, displayRect.height);// setup display size (every frame to accommodate for window resizing)
            // TODO: dpi aware, scale, etc

            io.DeltaTime = Time.unscaledDeltaTime;                              // setup timestep

            // input
            UpdateKeyboard(io);                                                 // update keyboard state
            UpdateMouse(io);                                                    // update mouse state
            UpdateCursor(io, ImGui.GetMouseCursor());                           // update Unity cursor with the cursor requested by ImGui

            // ini settings
            if (_iniSettings != null && io.WantSaveIniSettings)
            {
                _iniSettings.Save(ImGui.SaveIniSettingsToMemory());
                io.WantSaveIniSettings = false;
            }
        }

        void UpdateKeyboard(ImGuiIOPtr io)
        {
            foreach (var key in _allKeys)
            {
                if (TryMapKeys(key, out ImGuiKey imguikey))
                {
                    io.AddKeyEvent(imguikey, Input.GetKey(key));
                }
            }

            // text input
            while (Event.PopEvent(_e))
                if (_e.rawType == EventType.KeyDown && _e.character != 0 && _e.character != '\n')
                    io.AddInputCharacter(_e.character);
        }

        static void UpdateMouse(ImGuiIOPtr io)
        {
            io.MousePos = ImGuiUn.ScreenToImGui(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

            io.MouseWheel  = Input.mouseScrollDelta.y;
            io.MouseWheelH = Input.mouseScrollDelta.x;

            io.MouseDown[0] = Input.GetMouseButton(0);
            io.MouseDown[1] = Input.GetMouseButton(1);
            io.MouseDown[2] = Input.GetMouseButton(2);
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

        static bool TryMapKeys(KeyCode key, out ImGuiKey imguikey)
        {
            //Special case not handed in the switch...
            //If the actual key we put in is "None", return none and true. 
            //otherwise, return none and false.
            if (key == KeyCode.None)
            {
                imguikey = ImGuiKey.None;
                return true;
            }
            imguikey = key switch
            {
                KeyCode.Backspace => ImGuiKey.Backspace,
                KeyCode.Delete => ImGuiKey.Delete,
                KeyCode.Tab => ImGuiKey.Tab,
                //KeyCode.Clear
                KeyCode.Return => ImGuiKey.Enter,
                KeyCode.Pause => ImGuiKey.Pause,
                KeyCode.Escape => ImGuiKey.Escape,
                KeyCode.Space => ImGuiKey.Space,
                >= KeyCode.Keypad0 and <= KeyCode.KeypadEquals => ImGuiKey.Keypad0 + (key - KeyCode.Keypad0),
                KeyCode.UpArrow => ImGuiKey.UpArrow,
                KeyCode.DownArrow => ImGuiKey.DownArrow,
                KeyCode.RightArrow => ImGuiKey.RightArrow,
                KeyCode.LeftArrow => ImGuiKey.LeftArrow,
                KeyCode.Insert => ImGuiKey.Insert,
                KeyCode.Home => ImGuiKey.Home,
                KeyCode.End => ImGuiKey.End,
                KeyCode.PageUp => ImGuiKey.PageUp,
                KeyCode.PageDown => ImGuiKey.PageDown,
                >= KeyCode.F1 and <= KeyCode.F15 => ImGuiKey.F1 + (key - KeyCode.F1),
                >= KeyCode.Alpha0 and <= KeyCode.Alpha9 => ImGuiKey._1 + (key - KeyCode.Alpha0),
                // KeyCode.Exclaim => ImGuiKey._1,
                // KeyCode.DoubleQuote => ImGuiKey.Apostrophe,
                // KeyCode.Hash => ImGuiKey._3,
                // KeyCode.Dollar => ImGuiKey._4,
                // KeyCode.Percent => ImGuiKey._5,
                // KeyCode.Ampersand => ImGuiKey._7,
                KeyCode.Quote => ImGuiKey.Apostrophe,
                // KeyCode.LeftParen => ImGuiKey._9,
                // KeyCode.RightParen => ImGuiKey._0,
                // KeyCode.Asterisk => ImGuiKey._8,
                // KeyCode.Plus => ImGuiKey.Equal,
                KeyCode.Comma => ImGuiKey.Comma,
                KeyCode.Minus => ImGuiKey.Minus,
                KeyCode.Period => ImGuiKey.Period,
                KeyCode.Slash => ImGuiKey.Slash,
                // KeyCode.Colon => ImGuiKey.Semicolon,
                KeyCode.Semicolon => ImGuiKey.Semicolon,
                // KeyCode.Less => ImGuiKey.Comma,
                KeyCode.Equals => ImGuiKey.Equal,
                // KeyCode.Greater => ImGuiKey.Period,
                // KeyCode.Question => ImGuiKey.Slash,
                // KeyCode.At => ImGuiKey._2,
                KeyCode.LeftBracket => ImGuiKey.LeftBracket,
                KeyCode.Backslash => ImGuiKey.Backslash,
                KeyCode.RightBracket => ImGuiKey.RightBracket,
                // KeyCode.Caret => ImGuiKey._6,
                // KeyCode.Underscore => ImGuiKey.Minus,
                KeyCode.BackQuote => ImGuiKey.GraveAccent,
                >= KeyCode.A and <= KeyCode.Z => ImGuiKey.A + (key - KeyCode.A),
                // KeyCode.LeftCurlyBracket => ImGuiKey.LeftBracket,
                // KeyCode.Pipe => ImGuiKey.Backslash,
                // KeyCode.RightCurlyBracket => ImGuiKey.RightBracket,
                // KeyCode.Tilde => ImGuiKey.GraveAccent,
                KeyCode.Numlock => ImGuiKey.NumLock,
                KeyCode.CapsLock => ImGuiKey.CapsLock,
                KeyCode.ScrollLock => ImGuiKey.ScrollLock,
                KeyCode.RightShift => ImGuiKey.ModShift,
                KeyCode.LeftShift => ImGuiKey.ModShift,
                // KeyCode.RightShift => ImGuiKey.RightShift,
                // KeyCode.LeftShift => ImGuiKey.LeftShift,  // Map to actual shift keys?
                KeyCode.RightControl => ImGuiKey.ModCtrl,
                KeyCode.LeftControl => ImGuiKey.ModCtrl,
                // KeyCode.RightControl => ImGuiKey.RightCtrl,
                // KeyCode.LeftControl => ImGuiKey.LeftCtrl,
                KeyCode.RightAlt => ImGuiKey.ModAlt,
                KeyCode.LeftAlt => ImGuiKey.ModAlt,
                // KeyCode.RightAlt => ImGuiKey.RightAlt,
                // KeyCode.LeftAlt => ImGuiKey.LeftAlt,
                KeyCode.LeftCommand => ImGuiKey.ModSuper,
                KeyCode.LeftWindows => ImGuiKey.ModSuper,
                KeyCode.RightCommand => ImGuiKey.ModSuper,
                KeyCode.RightWindows => ImGuiKey.ModSuper,
                // KeyCode.LeftCommand => ImGuiKey.LeftSuper,
                // KeyCode.LeftWindows => ImGuiKey.LeftSuper,
                // KeyCode.RightCommand => ImGuiKey.RightSuper,
                // KeyCode.RightWindows => ImGuiKey.RightSuper,
                // KeyCode.AltGr // no one likes u alt gr
                // KeyCode.Help
                KeyCode.Print => ImGuiKey.PrintScreen,
                // KeyCode.SysReq => ImGuiKey.PrintScreen,
                // KeyCode.Break => ImGuiKey.Pause,
                KeyCode.Menu => ImGuiKey.Menu,
                
                // Mouse 'keys'
                // Joystick 'keys'

                _ => ImGuiKey.None,
            };

            return imguikey != ImGuiKey.None;
        }
    }
}

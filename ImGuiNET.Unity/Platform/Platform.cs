using UnityEngine;

namespace ImGuiNET.Unity
{
    public enum PlatformType
    {
        InputManager = 0,
        InputSystem = 1,
    }

    static class Platform
    {
        public static bool IsAvailable(PlatformType type)
        {
            switch (type)
            {
                case PlatformType.InputManager: return true;
#if HAS_INPUTSYSTEM
                case PlatformType.InputSystem: return true;
#endif
                default: return false;
            }
        }

        public static IImGuiPlatform Create(PlatformType type, DearImGui cursors, IniSettingsAsset iniSettings)
        {
            switch (type)
            {
                case PlatformType.InputManager: return new ImGuiPlatformInputManager(cursors, iniSettings);
#if HAS_INPUTSYSTEM
                case PlatformType.InputSystem: return new ImGuiPlatformInputSystem(cursors, iniSettings);
#endif
                default:
                    Debug.LogError($"[DearImGui] {type} platform not available.");
                    return null;
            }
        }
    }
}

# ImGuiUnityInject

An edit of [realgamessoftware/dear-imgui-unity](https://github.com/realgamessoftware/dear-imgui-unity) to work with game modding.

It uses an asset bundle to load the shaders and cursor textures.

# Usage

1. Reference ImGuiUnityInject.dll in your project.
2. Ensure the injector instance exists.
```cs
ImGuiUnityInjector.EnsureExists();
```
3. Run your ImGui code by subscribing to the global layout event.
```cs
ImGuiUn.Layout += () => {
    ImGui.ShowDemoWindow();
};
```

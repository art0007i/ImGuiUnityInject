# ImGuiUnityInject

An edit of [realgamessoftware/dear-imgui-unity](https://github.com/realgamessoftware/dear-imgui-unity) to work with game modding.

It uses an asset bundle to load the shaders and cursor textures.

# Usage for modders

1. Add the dlls from [./Plugins](./Plugins) to the `Game_Data/Plugins/x86_64` folder of the game you are modding
2. Reference `ImGuiUnityInject.dll` in your project.
3. Ensure the injector instance exists.
```cs
ImGuiUnityInjector.EnsureExists();
```
4. Run your ImGui code by subscribing to the global layout event.
```cs
ImGuiUn.Layout += () => {
    ImGui.ShowDemoWindow();
};
```

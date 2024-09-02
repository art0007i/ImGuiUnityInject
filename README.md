# ImGuiUnityInject

An edit of [realgamessoftware/dear-imgui-unity](https://github.com/realgamessoftware/dear-imgui-unity) to work with game modding.

It uses an asset bundle to load the shaders and cursor textures.

# Usage for modders

Add the dlls from [./Plugins](./Plugins) to the `Game_Data/Plugins/x86_64` folder of the game you are modding.
Everyone who downloads your mod will need to do this step as well. This is slightly annoying but I don't know of a better way to do it.

Simple Example:
```cs
ImGuiInstance.GetOrCreate().Layout += () => {
    ImGui.ShowDemoWindow();
}
```

Advanced Example:
```cs
// This function creates a new ImGuiInstance, or gives you an existing one with the same name.
ImGuiInstance.GetOrCreate(
// This library allows multiple imgui instances exist. Each instance is identified by a string.
    "test",
// This is the callback which is called when the DearImGui component is ready.
// It will be called on both new and existing instances, you can tell which one it is by the isNew parameter
    (gui, isNew) => {
        // You can return early if isNew is false to not overwrite whatever the instance creator did
        if(!isNew) return;
        // This is the place to edit initial parameters of the DearImGui object
        // Most of these don't do anything after the DearImGui object is enabled
        // You can call DearImGui.Reload() to reapply the initial settings
        gui._initialConfiguration.TextCursorBlink = false;
        // At the end of the callback make sure to enable the DearImGui object otherwise it won't work
        gui.enabled = true;
    }
).Layout += () =>
{
    ImGui.Begin("Example Window");
    ImGui.Text("Hello World");
    ImGui.End();
};
```

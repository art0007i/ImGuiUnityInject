using ImGuiNET.Unity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ImGuiUnityInject;

public class ImGuiInstance
{
    private static Dictionary<string, ImGuiInstance> GuiInstances = new();

    private DearImGui _imGui;
    private event Action<DearImGui> _imGuiAvailable;
    public void GetImGui(Action<DearImGui> callback)
    {
        if (_imGui == null)
            _imGuiAvailable += callback;
        else
            callback(_imGui);
    }

    /// <summary>
    /// Creates a new ImGuiInstance with the "global" name or returns it if it already exists.
    /// </summary>
    /// <param name="onReady">If this argument is present the DearImGui component must be enabled manually. Otherwise it will be enabled automatically.</param>
    /// <returns></returns>
    public static ImGuiInstance GetOrCreate(ImGuiReady onReady) => GetOrCreate("global", onReady);
    /// <summary>
    /// Creates a new ImGuiInstance or returns one if it already exists.
    /// </summary>
    /// <param name="onReady">If this argument is present the DearImGui component must be enabled manually. Otherwise it will be enabled automatically.</param>
    /// <returns></returns>
    public static ImGuiInstance GetOrCreate(string name = "global", ImGuiReady onReady = null)
    {
        if (GuiInstances.TryGetValue(name, out var instance))
        {
            instance.GetImGui((gui) => onReady(gui, false));
            return instance;
        }

        instance = new ImGuiInstance();
        GuiInstances.Add(name, instance);
        ImGuiUnityInjector.Instance.GetImGuiPrefab((prefab) =>
        {
            var obj = GameObject.Instantiate(prefab, ImGuiUnityInjector.Instance.transform);
            var gui = obj.GetComponent<DearImGui>();
            instance._imGui = gui;

            gui.Layout += instance.Layout;
            
            if(onReady != null) onReady(gui, true);
            else gui.enabled = true;
            if(instance._imGuiAvailable != null) instance._imGuiAvailable(gui);
        });
        return instance;
    }

    public event Action Layout;
}

public delegate void ImGuiReady(DearImGui imGui, bool isNewInstance);

using ImGuiNET.Unity;
using System;
using UnityEngine;

namespace ImGuiUnityInject;

public class ImGuiInstance
{
    /// <summary>
    /// Creates a new ImGuiInstance
    /// </summary>
    /// <param name="onReady">If this argument is present the DearImGui component must be enabled manually. Otherwise it will be enabled automatically.</param>
    /// <returns></returns>
    public static ImGuiInstance Create(Action<DearImGui> onReady = null)
    {
        ImGuiInstance instance = new ImGuiInstance();
        ImGuiUnityInjector.Instance.GetImGuiPrefab((prefab) =>
        {
            var obj = GameObject.Instantiate(prefab, ImGuiUnityInjector.Instance.transform);
            var gui = obj.GetComponent<DearImGui>();

            gui.Layout += instance.Layout;
            
            if(onReady != null) onReady(gui);
            else gui.enabled = true;
        });
        return instance;
    }

    public event Action Layout;
}

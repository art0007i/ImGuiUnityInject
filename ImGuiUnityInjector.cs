using System.Collections;
using UnityEngine;

namespace ImGuiUnityInject;

public static class ImGuiUnityInjector
{
    public static void EnsureExists()
    {
        var obj = new GameObject("ImGuiUnityInject");
        Object.DontDestroyOnLoad(obj);
        _instance = obj.AddComponent<ImGuiUnityInjectorScript>();
    }

    internal static ImGuiUnityInjectorScript _instance;
    public static ImGuiUnityInjectorScript Instance
    {
        get
        {
            if (_instance == null)
            {
                EnsureExists();
            }
            return _instance;
        }
    }
}

public class ImGuiUnityInjectorScript : MonoBehaviour
{
    void Start()
    {
        ImGuiUnityInjector._instance = this;
        StartCoroutine(LoadBundle());
    }
    IEnumerator LoadBundle()
    {
        var bundleRequest = AssetBundle.LoadFromMemoryAsync(Resources.assetbundle);
        bundleRequest.completed += (op) =>
        {
            var bundle = bundleRequest.assetBundle;
            var imGui = bundle.LoadAsset<GameObject>("ImGuiPrefab");
            Instantiate(imGui, transform);
        };
        yield return bundleRequest;
        Debug.Log("ImGuiUnityInject assets loaded");
        yield break;
    }
}

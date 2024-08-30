using System;
using System.Collections;
using UnityEngine;

namespace ImGuiUnityInject;


internal class ImGuiUnityInjector : MonoBehaviour
{
    GameObject ImGuiPrefab = null;
    event Action<GameObject> PrefabAvailable;

    public void GetImGuiPrefab(Action<GameObject> OnPrefabAvailable)
    {
        if(ImGuiPrefab != null)
        {
            OnPrefabAvailable(ImGuiPrefab);
            return;
        }
        else
        {
            PrefabAvailable += OnPrefabAvailable;
        }
    }

    static void EnsureExists()
    {
        if (_instance != null) return;
        var obj = new GameObject("ImGuiUnityInject");
        DontDestroyOnLoad(obj);
        _instance = obj.AddComponent<ImGuiUnityInjector>();
    }

    static ImGuiUnityInjector _instance;

    internal static ImGuiUnityInjector Instance
    {
        get {
            if (_instance == null) EnsureExists();
            return _instance;
        }
    }

    void Start()
    {
        _instance = this;
        StartCoroutine(LoadBundle());
    }
    IEnumerator LoadBundle()
    {
        var bundleRequest = AssetBundle.LoadFromMemoryAsync(Resources.assetbundle);
        bundleRequest.completed += (op) =>
        {
            var bundle = bundleRequest.assetBundle;
            var imGui = bundle.LoadAsset<GameObject>("ImGuiPrefab");
            ImGuiPrefab = imGui;
            PrefabAvailable(imGui);
            //Instantiate(imGui, transform);
        };
        yield return bundleRequest;
        Debug.Log("ImGuiUnityInject assets loaded");
        yield break;
    }
}

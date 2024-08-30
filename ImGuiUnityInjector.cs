using System.Collections;
using UnityEngine;

namespace ImGuiUnityInject;

public class ImGuiUnityInjector : MonoBehaviour
{
    public static void EnsureExists()
    {
        var obj = new GameObject("ImGuiUnityInject");
        DontDestroyOnLoad(obj);
        _instance = obj.AddComponent<ImGuiUnityInjector>();
    }

    private static ImGuiUnityInjector _instance;
    public static ImGuiUnityInjector Instance {
        get { 
            if (_instance == null) {
                EnsureExists();
            }
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
            Instantiate(imGui, transform);
        };
        yield return bundleRequest;
        Debug.Log("ImGuiUnityInject assets loaded");
        yield break;
    }
}

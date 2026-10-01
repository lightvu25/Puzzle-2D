using UnityEngine;

public static class FirebaseBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void SpawnService()
    {
        if (FirebaseService.Instance != null) return;

        var go = new GameObject(nameof(FirebaseService));
        go.AddComponent<FirebaseService>();
        Object.DontDestroyOnLoad(go);
    }
}

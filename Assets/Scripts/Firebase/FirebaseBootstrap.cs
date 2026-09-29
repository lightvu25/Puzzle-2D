using UnityEngine;

/// <summary>
/// Spawns the FirebaseService GameObject before the first scene loads so the
/// Firebase layer initializes independently of any scene wiring.
/// Infrastructure only — gameplay code never references this.
/// </summary>
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

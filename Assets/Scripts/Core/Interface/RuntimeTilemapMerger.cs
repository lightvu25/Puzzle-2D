using UnityEngine;

// RuntimeTilemapMerger.cs — STUB
// Merges tilemap rooms at runtime. Implement with your actual tilemap merging logic.
public class RuntimeTilemapMerger : MonoBehaviour
{
    public static RuntimeTilemapMerger Instance { get; private set; }

    public bool IsMerging { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}

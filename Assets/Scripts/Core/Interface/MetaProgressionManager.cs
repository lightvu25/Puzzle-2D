using UnityEngine;

// MetaProgressionManager.cs — STUB
// Manages unlocked meta-progression skills across runs.
// Implement with your actual meta-progression logic.
public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool HasSkill(string skillID) => false;
}

using UnityEngine;

// RunManager.cs — cleaned for hybrid puzzle project.
// All roguelike run/blessing logic (BlessingData, RunData, StatBonusSystem)
// has been removed. This shell is kept in case you want to track level
// progression or timed runs in your puzzle game.
public class RunManager : MonoBehaviour
{
    private static RunManager _instance;
    public static RunManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<RunManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("RunManager");
                    _instance = go.AddComponent<RunManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(this); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    // ── Add puzzle run/level tracking here ────────────────────────────────
    // e.g. track elapsed time, moves taken, level index, etc.
}

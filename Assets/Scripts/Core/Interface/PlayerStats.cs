using UnityEngine;

// PlayerStats.cs — STUB
// Tracks player run stats (gold, health, currencies).
// Implement with your actual stats logic.
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void AddGold(int amount) { }
    public void ResetRunCurrencies() { }
}

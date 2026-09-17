using System;
using UnityEngine;

// PlayerInteract.cs — STUB
// Minimal singleton stub so scripts that referenced the roguelike PlayerInteract compile.
// Replace with your actual player interaction logic for the puzzle game.
[DisallowMultipleComponent]
public class PlayerInteract : MonoBehaviour
{
    public static PlayerInteract Instance { get; private set; }

    // ── Events (kept so existing event subscriptions compile) ──────────────
    public event EventHandler OnCoinPickup;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    public enum State { Normal, Dead, Interacting }

    public class OnCoinPickupEventArgs : EventArgs
    {
        public GameObject coinPickup;
    }

    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    // ──────────────────────────────────────────────────────────────────────
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

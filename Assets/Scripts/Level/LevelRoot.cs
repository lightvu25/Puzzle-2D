using System;
using UnityEngine;

/// <summary>
/// Marker and coordinator for a maze Layout Prefab.
/// This component MUST be on the root GameObject of every Layout Prefab.
///
/// Responsibilities:
///   - Cache all gameplay elements in the layout (collectibles, hazards,
///     exits, maze tiles, cracked blocks, spawn point) at Awake.
///   - Forward gameplay notifications (pickup collected, exit reached,
///     hazard hit) to listeners like the ObjectiveSystem and
///     LevelFlowController.
///   - Coordinate a level-wide reset without knowing about LevelLoader
///     or LevelData.
/// </summary>
public class LevelRoot : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  Events
    // ─────────────────────────────────────────────────────────────

    /// <summary>Fired when the player collects a Collectible.</summary>
    public event Action<Collectible> OnCollectibleCollected;

    /// <summary>Fired when the player reaches a LevelExit.</summary>
    public event Action<LevelExit> OnExitReached;

    /// <summary>Fired when the player touches a Hazard.</summary>
    public event Action<Hazard> OnHazardTriggered;

    // ─────────────────────────────────────────────────────────────
    //  Cached References (populated at Awake)
    // ─────────────────────────────────────────────────────────────

    private Collectible[] collectibles;
    private Hazard[] hazards;
    private LevelExit[] exits;
    private MazeTile[] mazeTiles;
    private CrackedBlock[] crackedBlocks;
    private PlayerSpawnPoint spawnPoint;

    private int collectedCount;

    // ─────────────────────────────────────────────────────────────
    //  Public Accessors
    // ─────────────────────────────────────────────────────────────

    public Collectible[] Collectibles => collectibles;
    public Hazard[] Hazards => hazards;
    public LevelExit[] Exits => exits;

    /// <summary>World position where the player (re)spawns.</summary>
    public Vector3 PlayerSpawnPosition => spawnPoint != null ? spawnPoint.Position : transform.position;

    /// <summary>Total collectibles placed in this layout.</summary>
    public int TotalCollectibles => collectibles != null ? collectibles.Length : 0;

    /// <summary>Collectibles picked up since the last reset.</summary>
    public int CollectedCount => collectedCount;

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        EnsureInitialized();
    }

    public void EnsureInitialized()
    {
        if (collectibles != null) return;

        collectibles = GetComponentsInChildren<Collectible>(includeInactive: true);
        hazards = GetComponentsInChildren<Hazard>(includeInactive: true);
        exits = GetComponentsInChildren<LevelExit>(includeInactive: true);
        mazeTiles = GetComponentsInChildren<MazeTile>(includeInactive: true);
        crackedBlocks = GetComponentsInChildren<CrackedBlock>(includeInactive: true);
        spawnPoint = GetComponentInChildren<PlayerSpawnPoint>(includeInactive: true);

        if (spawnPoint == null)
            Debug.LogWarning($"[LevelRoot] '{name}': No PlayerSpawnPoint found — the player will not be repositioned.", this);
        if (exits == null || exits.Length == 0)
            Debug.LogWarning($"[LevelRoot] '{name}': No LevelExit found — ReachExit objectives can never complete.", this);
    }

    // ─────────────────────────────────────────────────────────────
    //  Notifications (called by gameplay elements)
    // ─────────────────────────────────────────────────────────────

    public void NotifyCollectibleCollected(Collectible collectible)
    {
        collectedCount++;
        OnCollectibleCollected?.Invoke(collectible);
    }

    public void NotifyExitReached(LevelExit exit)
    {
        OnExitReached?.Invoke(exit);
    }

    public void NotifyHazardTriggered(Hazard hazard)
    {
        OnHazardTriggered?.Invoke(hazard);
    }

    // ─────────────────────────────────────────────────────────────
    //  Level-Wide Reset
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Resets every gameplay element in this layout to its initial state.
    /// Called by LevelFlowController.ResetLevel().
    /// </summary>
    public void ResetAll()
    {
        EnsureInitialized();
        collectedCount = 0;

        foreach (var collectible in collectibles)
        {
            if (collectible != null)
                collectible.ResetCollectible();
        }

        foreach (var exit in exits)
        {
            if (exit != null)
                exit.ResetExit();
        }

        foreach (var tile in mazeTiles)
        {
            if (tile != null)
                tile.ResetTile();
        }

        foreach (var block in crackedBlocks)
        {
            if (block != null)
                block.ResetBlock();
        }
    }
}

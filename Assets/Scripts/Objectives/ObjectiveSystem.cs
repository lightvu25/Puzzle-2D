using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the runtime lifecycle of the current level's objectives.
///
/// The ObjectiveSystem is responsible for:
///   - Creating the appropriate ObjectiveBase subclass from an ObjectiveDefinition.
///   - Initialising it against the loaded LevelRoot.
///   - Forwarding the completion event to LevelFlowController.
///   - Resetting objective state when the level resets.
///
/// This component does NOT handle level state changes — that belongs to LevelFlowController.
/// </summary>
public class ObjectiveSystem : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  Events
    // ─────────────────────────────────────────────────────────────

    /// <summary>Fired when the primary objective is satisfied.</summary>
    public event Action OnPrimaryObjectiveCompleted;

    /// <summary>Fired when an optional objective is satisfied. Argument is (objective, index).</summary>
    public event Action<ObjectiveBase, int> OnOptionalObjectiveCompleted;

    // ─────────────────────────────────────────────────────────────
    //  State
    // ─────────────────────────────────────────────────────────────

    private ObjectiveBase primaryObjective;
    private readonly List<ObjectiveBase> optionalObjectives = new List<ObjectiveBase>();
    private LevelRoot currentLayout;

    // ─────────────────────────────────────────────────────────────
    //  Public Properties
    // ─────────────────────────────────────────────────────────────

    public bool IsPrimaryObjectiveComplete => primaryObjective?.IsComplete ?? false;
    public float PrimaryObjectiveProgress  => primaryObjective?.Progress ?? 0f;

    /// <summary>Exposes the primary objective for debug/UI read access.</summary>
    public ObjectiveBase PrimaryObjective  => primaryObjective;

    /// <summary>Exposes the optional objectives for debug/UI read access.</summary>
    public IReadOnlyList<ObjectiveBase> OptionalObjectives => optionalObjectives;

    public int CompletedOptionalObjectiveCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < optionalObjectives.Count; i++)
            {
                if (optionalObjectives[i].IsComplete) count++;
            }
            return count;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  API
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates and initialises the primary and optional objectives from the given LevelData and layout.
    /// Must be called after the layout prefab is instantiated.
    /// </summary>
    public void Initialize(LevelData levelData, LevelRoot layout)
    {
        if (levelData == null)
        {
            Debug.LogError("[ObjectiveSystem] LevelData is null. Cannot initialise objective.", this);
            return;
        }

        currentLayout = layout;
        if (currentLayout != null)
            currentLayout.OnCollectibleCollected += HandleCollectibleCollected;

        // 1. Primary Objective
        ObjectiveDefinition def = levelData.PrimaryObjective;
        if (def == null)
        {
            Debug.LogError($"[ObjectiveSystem] LevelData '{levelData.LevelId}' has no primary objective.", this);
            return;
        }

        primaryObjective = CreateObjective(def.objectiveType);
        if (primaryObjective != null)
        {
            primaryObjective.OnCompleted += HandlePrimaryObjectiveCompleted;
            primaryObjective.Initialize(def, layout);
        }

        // 2. Optional Objectives
        optionalObjectives.Clear();
        if (levelData.OptionalObjectives != null)
        {
            for (int i = 0; i < levelData.OptionalObjectives.Length; i++)
            {
                var optDef = levelData.OptionalObjectives[i];
                if (optDef == null) continue;

                var optObj = CreateObjective(optDef.objectiveType);
                if (optObj != null)
                {
                    int index = i;
                    optObj.OnCompleted += () => HandleOptionalObjectiveCompleted(optObj, index);
                    optObj.Initialize(optDef, layout);
                    optionalObjectives.Add(optObj);
                }
            }
        }
    }

    /// <summary>Resets the objective system to its initial state for a level restart.</summary>
    public void ResetObjective()
    {
        primaryObjective?.Reset();

        if (primaryObjective != null)
            primaryObjective.OnCompleted += HandlePrimaryObjectiveCompleted;

        for (int i = 0; i < optionalObjectives.Count; i++)
        {
            var opt = optionalObjectives[i];
            opt?.Reset();
            if (opt != null)
            {
                int index = i;
                opt.OnCompleted += () => HandleOptionalObjectiveCompleted(opt, index);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Factory
    // ─────────────────────────────────────────────────────────────

    private ObjectiveBase CreateObjective(ObjectiveType type)
    {
        switch (type)
        {
            case ObjectiveType.ReachExit:
                return new ReachExitObjective();
            case ObjectiveType.CollectPickups:
                return new CollectPickupsObjective();
            default:
                Debug.LogWarning($"[ObjectiveSystem] Objective type {type} is not implemented.", this);
                return null;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Event Handlers
    // ─────────────────────────────────────────────────────────────

    private void HandleCollectibleCollected(Collectible collectible)
    {
        if (primaryObjective is CollectPickupsObjective primary)
            primary.NotifyPickupCollected();

        for (int i = 0; i < optionalObjectives.Count; i++)
        {
            if (optionalObjectives[i] is CollectPickupsObjective opt)
                opt.NotifyPickupCollected();
        }
    }

    private void HandlePrimaryObjectiveCompleted()
    {
        Debug.Log("[ObjectiveSystem] Primary objective completed!");
        OnPrimaryObjectiveCompleted?.Invoke();
    }

    private void HandleOptionalObjectiveCompleted(ObjectiveBase obj, int index)
    {
        Debug.Log($"[ObjectiveSystem] Optional objective #{index} completed!");
        OnOptionalObjectiveCompleted?.Invoke(obj, index);
    }

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────

    private void OnDestroy()
    {
        if (primaryObjective != null)
            primaryObjective.OnCompleted -= HandlePrimaryObjectiveCompleted;

        if (currentLayout != null)
            currentLayout.OnCollectibleCollected -= HandleCollectibleCollected;
    }
}

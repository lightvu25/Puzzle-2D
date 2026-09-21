using System;
using UnityEngine;
using HouseFlow.Level;

namespace HouseFlow.Objective
{
    /// <summary>
    /// Manages the runtime lifecycle of the current level's primary objective.
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

        // ─────────────────────────────────────────────────────────────
        //  State
        // ─────────────────────────────────────────────────────────────

        private ObjectiveBase primaryObjective;

        // ─────────────────────────────────────────────────────────────
        //  Public Properties
        // ─────────────────────────────────────────────────────────────

        public bool IsPrimaryObjectiveComplete => primaryObjective?.IsComplete ?? false;
        public float PrimaryObjectiveProgress  => primaryObjective?.Progress ?? 0f;

        /// <summary>Exposes the primary objective for debug/UI read access.</summary>
        public ObjectiveBase PrimaryObjective  => primaryObjective;

        // ─────────────────────────────────────────────────────────────
        //  API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates and initialises the primary objective from the given LevelData and layout.
        /// Must be called after the layout prefab is instantiated.
        /// </summary>
        public void Initialize(LevelData levelData, LevelRoot layout)
        {
            if (levelData == null)
            {
                Debug.LogError("[ObjectiveSystem] LevelData is null. Cannot initialise objective.", this);
                return;
            }

            ObjectiveDefinition def = levelData.PrimaryObjective;
            if (def == null)
            {
                Debug.LogError($"[ObjectiveSystem] LevelData '{levelData.LevelId}' has no primary objective.", this);
                return;
            }

            primaryObjective = CreateObjective(def.objectiveType);
            if (primaryObjective == null) return;

            primaryObjective.OnCompleted += HandlePrimaryObjectiveCompleted;
            primaryObjective.Initialize(def, layout);
        }

        /// <summary>Resets the objective system to its initial state for a level restart.</summary>
        public void ResetObjective()
        {
            primaryObjective?.Reset();

            if (primaryObjective != null)
                primaryObjective.OnCompleted += HandlePrimaryObjectiveCompleted;
        }

        // ─────────────────────────────────────────────────────────────
        //  Factory
        // ─────────────────────────────────────────────────────────────

        private ObjectiveBase CreateObjective(ObjectiveType type)
        {
            switch (type)
            {
                case ObjectiveType.DeliverFluidToContainer:
                    return new DeliverFluidObjective();

                default:
                    Debug.LogError($"[ObjectiveSystem] Unsupported objective type: {type}. " +
                                   "Implement a new ObjectiveBase subclass for this type.", this);
                    return null;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Event Handlers
        // ─────────────────────────────────────────────────────────────

        private void HandlePrimaryObjectiveCompleted()
        {
            Debug.Log("[ObjectiveSystem] Primary objective completed!");
            OnPrimaryObjectiveCompleted?.Invoke();
        }

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void OnDestroy()
        {
            if (primaryObjective != null)
                primaryObjective.OnCompleted -= HandlePrimaryObjectiveCompleted;
        }
    }
}

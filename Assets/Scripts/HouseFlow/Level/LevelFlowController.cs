using System;
using UnityEngine;
using HouseFlow.Objective;

namespace HouseFlow.Level
{
    /// <summary>
    /// Top-level coordinator for a HOUSEFLOW level session.
    ///
    /// Responsibilities:
    ///   - Drive LevelLoader to load/unload the level layout.
    ///   - Drive ObjectiveSystem to initialise and track objectives.
    ///   - Manage LevelState transitions (Idle → Loading → Playing → Completed).
    ///   - Handle level reset (in-place — no scene reload).
    ///   - Control HouseflowInputHandler to block interaction on completion.
    ///
    /// This is the single entry point for level lifecycle management.
    /// Nothing else should directly drive LevelLoader or ObjectiveSystem.
    /// </summary>
    public class LevelFlowController : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        //  Inspector References
        // ─────────────────────────────────────────────────────────────

        [Header("System References")]
        [Tooltip("The LevelLoader responsible for instantiating Layout Prefabs.")]
        [SerializeField] private LevelLoader levelLoader;

        [Tooltip("The objective system for tracking level completion.")]
        [SerializeField] private ObjectiveSystem objectiveSystem;

        [Header("Level To Load")]
        [Tooltip("The LevelData asset to load when the scene starts.")]
        [SerializeField] private LevelData levelData;

        // ─────────────────────────────────────────────────────────────
        //  Events
        // ─────────────────────────────────────────────────────────────

        /// <summary>Fires every time the level state changes.</summary>
        public event Action<LevelState> OnStateChanged;

        /// <summary>Fires when the level is fully loaded and Playing begins.</summary>
        public event Action OnLevelStarted;

        /// <summary>Fires when the primary objective is completed.</summary>
        public event Action OnLevelCompleted;

        // ─────────────────────────────────────────────────────────────
        //  State
        // ─────────────────────────────────────────────────────────────

        private LevelState currentState = LevelState.Idle;
        private LevelRoot  currentRoot;

        public LevelState  CurrentState   => currentState;
        public LevelData   CurrentLevelData => levelData;
        public LevelRoot   CurrentRoot    => currentRoot;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            // If the Main Menu passed a specific level via the global GameSession, use it.
            if (GameSession.Instance != null && GameSession.Instance.pendingPuzzleLevel != null)
            {
                levelData = GameSession.Instance.pendingPuzzleLevel;
                GameSession.Instance.pendingPuzzleLevel = null; // Clear it after reading
            }

            if (levelData != null)
                StartLevel(levelData);
            else
                Debug.LogWarning("[LevelFlowController] No LevelData assigned. Call StartLevel() manually.", this);
        }

        // ─────────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads and starts the given level.
        /// Can be called at runtime to load a different LevelData (future Level Select).
        /// </summary>
        public void StartLevel(LevelData data)
        {
            if (data == null)
            {
                Debug.LogError("[LevelFlowController] Cannot start level — LevelData is null.", this);
                return;
            }

            levelData = data;
            SetState(LevelState.Loading);

            // Load the layout
            currentRoot = levelLoader.LoadLevel(levelData);
            if (currentRoot == null)
            {
                Debug.LogError("[LevelFlowController] Level layout failed to load.", this);
                SetState(LevelState.Idle);
                return;
            }

            // Apply per-level physics config to the layout
            currentRoot.InitializePhysics(levelData.PhysicsConfig);

            // Initialise objective system
            objectiveSystem.Initialize(levelData, currentRoot);
            objectiveSystem.OnPrimaryObjectiveCompleted -= HandleObjectiveCompleted;
            objectiveSystem.OnPrimaryObjectiveCompleted += HandleObjectiveCompleted;

            SetState(LevelState.Playing);
            OnLevelStarted?.Invoke();

            Debug.Log($"[LevelFlowController] Level '{levelData.LevelId}' started.");
        }

        /// <summary>
        /// Resets the current level to its initial state without reloading the scene.
        /// Restores all puzzle objects, particles, and objective progress.
        /// </summary>
        public void ResetLevel()
        {
            if (currentState == LevelState.Idle || currentState == LevelState.Loading)
            {
                Debug.LogWarning("[LevelFlowController] ResetLevel called but no level is active.", this);
                return;
            }

            // Unsubscribe to prevent double-firing
            objectiveSystem.OnPrimaryObjectiveCompleted -= HandleObjectiveCompleted;

            // Reset all layout state (particles, sources, valves, targets)
            currentRoot?.ResetAll();

            // Re-apply physics config in case it was modified
            currentRoot?.InitializePhysics(levelData.PhysicsConfig);

            // Reset objective
            objectiveSystem.ResetObjective();
            objectiveSystem.OnPrimaryObjectiveCompleted += HandleObjectiveCompleted;

            SetState(LevelState.Playing);
            Debug.Log($"[LevelFlowController] Level '{levelData.LevelId}' reset.");
        }

        // ─────────────────────────────────────────────────────────────
        //  Update Loop (Input)
        // ─────────────────────────────────────────────────────────────
        
        private void Update()
        {
            if (currentState != LevelState.Playing) return;

            // Check if GameInput registered a tap
            if (GameInput.Instance != null && GameInput.Instance.IsPuzzleTapPressed(out Vector2 screenPos))
            {
                // Prevent clicking through UI
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    return;

                if (Camera.main != null)
                {
                    Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                    RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 100f);
                    
                    if (hit.collider != null)
                    {
                        var interactable = hit.collider.GetComponentInParent<IInteractable>();
                        
                        // Small check to respect Valve's isAnimating state if we can cast it
                        if (interactable is HouseFlow.Mechanical.Valve valve && !valve.IsInteractable)
                            return;

                        interactable?.Interact();
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Private
        // ─────────────────────────────────────────────────────────────

        private void HandleObjectiveCompleted()
        {
            if (currentState != LevelState.Playing) return;

            SetState(LevelState.Completed);

            OnLevelCompleted?.Invoke();
            Debug.Log($"[LevelFlowController] Level '{levelData.LevelId}' COMPLETED!");
        }

        private void SetState(LevelState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }

        private void ValidateReferences()
        {
            if (levelLoader == null)
                Debug.LogError("[LevelFlowController] LevelLoader reference is not assigned.", this);

            if (objectiveSystem == null)
                Debug.LogError("[LevelFlowController] ObjectiveSystem reference is not assigned.", this);
        }

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle Cleanup
        // ─────────────────────────────────────────────────────────────

        private void OnDestroy()
        {
            if (objectiveSystem != null)
                objectiveSystem.OnPrimaryObjectiveCompleted -= HandleObjectiveCompleted;
        }
    }
}

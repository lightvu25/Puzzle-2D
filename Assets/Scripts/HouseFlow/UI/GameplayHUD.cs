using System;
using UnityEngine;
using UnityEngine.UI;
using HouseFlow.Level;
using HouseFlow.Objective;
using HouseFlow.Progression;

namespace HouseFlow.UI
{
    /// <summary>
    /// In-game functional HUD for HOUSEFLOW.
    /// Handles level information, objective progress, restart, pause, win/fail overlays.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private LevelFlowController flowController;
        [SerializeField] private ObjectiveSystem objectiveSystem;

        [Header("HUD Header")]
        [SerializeField] private Text levelTitleText;
        [SerializeField] private Text objectiveProgressText;
        [SerializeField] private Slider objectiveProgressBar;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mapButton;

        [Header("Level Completed Overlay")]
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private Text completionTitleText;
        [SerializeField] private Text starsEarnedText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button completionRestartButton;
        [SerializeField] private Button completionMapButton;

        [Header("Level Failed Overlay")]
        [SerializeField] private GameObject failurePanel;
        [SerializeField] private Text failureReasonText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button failureMapButton;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (flowController == null)
                flowController = FindFirstObjectByType<LevelFlowController>();

            if (objectiveSystem == null)
                objectiveSystem = FindFirstObjectByType<ObjectiveSystem>();

            WireButtons();
        }

        private void Start()
        {
            if (flowController != null)
            {
                flowController.OnLevelStarted += HandleLevelStarted;
                flowController.OnLevelCompleted += HandleLevelCompleted;
                flowController.OnLevelFailed += HandleLevelFailed;
                flowController.OnStateChanged += HandleStateChanged;
            }

            HideOverlays();
            RefreshHUD();
        }

        private void OnDestroy()
        {
            if (flowController != null)
            {
                flowController.OnLevelStarted -= HandleLevelStarted;
                flowController.OnLevelCompleted -= HandleLevelCompleted;
                flowController.OnLevelFailed -= HandleLevelFailed;
                flowController.OnStateChanged -= HandleStateChanged;
            }
        }

        private void Update()
        {
            UpdateObjectiveProgress();
        }

        // ─────────────────────────────────────────────────────────────
        //  UI Handlers
        // ─────────────────────────────────────────────────────────────

        private void WireButtons()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (mapButton != null)
                mapButton.onClick.AddListener(OnMapClicked);

            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);

            if (completionRestartButton != null)
                completionRestartButton.onClick.AddListener(OnRestartClicked);

            if (completionMapButton != null)
                completionMapButton.onClick.AddListener(OnMapClicked);

            if (retryButton != null)
                retryButton.onClick.AddListener(OnRestartClicked);

            if (failureMapButton != null)
                failureMapButton.onClick.AddListener(OnMapClicked);
        }

        private void HandleLevelStarted()
        {
            HideOverlays();
            RefreshHUD();
        }

        private void HandleLevelCompleted()
        {
            if (completionPanel != null)
            {
                completionPanel.SetActive(true);

                if (completionTitleText != null && flowController != null && flowController.CurrentLevelData != null)
                {
                    completionTitleText.text = $"{flowController.CurrentLevelData.DisplayName} Complete!";
                }

                if (starsEarnedText != null && objectiveSystem != null)
                {
                    int stars = 1 + objectiveSystem.CompletedOptionalObjectiveCount;
                    starsEarnedText.text = $"{stars} ★ Earned";
                }
            }
        }

        private void HandleLevelFailed(string reason)
        {
            if (failurePanel != null)
            {
                failurePanel.SetActive(true);

                if (failureReasonText != null)
                {
                    failureReasonText.text = reason;
                }
            }
        }

        private void HandleStateChanged(LevelState state)
        {
            if (state == LevelState.Playing)
            {
                HideOverlays();
            }
        }

        public void RefreshHUD()
        {
            if (flowController != null && flowController.CurrentLevelData != null)
            {
                var data = flowController.CurrentLevelData;
                if (levelTitleText != null)
                {
                    levelTitleText.text = $"World {data.World} - {data.DisplayName}";
                }
            }

            UpdateObjectiveProgress();
        }

        private void UpdateObjectiveProgress()
        {
            if (objectiveSystem == null) return;

            float progress = objectiveSystem.PrimaryObjectiveProgress;

            if (objectiveProgressBar != null)
            {
                objectiveProgressBar.value = progress;
            }

            if (objectiveProgressText != null)
            {
                objectiveProgressText.text = $"{Mathf.RoundToInt(progress * 100f)}%";
            }
        }

        private void HideOverlays()
        {
            if (completionPanel != null) completionPanel.SetActive(false);
            if (failurePanel != null) failurePanel.SetActive(false);
        }

        // ─────────────────────────────────────────────────────────────
        //  Button Callbacks
        // ─────────────────────────────────────────────────────────────

        private void OnRestartClicked()
        {
            HideOverlays();
            flowController?.RestartLevel();
        }

        private void OnMapClicked()
        {
            HideOverlays();
            flowController?.ReturnToMap();
        }

        private void OnNextLevelClicked()
        {
            HideOverlays();
            flowController?.LoadNextLevel();
        }
    }
}

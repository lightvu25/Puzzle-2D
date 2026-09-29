using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game functional HUD.
/// Handles level information, objective progress, restart, pause, win/fail overlays.
/// </summary>
public class GameplayHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private LevelFlowController flowController;
    [SerializeField] private ObjectiveSystem objectiveSystem;

    [Header("HUD Header")]
    [SerializeField] private Text levelTitleText;
    [SerializeField] private Text primaryObjectiveDescText;
    [SerializeField] private Text optionalObjectiveDescText;
    [SerializeField] private Text objectiveProgressText;
    [SerializeField] private Slider objectiveProgressBar;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mapButton;

    [Header("Currencies")]
    [SerializeField] private Text coinsText;
    [SerializeField] private Text gemsText;
    [SerializeField] private Text acclaimText;

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
            flowController = FindAnyObjectByType<LevelFlowController>();

        if (objectiveSystem == null)
            objectiveSystem = FindAnyObjectByType<ObjectiveSystem>();

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

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoinsChanged += UpdateCoins;
            EconomyManager.Instance.OnGemsChanged += UpdateGems;
            EconomyManager.Instance.OnShowcaseAcclaimChanged += UpdateAcclaim;
        }

        HideOverlays();
        RefreshHUD();

        // The panel must start active in the scene so Awake/Start execute;
        // reconcile visibility with the current flow state immediately.
        if (flowController != null)
            HandleStateChanged(flowController.CurrentState);
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

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoinsChanged -= UpdateCoins;
            EconomyManager.Instance.OnGemsChanged -= UpdateGems;
            EconomyManager.Instance.OnShowcaseAcclaimChanged -= UpdateAcclaim;
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
        gameObject.SetActive(true);
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
            gameObject.SetActive(true);
            HideOverlays();
        }
        else if (state == LevelState.Idle)
        {
            gameObject.SetActive(false);
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

        if (EconomyManager.Instance != null)
        {
            UpdateCoins(EconomyManager.Instance.Coins, 0);
            UpdateGems(EconomyManager.Instance.Gems, 0);
            UpdateAcclaim(EconomyManager.Instance.ShowcaseAcclaim, 0);
        }

        if (flowController != null && flowController.CurrentLevelData != null)
        {
            if (primaryObjectiveDescText != null)
            {
                primaryObjectiveDescText.text = GetObjectiveText(flowController.CurrentLevelData.PrimaryObjective);
            }

            if (optionalObjectiveDescText != null && flowController.CurrentLevelData.OptionalObjectives != null && objectiveSystem != null)
            {
                string optionalText = "";
                for (int i = 0; i < flowController.CurrentLevelData.OptionalObjectives.Length; i++)
                {
                    var optDef = flowController.CurrentLevelData.OptionalObjectives[i];
                    bool isCompleted = objectiveSystem.OptionalObjectives != null && i < objectiveSystem.OptionalObjectives.Count && objectiveSystem.OptionalObjectives[i].IsComplete;
                    optionalText += (isCompleted ? "[X] " : "[ ] ") + GetObjectiveText(optDef) + "\n";
                }
                optionalObjectiveDescText.text = optionalText;
            }
        }

        UpdateObjectiveProgress();
    }

    /// <summary>Human-readable label for an objective, falling back to a generated description.</summary>
    private static string GetObjectiveText(ObjectiveDefinition def)
    {
        if (def == null) return "";
        if (!string.IsNullOrEmpty(def.description)) return def.description;

        return def.objectiveType switch
        {
            ObjectiveType.ReachExit      => "Reach the exit!",
            ObjectiveType.CollectPickups => def.requiredCount > 0
                ? $"Collect {def.requiredCount} dots"
                : "Collect all dots",
            _ => def.objectiveType.ToString()
        };
    }

    private void UpdateCoins(int amount, int delta) { if (coinsText != null) coinsText.text = amount.ToString(); }
    private void UpdateGems(int amount, int delta) { if (gemsText != null) gemsText.text = amount.ToString(); }
    private void UpdateAcclaim(int amount, int delta) { if (acclaimText != null) acclaimText.text = amount.ToString(); }

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


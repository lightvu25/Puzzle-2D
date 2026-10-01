using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game HUD shown only while a level is running (Tomb of the Mask style):
/// currency counters up top, pause/menu button, optional power-up counter.
/// Result screens are handled by <see cref="ResultPanelUI"/> — this component
/// owns the in-run HUD only.
/// </summary>
public class InGameHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private LevelFlowController flowController;

    [Header("HUD Root")]
    [Tooltip("The visible HUD container toggled by flow state. If empty, this component's own GameObject is toggled — only safe when no other always-needed UI components share it.")]
    [SerializeField] private GameObject hudRoot;

    [Header("Header")]
    [Tooltip("Optional level name label (TMP_Text, Munro font).")]
    [SerializeField] private TMP_Text levelTitleText;
    [Tooltip("Pause/menu button — opens the pause menu overlay.")]
    [SerializeField] private Button menuButton;
    [Tooltip("Pause menu controller — falls back to scene lookup if unassigned.")]
    [SerializeField] private PauseMenuUI pauseMenu;

    [Header("Currencies")]
    [Tooltip("Coin counter label. Icon Image lives next to it — assign art in the scene.")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text gemsText;
    [SerializeField] private TMP_Text starsText;

    [Header("Power-up Counter")]
    [Tooltip("Shows how many Kinetic Shields are banked. Optional.")]
    [SerializeField] private GameObject shieldCounterRoot;
    [SerializeField] private TMP_Text shieldCountText;

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (flowController == null)
            flowController = FindAnyObjectByType<LevelFlowController>();
        if (pauseMenu == null)
            pauseMenu = FindAnyObjectByType<PauseMenuUI>(FindObjectsInactive.Include);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);
    }

    private void Start()
    {
        if (flowController != null)
        {
            flowController.OnLevelStarted += HandleLevelStarted;
            flowController.OnStateChanged += HandleStateChanged;
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoinsChanged += UpdateCoins;
            EconomyManager.Instance.OnGemsChanged += UpdateGems;
        }

        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.OnProgressionChanged += UpdateProgressionDisplay;

        RefreshHUD();

        // Reconcile visibility with the current flow state immediately —
        // the HUD object must start active in the scene so Awake/Start run.
        if (flowController != null)
            HandleStateChanged(flowController.CurrentState);
    }

    private void OnDestroy()
    {
        if (flowController != null)
        {
            flowController.OnLevelStarted -= HandleLevelStarted;
            flowController.OnStateChanged -= HandleStateChanged;
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoinsChanged -= UpdateCoins;
            EconomyManager.Instance.OnGemsChanged -= UpdateGems;
        }

        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.OnProgressionChanged -= UpdateProgressionDisplay;
    }

    // ─────────────────────────────────────────────────────────────
    //  Flow state
    // ─────────────────────────────────────────────────────────────

    private void HandleLevelStarted()
    {
        SetHudVisible(true);
        RefreshHUD();
    }

    private void HandleStateChanged(LevelState state)
    {
        // HUD only exists while playing — completed/failed show ResultPanelUI,
        // idle shows the main menu tab bar.
        if (state == LevelState.Playing || state == LevelState.Idle)
            SetHudVisible(state == LevelState.Playing);
        else
            SetHudVisible(false);
    }

    private void SetHudVisible(bool visible)
    {
        GameObject target = hudRoot != null ? hudRoot : gameObject;
        if (target.activeSelf != visible)
            target.SetActive(visible);
    }

    // ─────────────────────────────────────────────────────────────
    //  Display
    // ─────────────────────────────────────────────────────────────

    public void RefreshHUD()
    {
        if (flowController != null && flowController.CurrentLevelData != null && levelTitleText != null)
            levelTitleText.text = flowController.CurrentLevelData.DisplayName;

        if (EconomyManager.Instance != null)
        {
            UpdateCoins(EconomyManager.Instance.Coins, 0);
            UpdateGems(EconomyManager.Instance.Gems, 0);
        }

        UpdateProgressionDisplay();
    }

    private void UpdateCoins(int amount, int delta) { if (coinsText != null) coinsText.text = amount.ToString(); }
    private void UpdateGems(int amount, int delta) { if (gemsText != null) gemsText.text = amount.ToString(); }

    private void UpdateProgressionDisplay()
    {
        if (starsText != null && ProgressionManager.Instance != null)
            starsText.text = $"{ProgressionManager.Instance.TotalStars}";

        int shields = ProgressionManager.Instance != null
            ? ProgressionManager.Instance.GetPowerUpCount(PowerUpIds.KineticShield)
            : 0;

        if (shieldCounterRoot != null)
            shieldCounterRoot.SetActive(shields > 0);
        if (shieldCountText != null)
            shieldCountText.text = shields.ToString();
    }

    // ─────────────────────────────────────────────────────────────
    //  Buttons
    // ─────────────────────────────────────────────────────────────

    private void OnMenuClicked()
    {
        if (pauseMenu != null)
            pauseMenu.OpenPause();
        else if (UIManager.Instance != null)
            UIManager.Instance.OpenPanel(UIPanelType.Pause);
    }
}

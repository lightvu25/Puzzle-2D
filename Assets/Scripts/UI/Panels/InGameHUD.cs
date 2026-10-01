using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private LevelFlowController flowController;

    [Header("HUD Root")]
    [SerializeField] private GameObject hudRoot;

    [Header("Header")]
    [SerializeField] private Button menuButton;
    [SerializeField] private PauseMenuUI pauseMenu;

    [Header("Currencies")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text gemsText;
    [SerializeField] private TMP_Text starsText;

    [Header("Run Info")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text tierText;

    [Header("Power-up Counter")]
    [SerializeField] private GameObject shieldCounterRoot;
    [SerializeField] private TMP_Text shieldCountText;

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
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.OnTileCrossed += UpdateMoves;
            PlayerMovement.Instance.OnTierChanged += UpdateTier;
        }

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

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.OnTileCrossed -= UpdateMoves;
            PlayerMovement.Instance.OnTierChanged -= UpdateTier;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Flow state
    // ─────────────────────────────────────────────────────────────

    private void HandleLevelStarted()
    {
        SetHudVisible(true);
        RefreshHUD();

        if (levelText != null && flowController != null && flowController.CurrentLevelData != null)
            levelText.text = flowController.CurrentLevelData.DisplayName;
        UpdateMoves(0);
        UpdateTier(VelocityTier.None);
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
        if (EconomyManager.Instance != null)
        {
            UpdateCoins(EconomyManager.Instance.Coins, 0);
            UpdateGems(EconomyManager.Instance.Gems, 0);
        }

        UpdateProgressionDisplay();
    }

    private void UpdateCoins(int amount, int delta) { if (coinsText != null) coinsText.text = amount.ToString(); }
    private void UpdateGems(int amount, int delta) { if (gemsText != null) gemsText.text = amount.ToString(); }

    private void UpdateMoves(int tiles)
    {
        if (movesText != null) movesText.text = tiles.ToString();
    }

    private void UpdateTier(VelocityTier tier)
    {
        if (tierText != null) tierText.text = tier == VelocityTier.None ? "" : tier.ToString().ToUpperInvariant();
    }

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
        // Pause is a UIManager overlay — it opens on top and freezes time
        // without hiding this HUD.
        if (UIManager.Instance != null && UIManager.Instance.GetPanel<IUIPanel>(UIPanelType.Pause) != null)
            UIManager.Instance.OpenPanel(UIPanelType.Pause);
        else if (pauseMenu != null)
            pauseMenu.Show();
    }
}

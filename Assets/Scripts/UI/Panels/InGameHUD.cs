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

    [Header("Currencies")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private Image[] starImages;

    [Header("Run Info")]
    [SerializeField] private TMP_Text movesText;

    [Header("Power-up Counter")]
    [SerializeField] private Button shieldButton;
    [SerializeField] private TMP_Text shieldCountText;

    private void Awake()
    {
        if (flowController == null)
            flowController = FindAnyObjectByType<LevelFlowController>();

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);

        if (shieldButton != null)
            shieldButton.onClick.AddListener(OnShieldClicked);
    }

    private void Start()
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.OnTileCrossed += UpdateMoves;
        }

        if (flowController != null)
        {
            flowController.OnLevelStarted += HandleLevelStarted;
            flowController.OnStateChanged += HandleStateChanged;
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoinsChanged += UpdateCoins;
        }

        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.OnProgressionChanged += RefreshPowerUp;
        }

        RefreshHUD();

        if (flowController != null)
        {
            HandleStateChanged(flowController.CurrentState);
        }
    }

    private void OnDestroy()
    {
        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(OnMenuClicked);
        }

        if (shieldButton != null)
        {
            shieldButton.onClick.RemoveListener(OnShieldClicked);
        }

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.OnTileCrossed -= UpdateMoves;
        }

        if (flowController != null)
        {
            flowController.OnLevelStarted -= HandleLevelStarted;
            flowController.OnStateChanged -= HandleStateChanged;
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoinsChanged -= UpdateCoins;
        }

        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.OnProgressionChanged -= RefreshPowerUp;
        }
    }

    private void HandleLevelStarted()
    {
        SetHudVisible(true);
        UpdateMoves(0);
        SetStars(0);
        RefreshHUD();
    }

    private void HandleStateChanged(LevelState state)
    {
        bool show = state == LevelState.Playing;
        SetHudVisible(show);
    }

    private void SetHudVisible(bool visible)
    {
        GameObject target = hudRoot != null ? hudRoot : gameObject;

        if (target.activeSelf != visible)
        {
            target.SetActive(visible);
        }
    }

    public void RefreshHUD()
    {
        if (EconomyManager.Instance != null)
        {
            UpdateCoins(EconomyManager.Instance.Coins, 0);
        }

        UpdateMoves(0);
        RefreshPowerUp();
        SetStars(0);
    }

    private void UpdateCoins(int amount, int delta) { if (coinsText != null) coinsText.text = amount.ToString(); }
    
    public void SetStars(int earnedStars)
    {
        earnedStars = Mathf.Clamp(earnedStars, 0, 3);

        if (starImages == null)
        {
            return;
        }

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null)
            {
                continue;
            }

            starImages[i].gameObject.SetActive(i < earnedStars);
        }
    }

    private void UpdateMoves(int tiles)
    {
        if (movesText != null) movesText.text = tiles.ToString();
    }

    private void RefreshPowerUp()
    {
        int shieldCount = 0;

        if (ProgressionManager.Instance != null)
        {
            shieldCount =
                ProgressionManager.Instance.GetPowerUpCount(
                    PowerUpIds.KineticShield
                );
        }

        if (shieldCountText != null)
        {
            shieldCountText.text = shieldCount.ToString();
        }

        if (shieldButton != null)
        {
            shieldButton.gameObject.SetActive(shieldCount > 0);
            shieldButton.interactable = shieldCount > 0;
        }
    }

    private void OnShieldClicked()
    {
    }

    private void OnMenuClicked()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.OpenPanel(UIPanelType.Pause);
    }

}

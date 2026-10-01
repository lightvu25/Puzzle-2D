using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Post-game result screens — completion panel (star icons, coins earned,
/// next/retry/map) and failure panel (reason + retry). Lives on a separate
/// GameObject from <see cref="InGameHUD"/> so in-run HUD and result flow are
/// fully independent. The blind-box button is owned by <see cref="BlindBoxUI"/>
/// on the same completion panel.
/// </summary>
public class ResultPanelUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private LevelFlowController flowController;

    [Header("Completion Panel")]
    [SerializeField] private GameObject completionPanel;
    [Tooltip("Result header art slot — assign your 'Level Complete' image.")]
    [SerializeField] private Image completionTitleImage;
    [SerializeField] private TMP_Text completionTitleText;
    [SerializeField] private TMP_Text starsEarnedText;
    [Tooltip("Exactly 3 star icons — filled color when earned, dimmed when not.")]
    [SerializeField] private Image[] starSlots;
    [SerializeField] private TMP_Text coinsEarnedText;
    [SerializeField] private Image coinIconImage;

    [Header("Completion Buttons")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button completionRestartButton;
    [SerializeField] private Button completionMapButton;

    [Header("Failure Panel")]
    [SerializeField] private GameObject failurePanel;
    [SerializeField] private Image failureTitleImage;
    [SerializeField] private TMP_Text failureReasonText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button failureMapButton;

    [Header("Star Slot Colors")]
    [SerializeField] private Color starFilledColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private Color starEmptyColor = new Color(1f, 1f, 1f, 0.25f);

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (flowController == null)
            flowController = FindAnyObjectByType<LevelFlowController>();
    }

    private void Start()
    {
        if (flowController != null)
        {
            flowController.OnLevelCompleted += ShowCompletion;
            flowController.OnLevelFailed += ShowFailure;
            flowController.OnLevelRestarted += HideAll;
            flowController.OnReturnToMapRequested += HideAll;
        }

        BindButton(nextLevelButton, OnNextLevel);
        BindButton(completionRestartButton, OnRestart);
        BindButton(completionMapButton, OnReturnToMap);
        BindButton(retryButton, OnRestart);
        BindButton(failureMapButton, OnReturnToMap);

        HideAll();
    }

    private void OnDestroy()
    {
        if (flowController != null)
        {
            flowController.OnLevelCompleted -= ShowCompletion;
            flowController.OnLevelFailed -= ShowFailure;
            flowController.OnLevelRestarted -= HideAll;
            flowController.OnReturnToMapRequested -= HideAll;
        }
    }

    private void BindButton(Button button, Action callback)
    {
        if (button != null)
            button.onClick.AddListener(() => callback());
    }

    // ─────────────────────────────────────────────────────────────
    //  Result displays
    // ─────────────────────────────────────────────────────────────

    private void ShowCompletion()
    {
        int stars = Mathf.Clamp(flowController != null ? flowController.LastCompletedStars : 1, 1, 3);

        if (failurePanel != null) failurePanel.SetActive(false);
        if (completionPanel != null) completionPanel.SetActive(true);

        if (completionTitleText != null)
            completionTitleText.text = "LEVEL COMPLETE";

        if (starsEarnedText != null)
            starsEarnedText.text = $"{stars} ★";

        if (starSlots != null)
        {
            for (int i = 0; i < starSlots.Length; i++)
            {
                if (starSlots[i] != null)
                    starSlots[i].color = i < stars ? starFilledColor : starEmptyColor;
            }
        }

        RewardBundle clearReward = flowController != null ? flowController.LastClearReward : null;
        if (coinsEarnedText != null)
            coinsEarnedText.text = clearReward != null ? $"+{clearReward.CoinCount}" : "";

        if (nextLevelButton != null)
        {
            bool hasNext = flowController != null
                && flowController.CurrentLevelData != null
                && ProgressionManager.Instance != null
                && ProgressionManager.Instance.Database != null
                && ProgressionManager.Instance.Database.GetNextLevel(flowController.CurrentLevelData) != null;
            nextLevelButton.gameObject.SetActive(hasNext);
        }
    }

    private void ShowFailure(string reason)
    {
        if (completionPanel != null) completionPanel.SetActive(false);
        if (failurePanel != null) failurePanel.SetActive(true);
        if (failureReasonText != null)
            failureReasonText.text = string.IsNullOrEmpty(reason) ? "LEVEL FAILED" : reason;
    }

    public void HideAll()
    {
        if (completionPanel != null) completionPanel.SetActive(false);
        if (failurePanel != null) failurePanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────
    //  Buttons
    // ─────────────────────────────────────────────────────────────

    private void OnNextLevel() => flowController?.LoadNextLevel();
    private void OnRestart() => flowController?.RestartLevel();
    private void OnReturnToMap() => flowController?.ReturnToMap();
}

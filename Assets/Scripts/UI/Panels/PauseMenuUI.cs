using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private LevelFlowController flowController;

    [Header("Panel")]
    [SerializeField] private GameObject pausePanel;

    [Header("Title")]
    [SerializeField] private TMP_Text levelTitleText;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitToMenuButton;
    [SerializeField] private Button settingsButton;

    public event Action OnSettingsRequested;

    public bool IsPaused => UIManager.Instance != null &&
                            UIManager.Instance.IsPanelOpen(UIPanelType.Pause);

    private void Awake()
    {
        if (flowController == null)
            flowController = FindAnyObjectByType<LevelFlowController>();

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (quitToMenuButton != null)
            quitToMenuButton.onClick.AddListener(OnQuitClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (flowController != null && flowController.CurrentLevelData != null && levelTitleText != null)
            levelTitleText.text = flowController.CurrentLevelData.DisplayName;

        Hide();
    }

    private void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClicked);

        if (quitToMenuButton != null)
            quitToMenuButton.onClick.RemoveListener(OnQuitClicked);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
    }

    public void Show()
    {
        pausePanel.SetActive(true);
    }

    public void Hide()
    {
        pausePanel.SetActive(false);
    }

    private void OnResumeClicked()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ClosePanelIfOpen(UIPanelType.Pause);
    }

    private void OnRestartClicked()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ClosePanelIfOpen(UIPanelType.Pause);

        flowController?.RestartLevel();
    }

    private void OnQuitClicked()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ClosePanelIfOpen(UIPanelType.Pause);

        flowController?.ReturnToMap();
    }

    private void OnSettingsClicked()
    {
        OnSettingsRequested?.Invoke();
    }

}

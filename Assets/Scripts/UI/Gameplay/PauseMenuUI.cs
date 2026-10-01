using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause/menu overlay opened from the in-game HUD's menu button.
/// Freezes gameplay with Time.timeScale while open; Resume restores it,
/// Restart replays the level, Quit returns to the map/main menu.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private LevelFlowController flowController;

    [Header("Panel")]
    [Tooltip("Root pause panel — keep inactive in the scene.")]
    [SerializeField] private GameObject pausePanel;
    [Tooltip("Header art slot — assign your pause icon image.")]
    [SerializeField] private Image pauseTitleImage;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitToMenuButton;
    [SerializeField] private Button settingsButton;

    public event Action OnSettingsRequested;

    private float previousTimeScale = 1f;
    private bool paused;

    public bool IsPaused => paused;

    private void Awake()
    {
        if (flowController == null)
            flowController = FindAnyObjectByType<LevelFlowController>();

        if (resumeButton != null) resumeButton.onClick.AddListener(ClosePause);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (quitToMenuButton != null) quitToMenuButton.onClick.AddListener(OnQuitToMenu);
        if (settingsButton != null) settingsButton.onClick.AddListener(() => OnSettingsRequested?.Invoke());

        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (paused) RestoreTime();
    }

    // ─────────────────────────────────────────────────────────────
    //  Pause control
    // ─────────────────────────────────────────────────────────────

    public void OpenPause()
    {
        if (paused) return;
        paused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ClosePause()
    {
        if (!paused) return;
        paused = false;
        RestoreTime();

        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void RestoreTime()
    {
        Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
    }

    // ─────────────────────────────────────────────────────────────
    //  Buttons
    // ─────────────────────────────────────────────────────────────

    private void OnRestart()
    {
        ClosePause();
        flowController?.RestartLevel();
    }

    private void OnQuitToMenu()
    {
        ClosePause();
        flowController?.ReturnToMap();
    }
}

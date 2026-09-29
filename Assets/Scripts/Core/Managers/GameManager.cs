using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private CinemachineCamera cinemachineCamera;

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameResume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Debug.Log("[GameManager] Awake called. Setting Instance.");
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (this != Instance) return;

        Debug.Log($"[GameManager] OnSceneLoaded called for scene: {scene.name}");

        // Ensure time is unpaused before initializing a new scene —
        // prevents the player staying frozen after a paused transition.
        Time.timeScale = 1f;

        cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();

        StartCoroutine(InitializeSceneRoutine());
    }

    private IEnumerator InitializeSceneRoutine()
    {
        yield return null;

        if (GameInput.Instance != null)
            GameInput.Instance.SetInputsEnabled(true);

        int depth = 1;

        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.IncrementLevelAttempt(depth);
        }
    }

    // ------------------------------------------------------------------ //
    //  Pause / Resume                                                      //
    // ------------------------------------------------------------------ //

    public void PauseResumeGame()
    {
        if (Time.timeScale == 1f) PauseGame();
        else                      ResumeGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(this, EventArgs.Empty);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        OnGameResume?.Invoke(this, EventArgs.Empty);
    }
}

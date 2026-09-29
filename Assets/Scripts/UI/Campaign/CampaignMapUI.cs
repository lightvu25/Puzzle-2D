using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controller for the Campaign World Map and Level Selection screens.
/// Fully data-driven from CampaignDatabase and ProgressionManager.
/// </summary>
public class CampaignMapUI : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Root panel for the World/Campaign selection.")]
    [SerializeField] private GameObject worldMapPanel;

    [Tooltip("Root panel for the Level Selection within a world.")]
    [SerializeField] private GameObject levelSelectPanel;

    [Header("World Selection UI")]
    [SerializeField] private Transform worldButtonContainer;
    [SerializeField] private Button worldButtonPrefab;
    [SerializeField] private Text selectedWorldTitleText;

    [Header("Level Selection UI")]
    [SerializeField] private Transform levelGridContainer;
    [SerializeField] private LevelNodeWidget levelNodePrefab;
    [SerializeField] private Button backToWorldsButton;
    [SerializeField] private Button closeMapButton;

    [Header("Scene Transition (Optional)")]
    [Tooltip("Target puzzle scene name if loading across scenes. Leave empty for single-scene setup.")]
    [SerializeField] private string gameplaySceneName = "GameScene";

    [Header("System References")]
    [SerializeField] private LevelFlowController flowController;

    public event Action OnMapClosed;

    private CampaignDatabase database;
    private int currentWorldIndex = 0;
    private bool subscribedToFlow;
    private readonly List<LevelNodeWidget> spawnedLevelWidgets = new List<LevelNodeWidget>();

    // ─────────────────────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        // Subscribe here rather than relying on Start alone: this panel is
        // inactive at scene load and is closed within the same synchronous
        // call chain that opened it (OnLevelNodeSelected -> StartLevel ->
        // CloseMap), so Start can be cancelled before it ever runs.
        SubscribeToFlow();
    }

    private void Start()
    {
        SubscribeToFlow();

        if (ProgressionManager.Instance != null)
            database = ProgressionManager.Instance.Database;

        if (backToWorldsButton != null)
            backToWorldsButton.onClick.AddListener(ShowWorldMap);

        if (closeMapButton != null)
            closeMapButton.onClick.AddListener(CloseMap);

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (subscribedToFlow && flowController != null)
            flowController.OnReturnToMapRequested -= OpenMap;
    }

    private void SubscribeToFlow()
    {
        if (subscribedToFlow) return;
        if (flowController == null)
            flowController = FindAnyObjectByType<LevelFlowController>();
        if (flowController == null) return;

        flowController.OnReturnToMapRequested += OpenMap;
        subscribedToFlow = true;
    }

    // ─────────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────────

    public void OpenMap()
    {
        gameObject.SetActive(true);
        ShowWorldMap();
        RefreshUI();
    }

    public void CloseMap()
    {
        if (worldMapPanel != null) worldMapPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        gameObject.SetActive(false);
        OnMapClosed?.Invoke();
    }

    public void ShowWorldMap()
    {
        if (worldMapPanel != null) worldMapPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

    public void SelectWorld(int worldIndex)
    {
        if (database == null || worldIndex < 0 || worldIndex >= database.Worlds.Count) return;

        var world = database.Worlds[worldIndex];
        if (ProgressionManager.Instance != null && !ProgressionManager.Instance.IsWorldUnlocked(world))
        {
            Debug.LogWarning($"[CampaignMapUI] World '{world.worldName}' is locked! Requires {world.requiredCompletedLevelsToUnlock} completed levels.");
            return;
        }

        currentWorldIndex = worldIndex;
        if (worldMapPanel != null) worldMapPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);

        AnalyticsService.Track(AnalyticsEvents.WorldOpened, new Dictionary<string, object>
        {
            ["world_index"] = worldIndex,
            ["world_name"]  = world.worldName
        });

        PopulateLevels(world);
    }

    // ─────────────────────────────────────────────────────────────
    //  UI Generation
    // ─────────────────────────────────────────────────────────────

    public void RefreshUI()
    {
        if (database == null && ProgressionManager.Instance != null)
            database = ProgressionManager.Instance.Database;

        if (database == null) return;

        // Populate worlds
        if (worldButtonContainer != null && worldButtonPrefab != null)
        {
            for (int i = worldButtonContainer.childCount - 1; i >= 0; i--)
            {
                var child = worldButtonContainer.GetChild(i).gameObject;
                if (child != worldButtonPrefab.gameObject)
                    SafeDestroy(child);
            }

            for (int i = 0; i < database.Worlds.Count; i++)
            {
                var world = database.Worlds[i];
                int index = i;
                Button btn = Instantiate(worldButtonPrefab, worldButtonContainer);
                btn.gameObject.SetActive(true);

                Text label = btn.GetComponentInChildren<Text>();
                bool unlocked = ProgressionManager.Instance == null || ProgressionManager.Instance.IsWorldUnlocked(world);

                if (label != null)
                {
                    label.text = unlocked ? world.worldName : $"{world.worldName} (Locked)";
                }
                btn.interactable = unlocked;
                btn.onClick.AddListener(() => SelectWorld(index));
            }
        }
    }

    private void PopulateLevels(WorldData world)
    {
        if (world == null) return;

        if (selectedWorldTitleText != null)
            selectedWorldTitleText.text = world.worldName;

        if (levelGridContainer == null || levelNodePrefab == null) return;

        // Clear ALL existing children — defensive against stale nodes baked
        // into the scene file or left behind by a previous refresh cycle.
        for (int i = levelGridContainer.childCount - 1; i >= 0; i--)
        {
            var child = levelGridContainer.GetChild(i).gameObject;
            if (child != levelNodePrefab.gameObject)
                SafeDestroy(child);
        }
        spawnedLevelWidgets.Clear();

        if (world.levels == null) return;

        foreach (var lvl in world.levels)
        {
            if (lvl == null) continue;

            LevelNodeWidget widget = Instantiate(levelNodePrefab, levelGridContainer);
            widget.gameObject.SetActive(true);
            widget.Setup(lvl, OnLevelNodeSelected);
            spawnedLevelWidgets.Add(widget);
        }
    }

    private void OnLevelNodeSelected(LevelData level)
    {
        if (level == null) return;

        Debug.Log($"[CampaignMapUI] Selected level: {level.LevelId}");

        // Store in session
        if (GameSession.Instance != null)
        {
            GameSession.Instance.pendingLevel = level;
        }

        // Start the level BEFORE closing the map: OnMapClosed listeners
        // (e.g. MainMenuUI) check the flow state, and it must no longer be
        // Idle when the event fires or the menu would re-open during gameplay.
        if (flowController != null)
        {
            flowController.StartLevel(level);
        }
        else if (!string.IsNullOrEmpty(gameplaySceneName) && SceneManager.GetActiveScene().name != gameplaySceneName)
        {
            SceneManager.LoadScene(gameplaySceneName);
        }

        CloseMap();
    }

    private static void SafeDestroy(GameObject go)
    {
        if (go == null) return;
        if (Application.isPlaying)
            Destroy(go);
        else
            DestroyImmediate(go);
    }
}


using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mission panel (main menu tab). Lists serialized missions with icon,
/// title, progress bar, and a claim button that lights up when complete.
/// Create a row prefab with <see cref="MissionRowWidget"/> and assign the
/// slots; fill the mission list (ids, icons, rewards) in the Inspector.
/// </summary>
public class MissionPanelUI : MonoBehaviour
{
    [Header("Mission List")]
    [SerializeField] private List<MissionService.MissionEntry> missions = new List<MissionService.MissionEntry>();

    [Header("List UI")]
    [SerializeField] private Transform rowContainer;
    [SerializeField] private MissionRowWidget rowPrefab;
    [SerializeField] private GameObject emptyStateRoot;

    private readonly List<MissionRowWidget> spawnedRows = new List<MissionRowWidget>();

    private void OnEnable()
    {
        RefreshList();
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.OnProgressionChanged += RefreshList;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnCoinsChanged += HandleCoinsChanged;
    }

    private void OnDisable()
    {
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.OnProgressionChanged -= RefreshList;
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
    }

    private void HandleCoinsChanged(int amount, int delta) => RefreshList();

    public void RefreshList()
    {
        if (rowContainer == null || rowPrefab == null) return;

        foreach (var row in spawnedRows)
            if (row != null) Destroy(row.gameObject);
        spawnedRows.Clear();

        foreach (var mission in missions)
        {
            if (mission == null || string.IsNullOrEmpty(mission.missionId)) continue;

            var row = Instantiate(rowPrefab, rowContainer);
            row.gameObject.SetActive(true);
            row.Setup(mission, OnClaimPressed);
            spawnedRows.Add(row);
        }

        if (emptyStateRoot != null) emptyStateRoot.SetActive(spawnedRows.Count == 0);
    }

    private void OnClaimPressed(MissionService.MissionEntry mission)
    {
        if (MissionService.TryClaim(mission))
            RefreshList();
    }
}

/// <summary>One mission row — assign icon, title, progress bar, claim button on the prefab.</summary>
public class MissionRowWidget : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text progressText;
    [Tooltip("Optional progress bar Image — set type to Filled.")]
    [SerializeField] private Image progressFill;
    [SerializeField] private Button claimButton;
    [SerializeField] private GameObject claimedIndicator;
    [SerializeField] private TMP_Text rewardText;

    private MissionService.MissionEntry mission;
    private Action<MissionService.MissionEntry> onClaim;

    public void Setup(MissionService.MissionEntry entry, Action<MissionService.MissionEntry> claimCallback)
    {
        mission = entry;
        onClaim = claimCallback;

        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(() => onClaim?.Invoke(mission));
        }
        Refresh();
    }

    public void Refresh()
    {
        if (mission == null) return;

        int progress = MissionService.GetProgress(mission);
        bool claimed = MissionService.IsClaimed(mission);
        bool complete = MissionService.IsComplete(mission);

        if (iconImage != null)
        {
            iconImage.sprite = mission.icon;
            iconImage.enabled = mission.icon != null;
        }
        if (titleText != null) titleText.text = mission.title;
        if (progressText != null) progressText.text = $"{Mathf.Min(progress, mission.target)}/{mission.target}";
        if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01((float)progress / mission.target);
        if (rewardText != null) rewardText.text = mission.rewardGems > 0 ? $"{mission.rewardCoins}c +{mission.rewardGems}g" : $"{mission.rewardCoins}c";
        if (claimButton != null) claimButton.interactable = complete && !claimed;
        if (claimedIndicator != null) claimedIndicator.SetActive(claimed);
    }
}

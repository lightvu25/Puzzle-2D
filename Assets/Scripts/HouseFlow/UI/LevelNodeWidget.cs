using System;
using UnityEngine;
using UnityEngine.UI;
using HouseFlow.Level;
using HouseFlow.Progression;

namespace HouseFlow.UI
{
    /// <summary>
    /// UI widget representing a single level node in the level selection grid.
    /// </summary>
    public class LevelNodeWidget : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text levelTitleText;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private GameObject completedIndicator;
        [SerializeField] private Text starsText;

        private LevelData levelData;
        private Action<LevelData> onSelectedCallback;

        public LevelData LevelData => levelData;

        public void Setup(LevelData data, Action<LevelData> onSelected)
        {
            levelData = data;
            onSelectedCallback = onSelected;

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(HandleClick);
            }

            Refresh();
        }

        public void Refresh()
        {
            if (levelData == null) return;

            if (levelTitleText != null)
            {
                string displayName = string.IsNullOrEmpty(levelData.DisplayName) ? levelData.LevelId : levelData.DisplayName;
                levelTitleText.text = displayName;
            }

            bool isUnlocked = ProgressionManager.Instance == null || ProgressionManager.Instance.IsLevelUnlocked(levelData);
            bool isCompleted = ProgressionManager.Instance != null && ProgressionManager.Instance.IsLevelCompleted(levelData);

            if (selectButton != null)
            {
                selectButton.interactable = isUnlocked;
            }

            if (lockOverlay != null)
            {
                lockOverlay.SetActive(!isUnlocked);
            }

            if (completedIndicator != null)
            {
                completedIndicator.SetActive(isCompleted);
            }

            if (starsText != null)
            {
                if (isCompleted && ProgressionManager.Instance != null)
                {
                    int stars = ProgressionManager.Instance.GetLevelStars(levelData.LevelId);
                    starsText.gameObject.SetActive(true);
                    starsText.text = $"{stars} ★";
                }
                else
                {
                    starsText.gameObject.SetActive(false);
                }
            }
        }

        private void HandleClick()
        {
            if (levelData != null)
            {
                onSelectedCallback?.Invoke(levelData);
            }
        }
    }
}

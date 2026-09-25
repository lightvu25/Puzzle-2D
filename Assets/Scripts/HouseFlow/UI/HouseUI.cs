using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HouseFlow.Economy;
using HouseFlow.House;
using HouseFlow.Progression;

namespace HouseFlow.UI
{
    /// <summary>
    /// Player-facing House screen. Renders HouseProgressionDatabase features as
    /// cards and routes unlock evaluation through HouseProgressionManager —
    /// the UI owns no progression logic. Feature unlocks are data-driven
    /// (completed levels + Showcase Acclaim), never purchases.
    /// </summary>
    public class HouseUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject housePanel;
        [SerializeField] private Button backButton;

        [Header("Stats Header")]
        [SerializeField] private Text completedText;
        [SerializeField] private Text acclaimText;

        [Header("Features")]
        [SerializeField] private Transform featuresContainer;
        [SerializeField] private GameObject featureCardTemplate;
        [SerializeField] private Text feedbackText;

        /// <summary>Fired when the player closes the house screen (back button).</summary>
        public event Action OnHouseClosed;

        private readonly List<GameObject> spawnedCards = new List<GameObject>();
        private HouseProgressionManager houseManager;
        private EconomyManager economy;
        private ProgressionManager progression;
        private bool subscribed;

        private void Awake()
        {
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
        }

        private void Start()
        {
            // Panel stays active in the saved scene so Awake/Start execute;
            // hide until the player opens the house screen.
            if (housePanel != null && housePanel.activeSelf)
                housePanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (houseManager == null) houseManager = HouseProgressionManager.Instance ?? FindAnyObjectByType<HouseProgressionManager>();
            if (economy == null) economy = EconomyManager.Instance ?? FindAnyObjectByType<EconomyManager>();
            if (progression == null) progression = ProgressionManager.Instance ?? FindAnyObjectByType<ProgressionManager>();
            Subscribe();
            RefreshAll();
        }

        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();

        private void Subscribe()
        {
            if (subscribed) return;
            subscribed = true;
            if (houseManager != null)
                houseManager.OnHouseUnlockChanged += HandleFeatureUnlocked;
        }

        private void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;
            if (houseManager != null)
                houseManager.OnHouseUnlockChanged -= HandleFeatureUnlocked;
        }

        // ─────────────────────────────────────────────────────────────
        //  Open / Close
        // ─────────────────────────────────────────────────────────────

        public void OpenHouse()
        {
            if (housePanel != null) housePanel.SetActive(true);
            // OnEnable performs RefreshAll; no extra work needed here.
        }

        public void CloseHouse()
        {
            if (housePanel != null) housePanel.SetActive(false);
            OnHouseClosed?.Invoke();
        }

        private void OnBackClicked() => CloseHouse();

        // ─────────────────────────────────────────────────────────────
        //  Refresh
        // ─────────────────────────────────────────────────────────────

        public void RefreshAll()
        {
            // Evaluate current progression stats against feature requirements.
            // This is the backend's designed evaluation entry point.
            if (houseManager != null)
            {
                int completed = progression != null ? progression.GetCompletedLevelCount() : 0;
                int acclaim = economy != null ? economy.ShowcaseAcclaim : 0;
                houseManager.CheckAndUnlockFeatures(completed, acclaim);
            }

            UpdateStatsTexts();
            RebuildFeatureCards();
        }

        private void UpdateStatsTexts()
        {
            int completed = progression != null ? progression.GetCompletedLevelCount() : 0;
            int acclaim = economy != null ? economy.ShowcaseAcclaim : 0;
            if (completedText != null) completedText.text = completed.ToString();
            if (acclaimText != null) acclaimText.text = acclaim.ToString();
        }

        private void HandleFeatureUnlocked(HouseFeatureDefinition feature)
        {
            ShowFeedback($"Unlocked: {feature.DisplayName}!");
            RebuildFeatureCards();
        }

        private void RebuildFeatureCards()
        {
            if (featuresContainer == null || featureCardTemplate == null || houseManager == null) return;

            // Destroy generated cards (iterate backwards; skip the inactive template).
            for (int i = featuresContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = featuresContainer.GetChild(i);
                if (child.gameObject == featureCardTemplate) continue;
                Destroy(child.gameObject);
            }
            spawnedCards.Clear();

            var db = houseManager.Database;
            if (db == null) return;

            int completed = progression != null ? progression.GetCompletedLevelCount() : 0;
            int acclaim = economy != null ? economy.ShowcaseAcclaim : 0;

            foreach (var feature in db.Features)
            {
                if (feature == null) continue;
                var card = Instantiate(featureCardTemplate, featuresContainer);
                card.name = $"HouseCard_{feature.FeatureId}";
                card.SetActive(true);
                SetupCard(card, feature, completed, acclaim);
                spawnedCards.Add(card);
            }
        }

        private void SetupCard(GameObject card, HouseFeatureDefinition feature, int completed, int acclaim)
        {
            var nameText = FindChildText(card.transform, "NameText");
            var descText = FindChildText(card.transform, "DescText");
            var reqText = FindChildText(card.transform, "ReqText");
            var stateText = FindChildText(card.transform, "StateText");

            if (nameText != null) nameText.text = feature.DisplayName;
            if (descText != null) descText.text = feature.Description;
            if (reqText != null) reqText.text = FormatRequirements(feature);
            if (stateText != null)
            {
                bool unlocked = houseManager.IsFeatureUnlocked(feature.FeatureId);
                stateText.text = unlocked ? "UNLOCKED" : "LOCKED";
                stateText.color = unlocked ? new Color(0.35f, 0.85f, 0.45f) : new Color(0.7f, 0.7f, 0.75f);
            }
        }

        private string FormatRequirements(HouseFeatureDefinition feature)
        {
            var parts = new List<string>();
            if (feature.RequiredCompletedLevels > 0)
                parts.Add($"{feature.RequiredCompletedLevels} completed levels");
            if (feature.RequiredAcclaim > 0)
                parts.Add($"{feature.RequiredAcclaim} Acclaim");
            return parts.Count > 0 ? "Requires: " + string.Join(" + ", parts) : "No requirements";
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText != null) feedbackText.text = message;
        }

        private static Text FindChildText(Transform root, string childName)
        {
            var t = root.Find(childName);
            return t != null ? t.GetComponent<Text>() : null;
        }
    }
}

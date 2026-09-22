using System;
using System.Collections.Generic;
using UnityEngine;

namespace HouseFlow.House
{
    /// <summary>
    /// Singleton service managing data-driven House meta progression.
    /// Unlocks cosmetic rooms and milestone wings without introducing artificial gameplay power locks.
    /// </summary>
    public class HouseProgressionManager : MonoBehaviour
    {
        public static HouseProgressionManager Instance { get; private set; }

        [Header("Database")]
        [Tooltip("The catalog of all House feature definitions.")]
        [SerializeField] private HouseProgressionDatabase database;

        /// <summary>Fired whenever a new house feature is unlocked.</summary>
        public event Action<HouseFeatureDefinition> OnHouseUnlockChanged;

        private readonly HashSet<string> unlockedFeatureIDs = new HashSet<string>();

        public HouseProgressionDatabase Database => database;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LoadFromProfile();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ─────────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Checks whether the specified house feature has been unlocked.
        /// </summary>
        public bool IsFeatureUnlocked(string featureId)
        {
            if (string.IsNullOrEmpty(featureId)) return false;
            return unlockedFeatureIDs.Contains(featureId);
        }

        /// <summary>
        /// Evaluates all features in the database against the player's current completion count
        /// and Showcase Acclaim, unlocking any newly eligible features.
        /// </summary>
        public void CheckAndUnlockFeatures(int completedLevelsCount, int currentAcclaim)
        {
            if (database == null || database.Features == null) return;

            foreach (var feature in database.Features)
            {
                if (feature == null) continue;
                if (IsFeatureUnlocked(feature.FeatureId)) continue;

                if (feature.MeetsRequirements(completedLevelsCount, currentAcclaim))
                {
                    UnlockFeature(feature);
                }
            }
        }

        /// <summary>
        /// Explicitly unlocks a house feature, persists the update, and notifies listeners.
        /// </summary>
        public void UnlockFeature(HouseFeatureDefinition feature)
        {
            if (feature == null || string.IsNullOrEmpty(feature.FeatureId)) return;
            if (unlockedFeatureIDs.Contains(feature.FeatureId)) return;

            unlockedFeatureIDs.Add(feature.FeatureId);
            SaveToProfile();

            Debug.Log($"[HouseProgressionManager] Unlocked House Feature: '{feature.DisplayName}' (ID: {feature.FeatureId})!");
            if (!string.IsNullOrEmpty(feature.VisitorAppraisalNotice))
            {
                Debug.Log($"[HouseProgressionManager] Visitor Appraisal: {feature.VisitorAppraisalNotice}");
            }

            OnHouseUnlockChanged?.Invoke(feature);
        }

        // ─────────────────────────────────────────────────────────────
        //  Persistence Integration
        // ─────────────────────────────────────────────────────────────

        public void LoadFromProfile()
        {
            unlockedFeatureIDs.Clear();
            ProfileData profile = SaveManager.loadProfile();
            if (profile != null && profile.unlockedHouseFeatureIDs != null)
            {
                foreach (var id in profile.unlockedHouseFeatureIDs)
                {
                    if (!string.IsNullOrEmpty(id))
                        unlockedFeatureIDs.Add(id);
                }
            }
        }

        private void SaveToProfile()
        {
            ProfileData profile = SaveManager.loadProfile() ?? new ProfileData();
            if (profile.unlockedHouseFeatureIDs == null)
                profile.unlockedHouseFeatureIDs = new List<string>();

            profile.unlockedHouseFeatureIDs.Clear();
            profile.unlockedHouseFeatureIDs.AddRange(unlockedFeatureIDs);

            SaveManager.saveProfile(profile);
        }

        public void SetDatabaseForTesting(HouseProgressionDatabase db)
        {
            database = db;
        }
    }
}

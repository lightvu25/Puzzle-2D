using System;
using System.Collections.Generic;
using UnityEngine;

namespace HouseFlow.Cosmetics
{
    /// <summary>
    /// Singleton service managing owned cosmetic items.
    /// Integrates persistence directly with ProfileData.ownedCosmeticIDs.
    /// </summary>
    public class CosmeticInventory : MonoBehaviour
    {
        public static CosmeticInventory Instance { get; private set; }

        /// <summary>Fired when a new cosmetic item is acquired.</summary>
        public event Action<string> OnCosmeticGranted;

        private readonly HashSet<string> ownedCosmeticIDs = new HashSet<string>();

        public IReadOnlyCollection<string> OwnedCosmeticIDs => ownedCosmeticIDs;

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
        /// Checks whether the player owns the specified cosmetic.
        /// </summary>
        public bool OwnsCosmetic(string cosmeticId)
        {
            if (string.IsNullOrEmpty(cosmeticId)) return false;
            return ownedCosmeticIDs.Contains(cosmeticId);
        }

        /// <summary>
        /// Grants ownership of a cosmetic item and persists it.
        /// </summary>
        public void GrantCosmetic(string cosmeticId)
        {
            if (string.IsNullOrEmpty(cosmeticId)) return;
            if (ownedCosmeticIDs.Contains(cosmeticId)) return;

            ownedCosmeticIDs.Add(cosmeticId);
            SaveToProfile();
            OnCosmeticGranted?.Invoke(cosmeticId);
            Debug.Log($"[CosmeticInventory] Granted cosmetic: '{cosmeticId}'.");
        }

        // ─────────────────────────────────────────────────────────────
        //  Persistence Integration
        // ─────────────────────────────────────────────────────────────

        public void LoadFromProfile()
        {
            ownedCosmeticIDs.Clear();
            ProfileData profile = SaveManager.loadProfile();
            if (profile != null && profile.ownedCosmeticIDs != null)
            {
                foreach (var id in profile.ownedCosmeticIDs)
                {
                    if (!string.IsNullOrEmpty(id))
                        ownedCosmeticIDs.Add(id);
                }
            }
        }

        private void SaveToProfile()
        {
            ProfileData profile = SaveManager.loadProfile() ?? new ProfileData();
            if (profile.ownedCosmeticIDs == null)
                profile.ownedCosmeticIDs = new List<string>();

            profile.ownedCosmeticIDs.Clear();
            profile.ownedCosmeticIDs.AddRange(ownedCosmeticIDs);
            SaveManager.saveProfile(profile);
        }

        public void ClearInventory()
        {
            ownedCosmeticIDs.Clear();
            SaveToProfile();
        }
    }
}

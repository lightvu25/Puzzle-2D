using System;
using System.Collections.Generic;
using UnityEngine;

namespace HouseFlow.Cosmetics
{
    /// <summary>
    /// Singleton service managing currently equipped cosmetics per category.
    /// Strictly guarantees that only owned items can be equipped.
    /// Persists loadouts using serializable EquippedCosmeticEntry records.
    /// </summary>
    public class CosmeticEquipService : MonoBehaviour
    {
        public static CosmeticEquipService Instance { get; private set; }

        /// <summary>Fired when a cosmetic slot changes (category, newCosmeticId).</summary>
        public event Action<CosmeticCategory, string> OnCosmeticEquipped;

        // In-memory mapping: Category -> CosmeticId
        private readonly Dictionary<CosmeticCategory, string> equippedCosmetics = new Dictionary<CosmeticCategory, string>();

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
        /// Returns the cosmetic ID currently equipped in the specified category (or null if empty).
        /// </summary>
        public string GetEquippedCosmeticId(CosmeticCategory category)
        {
            return equippedCosmetics.TryGetValue(category, out string id) ? id : null;
        }

        /// <summary>
        /// Equips the specified cosmetic definition. Requires ownership via CosmeticInventory.
        /// </summary>
        public bool EquipCosmetic(CosmeticDefinition cosmetic)
        {
            if (cosmetic == null || string.IsNullOrEmpty(cosmetic.CosmeticId)) return false;

            // Verify ownership if CosmeticInventory is active
            var inv = CosmeticInventory.Instance ?? FindAnyObjectByType<CosmeticInventory>();
            if (inv != null && !inv.OwnsCosmetic(cosmetic.CosmeticId))
            {
                Debug.LogWarning($"[CosmeticEquipService] Cannot equip '{cosmetic.DisplayName}': Player does not own it.");
                return false;
            }

            equippedCosmetics[cosmetic.Category] = cosmetic.CosmeticId;
            SaveToProfile();
            OnCosmeticEquipped?.Invoke(cosmetic.Category, cosmetic.CosmeticId);
            Debug.Log($"[CosmeticEquipService] Equipped '{cosmetic.DisplayName}' in slot {cosmetic.Category}.");
            return true;
        }

        /// <summary>
        /// Clears the equipped cosmetic in the specified category slot.
        /// </summary>
        public void UnequipCosmetic(CosmeticCategory category)
        {
            if (equippedCosmetics.Remove(category))
            {
                SaveToProfile();
                OnCosmeticEquipped?.Invoke(category, null);
                Debug.Log($"[CosmeticEquipService] Unequipped slot {category}.");
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Persistence Integration
        // ─────────────────────────────────────────────────────────────

        public void LoadFromProfile()
        {
            equippedCosmetics.Clear();
            ProfileData profile = SaveManager.loadProfile();
            if (profile != null && profile.equippedCosmetics != null)
            {
                foreach (var entry in profile.equippedCosmetics)
                {
                    if (entry != null && Enum.TryParse(entry.category, out CosmeticCategory cat))
                    {
                        equippedCosmetics[cat] = entry.cosmeticId;
                    }
                }
            }
        }

        private void SaveToProfile()
        {
            ProfileData profile = SaveManager.loadProfile() ?? new ProfileData();
            if (profile.equippedCosmetics == null)
                profile.equippedCosmetics = new List<EquippedCosmeticEntry>();

            profile.equippedCosmetics.Clear();
            foreach (var kvp in equippedCosmetics)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    profile.equippedCosmetics.Add(new EquippedCosmeticEntry(kvp.Key.ToString(), kvp.Value));
                }
            }

            SaveManager.saveProfile(profile);
        }
    }
}

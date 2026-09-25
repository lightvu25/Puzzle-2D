using System;
using System.Collections.Generic;
using UnityEngine;

namespace HouseFlow.Tools
{
    /// <summary>
    /// Singleton service managing owned Troubleshooter Tool quantities.
    /// Uses serializable ToolInventoryEntry records in ProfileData to avoid fragile parallel lists.
    /// Tools have consumable quantities and are never mandatory to complete a puzzle.
    /// </summary>
    public class ToolInventory : MonoBehaviour
    {
        public static ToolInventory Instance { get; private set; }

        /// <summary>Fired whenever a tool's inventory quantity changes (toolId, newTotal).</summary>
        public event Action<string, int> OnToolQuantityChanged;

        [Header("Starter Loadout")]
        [Tooltip("Granted once to brand-new installs (no existing save file). Data-driven starter configuration.")]
        [SerializeField] private ToolInventoryEntry[] starterTools;

        // In-memory lookup cache
        private readonly Dictionary<string, int> toolQuantities = new Dictionary<string, int>();

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
        /// Returns the current quantity of the specified tool.
        /// </summary>
        public int GetToolCount(string toolId)
        {
            if (string.IsNullOrEmpty(toolId)) return 0;
            return toolQuantities.TryGetValue(toolId, out int count) ? count : 0;
        }

        /// <summary>
        /// Checks whether the player possesses at least the required quantity of the tool.
        /// </summary>
        public bool HasTool(string toolId, int required = 1)
        {
            if (required <= 0) return true;
            return GetToolCount(toolId) >= required;
        }

        /// <summary>
        /// Adds a quantity of the specified tool to the player's inventory.
        /// </summary>
        public void AddTool(string toolId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(toolId) || quantity <= 0) return;

            int current = GetToolCount(toolId);
            int newTotal = current + quantity;
            toolQuantities[toolId] = newTotal;

            SaveToProfile();
            OnToolQuantityChanged?.Invoke(toolId, newTotal);
            Debug.Log($"[ToolInventory] Added {quantity}x '{toolId}'. New Total: {newTotal}");
        }

        /// <summary>
        /// Consumes a quantity of the specified tool. Returns false if insufficient quantity.
        /// Guaranteed never to allow negative counts.
        /// </summary>
        public bool ConsumeTool(string toolId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(toolId) || quantity <= 0) return false;
            if (!HasTool(toolId, quantity))
            {
                Debug.LogWarning($"[ToolInventory] ConsumeTool failed: Insufficient quantity of '{toolId}' ({GetToolCount(toolId)} < {quantity})");
                return false;
            }

            int current = GetToolCount(toolId);
            int newTotal = current - quantity;
            toolQuantities[toolId] = newTotal;

            SaveToProfile();
            OnToolQuantityChanged?.Invoke(toolId, newTotal);
            Debug.Log($"[ToolInventory] Consumed {quantity}x '{toolId}'. Remaining: {newTotal}");
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        //  Persistence Integration
        // ─────────────────────────────────────────────────────────────

        public void LoadFromProfile()
        {
            toolQuantities.Clear();
            ProfileData profile = SaveManager.loadProfile();
            if (profile != null && profile.toolInventory != null)
            {
                foreach (var entry in profile.toolInventory)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.toolId))
                    {
                        toolQuantities[entry.toolId] = Math.Max(0, entry.quantity);
                    }
                }
            }

            if (!SaveManager.HasSavedProfile())
            {
                GrantStarterTools();
            }
        }

        /// <summary>
        /// Applies the configured starter loadout. Only invoked when no save file
        /// exists, so it can never overwrite or duplicate a returning player's inventory.
        /// </summary>
        private void GrantStarterTools()
        {
            if (starterTools == null) return;

            bool granted = false;
            foreach (var entry in starterTools)
            {
                if (entry == null || string.IsNullOrEmpty(entry.toolId) || entry.quantity <= 0) continue;
                toolQuantities[entry.toolId] = GetToolCount(entry.toolId) + entry.quantity;
                granted = true;
            }

            if (granted)
            {
                SaveToProfile();
                Debug.Log("[ToolInventory] Granted starter tool loadout to new profile.");
            }
        }

        private void SaveToProfile()
        {
            ProfileData profile = SaveManager.loadProfile() ?? new ProfileData();
            if (profile.toolInventory == null)
                profile.toolInventory = new List<ToolInventoryEntry>();

            profile.toolInventory.Clear();
            foreach (var kvp in toolQuantities)
            {
                if (kvp.Value > 0)
                {
                    profile.toolInventory.Add(new ToolInventoryEntry(kvp.Key, kvp.Value));
                }
            }

            SaveManager.saveProfile(profile);
        }

        /// <summary>
        /// Clears all tool quantities from memory and persistent storage.
        /// </summary>
        public void ClearInventory()
        {
            toolQuantities.Clear();
            SaveToProfile();
        }
    }
}

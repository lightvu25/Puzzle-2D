using System;
using UnityEngine;
using HouseFlow.Economy;
using HouseFlow.Tools;
using HouseFlow.Cosmetics;

namespace HouseFlow.Rewards
{
    /// <summary>
    /// Central dispatcher responsible for applying granted RewardBundles to the respective player domains:
    /// EconomyManager (Coins, Gems, Acclaim), ToolInventory (Tools), and CosmeticInventory (Cosmetics).
    /// </summary>
    public class RewardService : MonoBehaviour
    {
        public static RewardService Instance { get; private set; }

        /// <summary>Fired whenever a reward bundle is successfully applied.</summary>
        public event Action<RewardBundle> OnRewardGranted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Applies all reward items within the specified bundle to player systems.
        /// </summary>
        /// <param name="bundle">The reward bundle to grant.</param>
        /// <param name="reason">Context tag for analytics/audit logging.</param>
        public bool GrantRewardBundle(RewardBundle bundle, string reason = "Unspecified")
        {
            if (bundle == null || bundle.IsEmpty)
            {
                Debug.LogWarning($"[RewardService] Attempted to grant null or empty bundle. Reason: {reason}");
                return false;
            }

            var economy = EconomyManager.Instance ?? FindAnyObjectByType<EconomyManager>();
            var toolInv = ToolInventory.Instance ?? FindAnyObjectByType<ToolInventory>();
            var cosmeticInv = CosmeticInventory.Instance ?? FindAnyObjectByType<CosmeticInventory>();

            foreach (var item in bundle.Items)
            {
                if (item.Amount <= 0) continue;

                switch (item.Type)
                {
                    case RewardType.Coins:
                        if (economy != null)
                            economy.AddCoins(item.Amount, reason);
                        else
                            Debug.LogWarning($"[RewardService] EconomyManager unavailable to grant {item.Amount} Coins.");
                        break;

                    case RewardType.Gems:
                        if (economy != null)
                            economy.AddGems(item.Amount, reason);
                        else
                            Debug.LogWarning($"[RewardService] EconomyManager unavailable to grant {item.Amount} Gems.");
                        break;

                    case RewardType.ShowcaseAcclaim:
                        if (economy != null)
                            economy.AddShowcaseAcclaim(item.Amount);
                        else
                            Debug.LogWarning($"[RewardService] EconomyManager unavailable to grant {item.Amount} Acclaim.");
                        break;

                    case RewardType.Tool:
                        if (toolInv != null && !string.IsNullOrEmpty(item.ItemId))
                            toolInv.AddTool(item.ItemId, item.Amount);
                        else
                            Debug.LogWarning($"[RewardService] ToolInventory unavailable or invalid Tool ItemId '{item.ItemId}'.");
                        break;

                    case RewardType.Cosmetic:
                        if (cosmeticInv != null)
                            cosmeticInv.GrantCosmetic(item.ItemId);
                        else
                            Debug.LogWarning($"[RewardService] CosmeticInventory unavailable to grant '{item.ItemId}'.");
                        break;
                }
            }

            OnRewardGranted?.Invoke(bundle);
            Debug.Log($"[RewardService] Successfully granted reward bundle ({bundle.Items.Count} items). Reason: '{reason}'.");
            return true;
        }
    }
}

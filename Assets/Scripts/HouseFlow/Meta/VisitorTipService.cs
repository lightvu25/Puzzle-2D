using System;
using UnityEngine;
using HouseFlow.Economy;
using HouseFlow.Monetization;
using HouseFlow.Rewards;

namespace HouseFlow.Meta
{
    /// <summary>
    /// Manages visitor tip generation based on showcase acclaim.
    /// Supports cooldown tracking and optional rewarded ad tip doubling.
    /// </summary>
    public class VisitorTipService : MonoBehaviour
    {
        public static VisitorTipService Instance { get; private set; }

        public const double TipCooldownHours = 4.0;
        public const int BaseTipCoins = 30;
        public const int AcclaimBonusCoinMultiplier = 2;

        public event Action<int> OnVisitorTipsClaimed;

        private long lastClaimUtcTicks;

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

        /// <summary>
        /// Checks if tips are ready for collection.
        /// </summary>
        public bool CanCollectTips()
        {
            if (lastClaimUtcTicks == 0) return true;

            var lastClaim = new DateTime(lastClaimUtcTicks, DateTimeKind.Utc);
            return (DateTime.UtcNow - lastClaim).TotalHours >= TipCooldownHours;
        }

        /// <summary>
        /// Time remaining until next tip collection is available.
        /// </summary>
        public TimeSpan GetTimeUntilNextTip()
        {
            if (CanCollectTips()) return TimeSpan.Zero;

            var lastClaim = new DateTime(lastClaimUtcTicks, DateTimeKind.Utc);
            var nextAvailable = lastClaim.AddHours(TipCooldownHours);
            var diff = nextAvailable - DateTime.UtcNow;
            return diff > TimeSpan.Zero ? diff : TimeSpan.Zero;
        }

        /// <summary>
        /// Calculates current tip coin yield based on Showcase Acclaim.
        /// </summary>
        public int CalculateTipAmount()
        {
            var eco = EconomyManager.Instance ?? FindAnyObjectByType<EconomyManager>();
            int acclaim = eco != null ? eco.ShowcaseAcclaim : 0;
            return BaseTipCoins + (acclaim * AcclaimBonusCoinMultiplier);
        }

        /// <summary>
        /// Collects visitor tips.
        /// </summary>
        /// <param name="doubleWithAd">If true, attempts to watch a rewarded ad to double the tips.</param>
        /// <param name="onComplete">Callback with true on successful collection.</param>
        public void CollectTips(bool doubleWithAd = false, Action<bool> onComplete = null)
        {
            if (!CanCollectTips())
            {
                Debug.LogWarning("[VisitorTipService] Tips not ready to collect yet.");
                onComplete?.Invoke(false);
                return;
            }

            int amount = CalculateTipAmount();
            var rewardService = RewardService.Instance ?? FindAnyObjectByType<RewardService>();

            if (doubleWithAd)
            {
                var adService = AdRewardService.Instance ?? FindAnyObjectByType<AdRewardService>();
                if (adService != null && adService.IsAdAvailable(RewardedAdPlacement.VisitorTipMultiplier))
                {
                    adService.WatchAdForReward(RewardedAdPlacement.VisitorTipMultiplier, amount, adSuccess =>
                    {
                        // Base amount is always granted if ad completed, plus the ad bonus awarded by AdRewardService
                        if (adSuccess)
                        {
                            GrantBaseAndFinalize(amount, rewardService);
                            onComplete?.Invoke(true);
                        }
                        else
                        {
                            // User cancelled/skipped ad: grant standard base amount
                            GrantBaseAndFinalize(amount, rewardService);
                            onComplete?.Invoke(true);
                        }
                    });
                    return;
                }
            }

            GrantBaseAndFinalize(amount, rewardService);
            onComplete?.Invoke(true);
        }

        private void GrantBaseAndFinalize(int amount, RewardService rewardService)
        {
            if (rewardService != null)
            {
                var bundle = new RewardBundle();
                bundle.Add(RewardType.Coins, amount);
                rewardService.GrantRewardBundle(bundle, "VisitorTips");
            }

            lastClaimUtcTicks = DateTime.UtcNow.Ticks;
            SaveToProfile();

            OnVisitorTipsClaimed?.Invoke(amount);
            Debug.Log($"[VisitorTipService] Successfully collected {amount} visitor tip coins.");
        }

        // ─────────────────────────────────────────────────────────────
        //  Persistence Integration
        // ─────────────────────────────────────────────────────────────

        public void LoadFromProfile()
        {
            ProfileData profile = SaveManager.loadProfile();
            if (profile != null)
            {
                lastClaimUtcTicks = profile.lastVisitorTipClaimUtcTicks;
            }
            else
            {
                lastClaimUtcTicks = 0;
            }
        }

        private void SaveToProfile()
        {
            ProfileData profile = SaveManager.loadProfile() ?? new ProfileData();
            profile.lastVisitorTipClaimUtcTicks = lastClaimUtcTicks;
            SaveManager.saveProfile(profile);
        }

        public void ResetForTesting()
        {
            lastClaimUtcTicks = 0;
            SaveToProfile();
        }
    }
}

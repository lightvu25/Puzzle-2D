using UnityEngine;
using HouseFlow.Level;

namespace HouseFlow.Rewards
{
    /// <summary>
    /// Pure calculation logic evaluating puzzle completion context into a RewardBundle.
    /// Ensures replay completions grant reduced soft currency without inflating premium gems or showcase acclaim.
    /// </summary>
    public static class RewardCalculator
    {
        public const int FirstClearBaseCoins = 50;
        public const int OptionalObjectiveBonusCoins = 25;
        public const int ReplayBaseCoins = 15;

        public const int FirstClearBaseAcclaim = 10;
        public const int OptionalObjectiveBonusAcclaim = 5;

        public const int ThreeStarBonusGems = 1;

        /// <summary>
        /// Calculates the rewards earned for completing a puzzle level.
        /// </summary>
        /// <param name="levelData">Level data definition.</param>
        /// <param name="starsEarned">Total stars earned (1 to 3).</param>
        /// <param name="isFirstClear">Whether this level has never been cleared before.</param>
        /// <param name="optionalObjectivesCompleted">Number of optional objectives met.</param>
        /// <returns>A configured RewardBundle ready for dispatch by RewardService.</returns>
        public static RewardBundle CalculateLevelRewards(
            LevelData levelData, 
            int starsEarned, 
            bool isFirstClear, 
            int optionalObjectivesCompleted)
        {
            var bundle = new RewardBundle();

            if (isFirstClear)
            {
                // First-clear reward calculation
                int totalCoins = FirstClearBaseCoins + (Mathf.Max(0, optionalObjectivesCompleted) * OptionalObjectiveBonusCoins);
                bundle.Add(RewardType.Coins, totalCoins);

                int totalAcclaim = FirstClearBaseAcclaim + (Mathf.Max(0, optionalObjectivesCompleted) * OptionalObjectiveBonusAcclaim);
                bundle.Add(RewardType.ShowcaseAcclaim, totalAcclaim);

                // Premium gem bonus for flawless first 3-star performance
                if (starsEarned >= 3)
                {
                    bundle.Add(RewardType.Gems, ThreeStarBonusGems);
                }
            }
            else
            {
                // Replay completion: modest soft currency only, strictly zero acclaim and zero gems
                bundle.Add(RewardType.Coins, ReplayBaseCoins);
            }

            return bundle;
        }
    }
}

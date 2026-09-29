using System.Collections.Generic;

/// <summary>
/// Serializable player profile persisted by SaveManager.
/// Progression, economy and persistence-key data are kept here as plain
/// serializable fields so JsonUtility can round-trip them.
/// </summary>
public class ProfileData
{
    // ── Run stats (roguelike layer, kept as stub data) ────────────────────
    public int totalGold;
    public int deaths;

    public List<string> unlockedSkillIDs = new List<string>();
    public List<string> persistenceKeys = new List<string>();

    public List<int> attemptLevelKeys   = new List<int>();
    public List<int> attemptLevelValues = new List<int>();

    // ── Level Progression ─────────────────────────────────────────────────
    public List<string> unlockedLevelIDs  = new List<string>();
    public List<string> completedLevelIDs = new List<string>();
    public List<string> levelStarKeys     = new List<string>();
    public List<int>    levelStarValues   = new List<int>();

    // ── Economy ───────────────────────────────────────────────────────────
    public int coins;
    public int gems;
    public int showcaseAcclaim;

    // ── Purchases & Services ──────────────────────────────────────────────
    public bool noAdsPurchased;
    public List<string> purchasedProductIDs = new List<string>();
    public List<string> purchasedShopItemIDs = new List<string>();
    public long lastVisitorTipClaimUtcTicks;
    public long lastDailyRewardClaimUtcTicks;
    public int dailyRewardStreakDay;
    public bool tutorialCompleted;

    public int GetLevelAttempts(int levelIndex)
    {
        int idx = attemptLevelKeys.IndexOf(levelIndex);
        return idx >= 0 ? attemptLevelValues[idx] : 0;
    }

    public void IncrementLevelAttempt(int levelIndex)
    {
        int idx = attemptLevelKeys.IndexOf(levelIndex);
        if (idx >= 0)
            attemptLevelValues[idx]++;
        else
        {
            attemptLevelKeys.Add(levelIndex);
            attemptLevelValues.Add(1);
        }
    }

    public bool HasSkill(string skillID) =>
        unlockedSkillIDs != null && unlockedSkillIDs.Contains(skillID);
}

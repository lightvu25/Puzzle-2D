using System.Collections.Generic;
using System;

public class ProfileData
{
    public int totalGold;
    public int bankedAstralShards;
    public int deaths;
    public int kills;
    public int timeRun;
    public int bonusStartingMaxHP;
    public List<string> unlockedWeaponIDs = new List<string>();
    public List<string> unlockedSkillIDs = new List<string>();
    public List<string> persistenceKeys = new List<string>();

    public List<string> partialSkillIDs = new List<string>();
    public List<int> partialSkillAmounts = new List<int>();

    public List<int> attemptLevelKeys   = new List<int>();
    public List<int> attemptLevelValues = new List<int>();

    // ── HOUSEFLOW Progression ─────────────────────────────────────────────
    public List<string> unlockedLevelIDs  = new List<string>();
    public List<string> completedLevelIDs = new List<string>();
    public List<string> levelStarKeys     = new List<string>();
    public List<int>    levelStarValues   = new List<int>();

    // ── HOUSEFLOW Economy ─────────────────────────────────────────────────
    public int coins;
    public int gems;
    public int showcaseAcclaim;

    // ── HOUSEFLOW Meta & Inventory ────────────────────────────────────────
    public List<ToolInventoryEntry> toolInventory = new List<ToolInventoryEntry>();
    public List<string> unlockedHouseFeatureIDs = new List<string>();
    public List<string> ownedCosmeticIDs = new List<string>();
    public List<EquippedCosmeticEntry> equippedCosmetics = new List<EquippedCosmeticEntry>();
    public bool noAdsPurchased;
    public List<string> purchasedProductIDs = new List<string>();
    public List<string> purchasedShopItemIDs = new List<string>();
    public long lastVisitorTipClaimUtcTicks;
    public long lastDailyRewardClaimUtcTicks;
    // ──────────────────────────────────────────────────────────────────────

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

    public ProfileData()
    {
        totalGold        = 0;
        bankedAstralShards = 0;
        bonusStartingMaxHP = 0;
        unlockedWeaponIDs.Add("Sword_Basic");
    }

    public bool HasSkill(string skillID) =>
        unlockedSkillIDs != null && unlockedSkillIDs.Contains(skillID);

    public int GetInvestedAmount(string skillID)
    {
        if (partialSkillIDs == null || partialSkillAmounts == null) return 0;
        int idx = partialSkillIDs.IndexOf(skillID);
        return idx >= 0 ? partialSkillAmounts[idx] : 0;
    }

    public void SetInvestedAmount(string skillID, int amount)
    {
        if (partialSkillIDs == null) partialSkillIDs = new List<string>();
        if (partialSkillAmounts == null) partialSkillAmounts = new List<int>();

        int idx = partialSkillIDs.IndexOf(skillID);
        if (idx >= 0)
        {
            if (amount <= 0)
            {
                partialSkillIDs.RemoveAt(idx);
                partialSkillAmounts.RemoveAt(idx);
            }
            else
            {
                partialSkillAmounts[idx] = amount;
            }
        }
        else if (amount > 0)
        {
            partialSkillIDs.Add(skillID);
            partialSkillAmounts.Add(amount);
        }
    }
}

[System.Serializable]
public class ToolInventoryEntry
{
    public string toolId;
    public int quantity;

    public ToolInventoryEntry() { }

    public ToolInventoryEntry(string toolId, int quantity)
    {
        this.toolId = toolId;
        this.quantity = quantity;
    }
}

[System.Serializable]
public class EquippedCosmeticEntry
{
    public string category;
    public string cosmeticId;

    public EquippedCosmeticEntry() { }

    public EquippedCosmeticEntry(string category, string cosmeticId)
    {
        this.category = category;
        this.cosmeticId = cosmeticId;
    }
}
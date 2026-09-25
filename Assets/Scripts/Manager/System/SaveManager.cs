using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string profilePath => Path.Combine(Application.persistentDataPath, "profile.json");
    private static string runPath => Path.Combine(Application.persistentDataPath, "run_temp.json");

    public static void saveProfile(ProfileData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(profilePath, json);
    }

    public static void saveRun(RunData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(runPath, json);
    }

    public static ProfileData loadProfile()
    {
        if (File.Exists(profilePath))
        {
            string json = File.ReadAllText(profilePath);
            return JsonUtility.FromJson<ProfileData>(json);
        }

        return new ProfileData();
    }

    /// <summary>Returns true if a persisted profile file exists on disk.</summary>
    public static bool HasSavedProfile() => File.Exists(profilePath);

    public static RunData loadRun()
    {
        if (File.Exists(runPath))
        {
            string json = File.ReadAllText(runPath);
            return JsonUtility.FromJson<RunData>(json);
        }
        return null;
    }

    public static void deleteRun()
    {
        File.Delete(runPath);
    }

    public static void ResetHouseflowProgression()
    {
        ProfileData profile = loadProfile();
        if (profile != null)
        {
            profile.unlockedLevelIDs?.Clear();
            profile.completedLevelIDs?.Clear();
            profile.levelStarKeys?.Clear();
            profile.levelStarValues?.Clear();
            saveProfile(profile);
            Debug.Log("[SaveManager] HOUSEFLOW campaign progression has been reset!");
        }
    }

    public static void ResetHouseflowEconomy()
    {
        ProfileData profile = loadProfile();
        if (profile != null)
        {
            profile.coins = 0;
            profile.gems = 0;
            profile.showcaseAcclaim = 0;
            saveProfile(profile);
            Debug.Log("[SaveManager] HOUSEFLOW economy has been reset!");
        }
    }

    public static void ResetHouseflowAll()
    {
        ResetHouseflowProgression();
        ResetHouseflowEconomy();
        ProfileData profile = loadProfile();
        if (profile != null)
        {
            profile.toolInventory?.Clear();
            profile.unlockedHouseFeatureIDs?.Clear();
            profile.ownedCosmeticIDs?.Clear();
            profile.equippedCosmetics?.Clear();
            profile.noAdsPurchased = false;
            profile.purchasedProductIDs?.Clear();
            profile.purchasedShopItemIDs?.Clear();
            profile.lastVisitorTipClaimUtcTicks = 0;
            profile.lastDailyRewardClaimUtcTicks = 0;
            saveProfile(profile);
            Debug.Log("[SaveManager] All HOUSEFLOW meta, inventory, and economy data has been reset!");
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("HOUSEFLOW/Reset Campaign Progression")]
    public static void ResetHouseflowProgressionEditor()
    {
        ResetHouseflowProgression();
    }

    [UnityEditor.MenuItem("HOUSEFLOW/Reset Economy")]
    public static void ResetHouseflowEconomyEditor()
    {
        ResetHouseflowEconomy();
    }

    [UnityEditor.MenuItem("HOUSEFLOW/Reset ALL Meta & Progression")]
    public static void ResetHouseflowAllEditor()
    {
        ResetHouseflowAll();
    }

    [UnityEditor.MenuItem("Project Echoes/Reset Current Run Data")]
    public static void ResetRunDataEditor()
    {
        if (File.Exists(runPath))
        {
            File.Delete(runPath);
            Debug.Log("[SaveManager] Run data deleted successfully!");
        }
        else
        {
            Debug.Log("[SaveManager] No run data found to delete.");
        }
    }

    [UnityEditor.MenuItem("Project Echoes/Reset ALL Save Data (Profile + Run)")]
    public static void ResetAllSaveDataEditor()
    {
        if (File.Exists(runPath)) File.Delete(runPath);
        if (File.Exists(profilePath)) File.Delete(profilePath);
        Debug.Log("[SaveManager] All save data has been completely reset!");
    }
#endif
}
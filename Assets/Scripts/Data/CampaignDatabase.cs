using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldData
{
    public int worldNumber = 1;
    public string worldName = "World 1";
    [TextArea(2, 3)]
    public string description;

    [Tooltip("Minimum number of completed levels required to unlock this world. 0 = unlocked by default.")]
    [Min(0)]
    public int requiredCompletedLevelsToUnlock = 0;

    [Tooltip("Ordered list of levels in this world.")]
    public LevelData[] levels = new LevelData[0];
}

/// <summary>
/// ScriptableObject defining the data-driven campaign structure across worlds and levels.
/// </summary>
[CreateAssetMenu(menuName = "Game/Campaign/Campaign Database", fileName = "CampaignDatabase")]
public class CampaignDatabase : ScriptableObject
{
    [Tooltip("Configured worlds in this campaign.")]
    [SerializeField] private WorldData[] worlds = new WorldData[0];

    public IReadOnlyList<WorldData> Worlds => worlds;

    public LevelData GetLevelById(string levelId)
    {
        if (string.IsNullOrEmpty(levelId)) return null;

        foreach (var world in worlds)
        {
            if (world.levels == null) continue;
            foreach (var lvl in world.levels)
            {
                if (lvl != null && lvl.LevelId == levelId)
                    return lvl;
            }
        }
        return null;
    }

    public LevelData GetNextLevel(LevelData currentLevel)
    {
        if (currentLevel == null || worlds == null) return null;

        bool foundCurrent = false;
        foreach (var world in worlds)
        {
            if (world.levels == null) continue;
            for (int i = 0; i < world.levels.Length; i++)
            {
                var lvl = world.levels[i];
                if (foundCurrent && lvl != null)
                    return lvl;

                if (lvl == currentLevel)
                    foundCurrent = true;
            }
        }
        return null;
    }

    public LevelData GetFirstLevel()
    {
        if (worlds != null && worlds.Length > 0 && worlds[0].levels != null && worlds[0].levels.Length > 0)
        {
            return worlds[0].levels[0];
        }
        return null;
    }
}

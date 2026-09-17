using UnityEngine;

// BaseLevelGenerator.cs — STUB
// Abstract base class for level generation. Implement your puzzle level logic here.
public abstract class BaseLevelGenerator : MonoBehaviour
{
    public abstract void GenerateMap(int depth);
    public abstract void ClearMap();
    public abstract Transform GetPlayerSpawnPoint();
}

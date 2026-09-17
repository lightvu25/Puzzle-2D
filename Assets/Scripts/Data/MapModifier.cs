// MapModifier.cs — STUB
// Carries a loot/difficulty modifier from a MemoryNodeData into a level.
[System.Serializable]
public class MapModifier
{
    public enum ModifierType
    {
        RelicLootMultiplier,
        EchoLootMultiplier,
        EquipmentLootMultiplier
    }

    public string modifierName;
    public ModifierType type;
    public float value;
}

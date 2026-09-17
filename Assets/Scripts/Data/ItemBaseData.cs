using UnityEngine;

public enum ItemCategory
{
    None,
    Relic,
    Echo,
    Equipment,
    Tool,
    Consumable
}

/// <summary>
/// Base ScriptableObject for any item that can appear in the inventory, tooltip, or loot pool.
/// Extend this class to add item-specific fields.
/// </summary>
public abstract class ItemBaseData : ScriptableObject
{
    public string itemID;
    public string itemName;
    [TextArea] public string description;
    public Sprite itemIcon;
    public ItemCategory Category;
}

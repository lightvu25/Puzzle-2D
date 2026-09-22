using UnityEngine;

namespace HouseFlow.Tools
{
    /// <summary>
    /// Data-driven configuration asset for a Troubleshooter Tool.
    /// </summary>
    [CreateAssetMenu(fileName = "ToolDefinition_New", menuName = "HOUSEFLOW/Tools/Tool Definition", order = 1)]
    public class ToolDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier used by ToolInventory and Shop catalogs.")]
        [SerializeField] private string toolId;

        [Tooltip("GDD Troubleshooter Tool classification.")]
        [SerializeField] private ToolType toolType;

        [Tooltip("Human-readable title displayed in HUD / Shop.")]
        [SerializeField] private string displayName;

        [TextArea(2, 4)]
        [Tooltip("Gameplay description explaining tool function.")]
        [SerializeField] private string description;

        [Tooltip("Icon rendered in HUD slots and inventory menus.")]
        [SerializeField] private Sprite icon;

        [Header("Economy & Pricing")]
        [Tooltip("Cost in soft coins (0 if non-purchasable via coins).")]
        [SerializeField, Min(0)] private int coinPrice = 100;

        [Tooltip("Cost in premium gems (0 if non-purchasable via gems).")]
        [SerializeField, Min(0)] private int gemPrice = 0;

        public string ToolId => toolId;
        public ToolType ToolType => toolType;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public int CoinPrice => coinPrice;
        public int GemPrice => gemPrice;
    }
}

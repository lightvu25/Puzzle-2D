using UnityEngine;

namespace HouseFlow.Cosmetics
{
    /// <summary>
    /// Data-driven definition for a visual-only cosmetic item.
    ///
    /// CRITICAL GDD INTEGRITY RULE:
    /// Fluid skins and hardware finishes must NEVER modify particle density,
    /// mass, drag, cooling rate, or collision radius.
    /// </summary>
    [CreateAssetMenu(fileName = "Cosmetic_New", menuName = "HOUSEFLOW/Cosmetics/Cosmetic Definition", order = 1)]
    public class CosmeticDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier persisted in ProfileData.")]
        [SerializeField] private string cosmeticId;

        [Tooltip("Cosmetic category / equip slot.")]
        [SerializeField] private CosmeticCategory category;

        [Tooltip("Display name shown in inventory and shops.")]
        [SerializeField] private string displayName;

        [TextArea(2, 4)]
        [Tooltip("Lore/aesthetic description.")]
        [SerializeField] private string description;

        [Tooltip("Thumbnail preview sprite.")]
        [SerializeField] private Sprite previewSprite;

        [Header("Visual-Only Styling")]
        [Tooltip("Optional color tint applied to the visual sprite/mesh (physics unaffected).")]
        [SerializeField] private Color colorTint = Color.white;

        [Tooltip("Optional visual material override (e.g. holographic, copper, crystal shader).")]
        [SerializeField] private Material materialOverride;

        [Header("Unlocking & Pricing")]
        [Tooltip("Minimum Showcase Acclaim required to view/purchase in cosmetic catalog.")]
        [SerializeField, Min(0)] private int requiredAcclaim = 0;

        [Tooltip("Soft currency cost (0 if non-purchasable with coins).")]
        [SerializeField, Min(0)] private int coinPrice = 200;

        [Tooltip("Premium currency cost (0 if non-purchasable with gems).")]
        [SerializeField, Min(0)] private int gemPrice = 0;

        public string CosmeticId => cosmeticId;
        public CosmeticCategory Category => category;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite PreviewSprite => previewSprite;
        public Color ColorTint => colorTint;
        public Material MaterialOverride => materialOverride;
        public int RequiredAcclaim => requiredAcclaim;
        public int CoinPrice => coinPrice;
        public int GemPrice => gemPrice;
    }
}

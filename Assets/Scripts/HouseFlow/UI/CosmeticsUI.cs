using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HouseFlow.Cosmetics;
using HouseFlow.Economy;

namespace HouseFlow.UI
{
    /// <summary>
    /// Player-facing Cosmetics screen. Category tabs are generated from the
    /// CosmeticCategory enum; cards are generated from the serialized catalog
    /// (CosmeticDefinition assets). Equip/unequip routes exclusively through
    /// CosmeticEquipService — the UI owns no inventory or equip logic.
    /// </summary>
    public class CosmeticsUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject cosmeticsPanel;
        [SerializeField] private Button backButton;

        [Header("Stats Header")]
        [SerializeField] private Text acclaimText;

        [Header("Categories")]
        [SerializeField] private Transform categoryTabsContainer;
        [SerializeField] private GameObject categoryTabTemplate;

        [Header("Items")]
        [Tooltip("Catalog of CosmeticDefinition assets. Empty until cosmetic content exists.")]
        [SerializeField] private List<CosmeticDefinition> catalog = new List<CosmeticDefinition>();
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject itemCardTemplate;
        [SerializeField] private Text emptyText;
        [SerializeField] private Text feedbackText;

        /// <summary>Fired when the player closes the cosmetics screen (back button).</summary>
        public event Action OnCosmeticsClosed;

        private readonly List<GameObject> spawnedCards = new List<GameObject>();
        private readonly List<GameObject> spawnedTabs = new List<GameObject>();
        private CosmeticInventory inventory;
        private CosmeticEquipService equipService;
        private EconomyManager economy;
        private CosmeticCategory selectedCategory;
        private bool subscribed;

        private void Awake()
        {
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
        }

        private void Start()
        {
            // Panel stays active in the saved scene so Awake/Start execute;
            // hide until the player opens the cosmetics screen.
            if (cosmeticsPanel != null && cosmeticsPanel.activeSelf)
                cosmeticsPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (inventory == null) inventory = CosmeticInventory.Instance ?? FindAnyObjectByType<CosmeticInventory>();
            if (equipService == null) equipService = CosmeticEquipService.Instance ?? FindAnyObjectByType<CosmeticEquipService>();
            if (economy == null) economy = EconomyManager.Instance ?? FindAnyObjectByType<EconomyManager>();
            Subscribe();
            RefreshAll();
        }

        private void OnDisable() => Unsubscribe();
        private void OnDestroy() => Unsubscribe();

        private void Subscribe()
        {
            if (subscribed) return;
            subscribed = true;
            if (equipService != null)
                equipService.OnCosmeticEquipped += HandleCosmeticEquipped;
            if (inventory != null)
                inventory.OnCosmeticGranted += HandleCosmeticGranted;
        }

        private void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;
            if (equipService != null)
                equipService.OnCosmeticEquipped -= HandleCosmeticEquipped;
            if (inventory != null)
                inventory.OnCosmeticGranted -= HandleCosmeticGranted;
        }

        // ─────────────────────────────────────────────────────────────
        //  Open / Close
        // ─────────────────────────────────────────────────────────────

        public void OpenCosmetics()
        {
            if (cosmeticsPanel != null) cosmeticsPanel.SetActive(true);
            // OnEnable performs RefreshAll; no extra work needed here.
        }

        public void CloseCosmetics()
        {
            if (cosmeticsPanel != null) cosmeticsPanel.SetActive(false);
            OnCosmeticsClosed?.Invoke();
        }

        private void OnBackClicked() => CloseCosmetics();

        // ─────────────────────────────────────────────────────────────
        //  Refresh
        // ─────────────────────────────────────────────────────────────

        public void RefreshAll()
        {
            if (acclaimText != null && economy != null)
                acclaimText.text = economy.ShowcaseAcclaim.ToString();
            RebuildCategoryTabs();
            RebuildItemCards();
        }

        private void RebuildCategoryTabs()
        {
            if (categoryTabsContainer == null || categoryTabTemplate == null) return;

            for (int i = categoryTabsContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = categoryTabsContainer.GetChild(i);
                if (child.gameObject == categoryTabTemplate) continue;
                Destroy(child.gameObject);
            }
            spawnedTabs.Clear();

            bool first = true;
            foreach (CosmeticCategory cat in Enum.GetValues(typeof(CosmeticCategory)))
            {
                var tab = Instantiate(categoryTabTemplate, categoryTabsContainer);
                tab.name = $"Tab_{cat}";
                tab.SetActive(true);
                var label = tab.GetComponentInChildren<Text>();
                if (label != null) label.text = cat.ToString();
                var btn = tab.GetComponent<Button>();
                var captured = cat;
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnCategorySelected(captured));
                }
                spawnedTabs.Add(tab);
                if (first) { selectedCategory = cat; first = false; }
            }
        }

        private void OnCategorySelected(CosmeticCategory category)
        {
            selectedCategory = category;
            RebuildItemCards();
        }

        private void RebuildItemCards()
        {
            if (itemsContainer == null || itemCardTemplate == null) return;

            for (int i = itemsContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = itemsContainer.GetChild(i);
                if (child.gameObject == itemCardTemplate) continue;
                Destroy(child.gameObject);
            }
            spawnedCards.Clear();

            int count = 0;
            if (catalog != null)
            {
                foreach (var def in catalog)
                {
                    if (def == null || def.Category != selectedCategory) continue;
                    var card = Instantiate(itemCardTemplate, itemsContainer);
                    card.name = $"CosmeticCard_{def.CosmeticId}";
                    card.SetActive(true);
                    SetupCard(card, def);
                    spawnedCards.Add(card);
                    count++;
                }
            }

            if (emptyText != null)
            {
                emptyText.gameObject.SetActive(count == 0);
                if (count == 0)
                    emptyText.text = $"No items in {selectedCategory} yet.";
            }
        }

        private void SetupCard(GameObject card, CosmeticDefinition def)
        {
            var nameText = FindChildText(card.transform, "NameText");
            var descText = FindChildText(card.transform, "DescText");
            var priceText = FindChildText(card.transform, "PriceText");
            var stateText = FindChildText(card.transform, "StateText");
            var actionButton = card.transform.Find("ActionBtn")?.GetComponent<Button>();
            var actionLabel = actionButton != null ? actionButton.GetComponentInChildren<Text>() : null;
            var preview = card.transform.Find("PreviewImage")?.GetComponent<Image>();

            bool owned = inventory != null && inventory.OwnsCosmetic(def.CosmeticId);
            bool equipped = equipService != null
                && equipService.GetEquippedCosmeticId(def.Category) == def.CosmeticId;

            if (nameText != null) nameText.text = def.DisplayName;
            if (descText != null) descText.text = def.Description;
            if (priceText != null) priceText.text = FormatPrice(def);
            if (preview != null && def.PreviewSprite != null)
            {
                preview.sprite = def.PreviewSprite;
                preview.color = def.ColorTint;
            }
            if (stateText != null)
                stateText.text = equipped ? "EQUIPPED" : owned ? "OWNED" : "LOCKED";

            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
                if (equipped)
                {
                    if (actionLabel != null) actionLabel.text = "UNEQUIP";
                    var cat = def.Category;
                    actionButton.onClick.AddListener(() => OnUnequipClicked(cat));
                }
                else if (owned)
                {
                    if (actionLabel != null) actionLabel.text = "EQUIP";
                    actionButton.onClick.AddListener(() => OnEquipClicked(def));
                }
                else
                {
                    // No cosmetic purchase backend exists; unowned items are display-only.
                    if (actionLabel != null) actionLabel.text = "LOCKED";
                    actionButton.interactable = false;
                }
            }
        }

        private void OnEquipClicked(CosmeticDefinition def)
        {
            if (equipService == null || def == null) return;
            bool ok = equipService.EquipCosmetic(def);
            ShowFeedback(ok ? $"Equipped {def.DisplayName}." : $"Cannot equip {def.DisplayName}.");
            if (ok) RebuildItemCards();
        }

        private void OnUnequipClicked(CosmeticCategory category)
        {
            if (equipService == null) return;
            equipService.UnequipCosmetic(category);
            ShowFeedback("Unequipped.");
            RebuildItemCards();
        }

        private void HandleCosmeticEquipped(CosmeticCategory category, string cosmeticId)
        {
            if (category == selectedCategory) RebuildItemCards();
        }

        private void HandleCosmeticGranted(string cosmeticId) => RebuildItemCards();

        private string FormatPrice(CosmeticDefinition def)
        {
            var parts = new List<string>();
            if (def.CoinPrice > 0) parts.Add($"{def.CoinPrice} Coins");
            if (def.GemPrice > 0) parts.Add($"{def.GemPrice} Gems");
            if (def.RequiredAcclaim > 0) parts.Add($"{def.RequiredAcclaim} Acclaim");
            return parts.Count > 0 ? string.Join(" / ", parts) : "-";
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText != null) feedbackText.text = message;
        }

        private static Text FindChildText(Transform root, string childName)
        {
            var t = root.Find(childName);
            return t != null ? t.GetComponent<Text>() : null;
        }
    }
}

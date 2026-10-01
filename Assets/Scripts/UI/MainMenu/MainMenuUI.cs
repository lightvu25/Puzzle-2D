using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Tomb of the Mask style main menu shell: a full-screen "tap to play" intro,
/// then a persistent menu with a header (level / coins / shards) and a bottom
/// tab bar (Map, Shop, Power-ups, Missions, Settings). The selected tab's
/// button is enlarged. The Map tab auto-opens first.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    private enum Tab { Map, Shop, Powerups, Missions, Settings }

    [Header("System References")]
    [SerializeField] private CampaignMapUI campaignMapUI;
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private DailyRewardUI dailyRewardUI;
    [FormerlySerializedAs("visitorTipUI")]
    [SerializeField] private CoinBonusUI coinBonusUI;
    [SerializeField] private LevelFlowController flowController;

    [Header("Tap To Play")]
    [Tooltip("Full-screen intro panel — any tap dismisses it into the main menu.")]
    [SerializeField] private GameObject tapToPlayPanel;
    [SerializeField] private Button tapToPlayButton;
    [Tooltip("Tap-to-play art slot — assign your title/play image.")]
    [SerializeField] private Image tapToPlayImage;

    [Header("Main Menu Root")]
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Header")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text gemsText;
    [Tooltip("Icon slots — assign coin/shard/level art in the scene.")]
    [SerializeField] private Image coinIconImage;
    [SerializeField] private Image gemIconImage;
    [SerializeField] private Image levelIconImage;

    [Header("Tab Bar")]
    [SerializeField] private Button mapTabButton;
    [SerializeField] private Button shopTabButton;
    [SerializeField] private Button powerupsTabButton;
    [SerializeField] private Button missionsTabButton;
    [SerializeField] private Button settingsTabButton;
    [Tooltip("Tab icon slots — the selected tab's icon scales up.")]
    [SerializeField] private Image mapTabIcon;
    [SerializeField] private Image shopTabIcon;
    [SerializeField] private Image powerupsTabIcon;
    [SerializeField] private Image missionsTabIcon;
    [SerializeField] private Image settingsTabIcon;
    [SerializeField, Min(1f)] private float selectedTabScale = 1.3f;

    [Header("Tab Panels")]
    [SerializeField] private GameObject powerupsPanel;
    [SerializeField] private GameObject missionsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Side Buttons (Map Screen)")]
    [SerializeField] private Button dailyRewardButton;
    [FormerlySerializedAs("visitorButton")]
    [SerializeField] private Button bonusButton;

    [Header("Placeholders")]
    [SerializeField] private GameObject comingSoonModal;
    [SerializeField] private TMP_Text comingSoonText;
    [SerializeField] private Button closeComingSoonButton;

    private Tab currentTab = Tab.Map;

    private void Awake()
    {
        if (flowController == null)
            flowController = FindAnyObjectByType<LevelFlowController>();
        if (campaignMapUI == null)
            campaignMapUI = FindAnyObjectByType<CampaignMapUI>(FindObjectsInactive.Include);
        if (shopUI == null)
            shopUI = FindAnyObjectByType<ShopUI>(FindObjectsInactive.Include);
        if (dailyRewardUI == null)
            dailyRewardUI = FindAnyObjectByType<DailyRewardUI>(FindObjectsInactive.Include);
        if (coinBonusUI == null)
            coinBonusUI = FindAnyObjectByType<CoinBonusUI>(FindObjectsInactive.Include);

        if (tapToPlayButton != null) tapToPlayButton.onClick.AddListener(OnTapToPlay);
        if (mapTabButton != null) mapTabButton.onClick.AddListener(() => SelectTab(Tab.Map));
        if (shopTabButton != null) shopTabButton.onClick.AddListener(() => SelectTab(Tab.Shop));
        if (powerupsTabButton != null) powerupsTabButton.onClick.AddListener(() => SelectTab(Tab.Powerups));
        if (missionsTabButton != null) missionsTabButton.onClick.AddListener(() => SelectTab(Tab.Missions));
        if (settingsTabButton != null) settingsTabButton.onClick.AddListener(() => SelectTab(Tab.Settings));
        if (dailyRewardButton != null) dailyRewardButton.onClick.AddListener(OnDailyRewardClicked);
        if (bonusButton != null) bonusButton.onClick.AddListener(OnBonusClicked);
        if (closeComingSoonButton != null)
            closeComingSoonButton.onClick.AddListener(() => { if (comingSoonModal != null) comingSoonModal.SetActive(false); });

        if (campaignMapUI != null) campaignMapUI.OnMapClosed += HandleMapClosed;
        if (shopUI != null) shopUI.OnShopClosed += HandleShopClosed;
        if (dailyRewardUI != null) dailyRewardUI.OnDailyRewardClosed += HandleSubPanelClosed;
        if (coinBonusUI != null) coinBonusUI.OnCoinBonusClosed += HandleSubPanelClosed;

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoinsChanged += UpdateCoins;
            EconomyManager.Instance.OnGemsChanged += UpdateGems;
        }
    }

    private void OnDestroy()
    {
        if (campaignMapUI != null) campaignMapUI.OnMapClosed -= HandleMapClosed;
        if (shopUI != null) shopUI.OnShopClosed -= HandleShopClosed;
        if (dailyRewardUI != null) dailyRewardUI.OnDailyRewardClosed -= HandleSubPanelClosed;
        if (coinBonusUI != null) coinBonusUI.OnCoinBonusClosed -= HandleSubPanelClosed;

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoinsChanged -= UpdateCoins;
            EconomyManager.Instance.OnGemsChanged -= UpdateGems;
        }
    }

    private void Start()
    {
        if (comingSoonModal != null) comingSoonModal.SetActive(false);

        bool showMenu = flowController == null || flowController.CurrentState == LevelState.Idle;
        if (tapToPlayPanel != null) tapToPlayPanel.SetActive(showMenu);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        RefreshHeader();
    }

    // ─────────────────────────────────────────────────────────────
    //  Tap to play / tab selection
    // ─────────────────────────────────────────────────────────────

    private void OnTapToPlay()
    {
        if (tapToPlayPanel != null) tapToPlayPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        SelectTab(Tab.Map); // Map auto-displays first, per TotM flow.
    }

    private void SelectTab(Tab tab)
    {
        currentTab = tab;

        if (mainMenuPanel != null && !mainMenuPanel.activeSelf)
            mainMenuPanel.SetActive(true);

        SetPanelVisible(Tab.Map, tab == Tab.Map);
        SetPanelVisible(Tab.Shop, tab == Tab.Shop);
        if (powerupsPanel != null) powerupsPanel.SetActive(tab == Tab.Powerups);
        if (missionsPanel != null) missionsPanel.SetActive(tab == Tab.Missions);
        if (settingsPanel != null) settingsPanel.SetActive(tab == Tab.Settings);
        else if (tab == Tab.Settings) ShowComingSoon("Settings coming soon!");

        UpdateTabVisuals();
        RefreshHeader();
    }

    private void SetPanelVisible(Tab tab, bool visible)
    {
        if (tab == Tab.Map && campaignMapUI != null)
        {
            if (visible) campaignMapUI.OpenMap();
            else campaignMapUI.CloseMap();
        }
        else if (tab == Tab.Shop && shopUI != null)
        {
            if (visible) shopUI.OpenShop();
            else shopUI.CloseShop();
        }
    }

    private void UpdateTabVisuals()
    {
        ScaleTab(mapTabButton, currentTab == Tab.Map);
        ScaleTab(shopTabButton, currentTab == Tab.Shop);
        ScaleTab(powerupsTabButton, currentTab == Tab.Powerups);
        ScaleTab(missionsTabButton, currentTab == Tab.Missions);
        ScaleTab(settingsTabButton, currentTab == Tab.Settings);
    }

    private void ScaleTab(Button button, bool selected)
    {
        if (button == null) return;
        float s = selected ? selectedTabScale : 1f;
        button.transform.localScale = new Vector3(s, s, 1f);
    }

    // ─────────────────────────────────────────────────────────────
    //  Header
    // ─────────────────────────────────────────────────────────────

    private void RefreshHeader()
    {
        if (EconomyManager.Instance != null)
        {
            UpdateCoins(EconomyManager.Instance.Coins, 0);
            UpdateGems(EconomyManager.Instance.Gems, 0);
        }

        if (levelText != null && ProgressionManager.Instance != null && ProgressionManager.Instance.Database != null)
        {
            var next = ProgressionManager.Instance.Database.GetNextLevel(null);
            levelText.text = next != null ? next.DisplayName : "";
        }
    }

    private void UpdateCoins(int amount, int delta) { if (coinsText != null) coinsText.text = amount.ToString(); }
    private void UpdateGems(int amount, int delta) { if (gemsText != null) gemsText.text = amount.ToString(); }

    // ─────────────────────────────────────────────────────────────
    //  Side buttons + sub-panel close handling
    // ─────────────────────────────────────────────────────────────

    private void OnDailyRewardClicked()
    {
        if (dailyRewardUI != null) dailyRewardUI.OpenDailyReward();
        else ShowComingSoon("Daily rewards opening soon!");
    }

    private void OnBonusClicked()
    {
        if (coinBonusUI != null) coinBonusUI.OpenCoinBonus();
        else ShowComingSoon("Coin bonus opening soon!");
    }

    private void HandleSubPanelClosed()
    {
        if (mainMenuPanel != null && (flowController == null || flowController.CurrentState == LevelState.Idle))
            mainMenuPanel.SetActive(true);
    }

    private void HandleShopClosed()
    {
        // Shop is a tab — closing it returns focus to the Map tab.
        if (currentTab == Tab.Shop)
            SelectTab(Tab.Map);
        else
            HandleSubPanelClosed();
    }

    private void HandleMapClosed()
    {
        // Map only "closes" when a level starts or another tab takes over;
        // on Idle with no level pending, keep the menu on the Map tab.
        if (flowController != null && flowController.CurrentState == LevelState.Idle && currentTab == Tab.Map)
            SelectTab(Tab.Map);
    }

    private void ShowComingSoon(string message)
    {
        if (comingSoonModal != null)
        {
            comingSoonModal.SetActive(true);
            if (comingSoonText != null) comingSoonText.text = message;
        }
        else
        {
            Debug.Log($"[MainMenuUI] {message}");
        }
    }
}

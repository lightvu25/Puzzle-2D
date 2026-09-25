using UnityEngine;
using UnityEngine.UI;
using HouseFlow.Level;
using HouseFlow.Progression;

namespace HouseFlow.UI
{
    /// <summary>
    /// Handles Title Screen logic and Main Menu navigation.
    /// Defers complex logic to CampaignMapUI and GameplayHUD.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private CampaignMapUI campaignMapUI;
        [SerializeField] private ShopUI shopUI;
        [SerializeField] private HouseUI houseUI;
        [SerializeField] private CosmeticsUI cosmeticsUI;
        [SerializeField] private DailyRewardUI dailyRewardUI;
        [SerializeField] private VisitorTipUI visitorTipUI;
        [SerializeField] private LevelFlowController flowController;
        [SerializeField] private GameObject mainMenuPanel;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button mapButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button houseButton;
        [SerializeField] private Button cosmeticsButton;
        [SerializeField] private Button dailyRewardButton;
        [SerializeField] private Button visitorButton;
        [SerializeField] private Button settingsButton;

        [Header("Placeholders")]
        [SerializeField] private GameObject comingSoonModal;
        [SerializeField] private Text comingSoonText;
        [SerializeField] private Button closeComingSoonButton;

        private void Awake()
        {
            if (flowController == null)
                flowController = FindAnyObjectByType<LevelFlowController>();

            if (campaignMapUI == null)
                campaignMapUI = FindAnyObjectByType<CampaignMapUI>();

            if (shopUI == null)
                shopUI = FindAnyObjectByType<ShopUI>(FindObjectsInactive.Include);

            if (houseUI == null)
                houseUI = FindAnyObjectByType<HouseUI>(FindObjectsInactive.Include);

            if (cosmeticsUI == null)
                cosmeticsUI = FindAnyObjectByType<CosmeticsUI>(FindObjectsInactive.Include);

            if (dailyRewardUI == null)
                dailyRewardUI = FindAnyObjectByType<DailyRewardUI>(FindObjectsInactive.Include);

            if (visitorTipUI == null)
                visitorTipUI = FindAnyObjectByType<VisitorTipUI>(FindObjectsInactive.Include);

            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (mapButton != null) mapButton.onClick.AddListener(OnMapClicked);
            if (shopButton != null) shopButton.onClick.AddListener(OnShopClicked);
            if (houseButton != null) houseButton.onClick.AddListener(OnHouseClicked);
            if (cosmeticsButton != null) cosmeticsButton.onClick.AddListener(OnCosmeticsClicked);
            if (dailyRewardButton != null) dailyRewardButton.onClick.AddListener(OnDailyRewardClicked);
            if (visitorButton != null) visitorButton.onClick.AddListener(OnVisitorClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(() => ShowComingSoon("Settings placeholder"));

            if (closeComingSoonButton != null)
            {
                closeComingSoonButton.onClick.AddListener(() => {
                    if (comingSoonModal != null) comingSoonModal.SetActive(false);
                });
            }

            if (campaignMapUI != null)
            {
                campaignMapUI.OnMapClosed += HandleMapClosed;
            }

            if (shopUI != null)
            {
                shopUI.OnShopClosed += HandleShopClosed;
            }

            if (houseUI != null)
            {
                houseUI.OnHouseClosed += HandleHouseClosed;
            }

            if (cosmeticsUI != null)
            {
                cosmeticsUI.OnCosmeticsClosed += HandleCosmeticsClosed;
            }

            if (dailyRewardUI != null)
            {
                dailyRewardUI.OnDailyRewardClosed += HandleDailyRewardClosed;
            }

            if (visitorTipUI != null)
            {
                visitorTipUI.OnVisitorClosed += HandleVisitorClosed;
            }
        }

        private void OnDestroy()
        {
            if (campaignMapUI != null)
            {
                campaignMapUI.OnMapClosed -= HandleMapClosed;
            }

            if (shopUI != null)
            {
                shopUI.OnShopClosed -= HandleShopClosed;
            }

            if (houseUI != null)
            {
                houseUI.OnHouseClosed -= HandleHouseClosed;
            }

            if (cosmeticsUI != null)
            {
                cosmeticsUI.OnCosmeticsClosed -= HandleCosmeticsClosed;
            }

            if (dailyRewardUI != null)
            {
                dailyRewardUI.OnDailyRewardClosed -= HandleDailyRewardClosed;
            }

            if (visitorTipUI != null)
            {
                visitorTipUI.OnVisitorClosed -= HandleVisitorClosed;
            }
        }

        private void Start()
        {
            // If we are starting from the menu and flow is idle, show main menu.
            if (flowController != null && flowController.CurrentState == LevelState.Idle)
            {
                mainMenuPanel.SetActive(true);
            }
            else
            {
                mainMenuPanel.SetActive(false);
            }

            if (comingSoonModal != null)
                comingSoonModal.SetActive(false);
        }

        private void OnPlayClicked()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (campaignMapUI != null)
            {
                campaignMapUI.OpenMap();
            }
        }

        private void OnMapClicked()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (campaignMapUI != null)
            {
                campaignMapUI.OpenMap();
            }
        }

        private void OnShopClicked()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (shopUI != null)
            {
                shopUI.OpenShop();
            }
            else
            {
                ShowComingSoon("Shop opening soon!");
            }
        }

        private void OnHouseClicked()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (houseUI != null)
            {
                houseUI.OpenHouse();
            }
            else
            {
                ShowComingSoon("House renovations opening soon!");
            }
        }

        private void OnCosmeticsClicked()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (cosmeticsUI != null)
            {
                cosmeticsUI.OpenCosmetics();
            }
            else
            {
                ShowComingSoon("Wardrobe opening soon!");
            }
        }

        private void OnDailyRewardClicked()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (dailyRewardUI != null)
            {
                dailyRewardUI.OpenDailyReward();
            }
            else
            {
                ShowComingSoon("Daily rewards opening soon!");
            }
        }

        private void OnVisitorClicked()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (visitorTipUI != null)
            {
                visitorTipUI.OpenVisitor();
            }
            else
            {
                ShowComingSoon("Visitors arriving soon!");
            }
        }

        private void RestoreMainMenu()
        {
            // If returning to idle state, restore main menu panel
            if (flowController != null && flowController.CurrentState == LevelState.Idle)
            {
                if (mainMenuPanel != null)
                    mainMenuPanel.SetActive(true);
            }
        }

        private void HandleShopClosed() => RestoreMainMenu();
        private void HandleHouseClosed() => RestoreMainMenu();
        private void HandleCosmeticsClosed() => RestoreMainMenu();
        private void HandleDailyRewardClosed() => RestoreMainMenu();
        private void HandleVisitorClosed() => RestoreMainMenu();

        private void HandleMapClosed()
        {
            // If returning to idle state, restore main menu panel
            if (flowController != null && flowController.CurrentState == LevelState.Idle)
            {
                if (mainMenuPanel != null)
                    mainMenuPanel.SetActive(true);
            }
        }

        private void ShowComingSoon(string message)
        {
            if (comingSoonModal != null)
            {
                comingSoonModal.SetActive(true);
                if (comingSoonText != null)
                    comingSoonText.text = message;
            }
            else
            {
                Debug.Log($"[MainMenuUI] {message}");
            }
        }
    }
}

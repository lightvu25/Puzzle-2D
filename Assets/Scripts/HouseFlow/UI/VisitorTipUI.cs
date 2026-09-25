using System;
using UnityEngine;
using UnityEngine.UI;
using HouseFlow.Economy;
using HouseFlow.Meta;
using HouseFlow.Monetization;

namespace HouseFlow.UI
{
    /// <summary>
    /// Visitor Tip screen. Exposes VisitorTipService state: tip amount,
    /// cooldown status, and collection. Optional ad-doubling path is shown
    /// only when AdRewardService reports the placement as available.
    /// All tip/cooldown logic stays in VisitorTipService.
    /// </summary>
    public class VisitorTipUI : MonoBehaviour
    {
        public static VisitorTipUI Instance { get; private set; }

        public event Action OnVisitorClosed;

        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button backButton;
        [SerializeField] private Text titleText;

        [Header("Currency")]
        [SerializeField] private Text coinsText;

        [Header("Tip Display")]
        [SerializeField] private Text tipText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button collectButton;
        [SerializeField] private Text collectButtonLabel;
        [SerializeField] private Button collectAdButton;
        [SerializeField] private Text feedbackText;

        private VisitorTipService tipService;
        private EconomyManager economy;
        private AdRewardService adService;
        private bool subscribed;

        private void Awake()
        {
            Instance = this;
            if (panelRoot == null) panelRoot = gameObject;
            if (backButton != null) backButton.onClick.AddListener(CloseVisitor);
            if (collectButton != null) collectButton.onClick.AddListener(OnCollectClicked);
            if (collectAdButton != null) collectAdButton.onClick.AddListener(OnCollectAdClicked);
        }

        private void Start()
        {
            panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            if (tipService == null) tipService = VisitorTipService.Instance ?? FindAnyObjectByType<VisitorTipService>();
            if (economy == null) economy = EconomyManager.Instance ?? FindAnyObjectByType<EconomyManager>();
            if (adService == null) adService = AdRewardService.Instance ?? FindAnyObjectByType<AdRewardService>();
            Subscribe();
            RefreshAll();
        }

        private void OnDisable() => Unsubscribe();
        private void OnDestroy()
        {
            Unsubscribe();
            if (Instance == this) Instance = null;
        }

        private void Subscribe()
        {
            if (subscribed) return;
            subscribed = true;

            if (tipService != null)
                tipService.OnVisitorTipsClaimed += HandleTipsClaimed;

            if (economy != null)
                economy.OnCoinsChanged += HandleCoinsChanged;
        }

        private void Unsubscribe()
        {
            if (!subscribed) return;
            subscribed = false;

            if (tipService != null)
                tipService.OnVisitorTipsClaimed -= HandleTipsClaimed;

            if (economy != null)
                economy.OnCoinsChanged -= HandleCoinsChanged;
        }

        public void OpenVisitor()
        {
            panelRoot.SetActive(true);
            RefreshAll();
        }

        public void CloseVisitor()
        {
            panelRoot.SetActive(false);
            OnVisitorClosed?.Invoke();
        }

        private void RefreshAll()
        {
            if (coinsText != null && economy != null)
                coinsText.text = economy.Coins.ToString();

            if (tipService == null)
            {
                if (statusText != null) statusText.text = "Visitor tips unavailable.";
                if (collectButton != null) collectButton.interactable = false;
                if (collectAdButton != null) collectAdButton.gameObject.SetActive(false);
                return;
            }

            int tip = tipService.CalculateTipAmount();
            if (tipText != null)
                tipText.text = $"A visitor left a tip: {tip} Coins";

            bool canCollect = tipService.CanCollectTips();
            if (canCollect)
            {
                if (statusText != null) statusText.text = "Tips ready to collect!";
                if (collectButton != null) collectButton.interactable = true;
                if (collectButtonLabel != null) collectButtonLabel.text = "COLLECT";
            }
            else
            {
                var remaining = tipService.GetTimeUntilNextTip();
                if (statusText != null)
                    statusText.text = $"Next tips in {remaining.Hours}h {remaining.Minutes}m";
                if (collectButton != null) collectButton.interactable = false;
                if (collectButtonLabel != null) collectButtonLabel.text = "COLLECTED";
            }

            // Ad-doubling is only offered when the ad service reports the
            // placement as available AND a collection is actually ready.
            if (collectAdButton != null)
            {
                bool adAvailable = canCollect && adService != null
                    && adService.IsAdAvailable(RewardedAdPlacement.VisitorTipMultiplier);
                collectAdButton.gameObject.SetActive(adAvailable);
            }
        }

        private void OnCollectClicked()
        {
            if (tipService == null) return;
            tipService.CollectTips(false, success =>
            {
                if (!success && feedbackText != null)
                    feedbackText.text = "Tips are not ready yet.";
            });
        }

        private void OnCollectAdClicked()
        {
            if (tipService == null) return;
            tipService.CollectTips(true, success =>
            {
                if (!success && feedbackText != null)
                    feedbackText.text = "Tips are not ready yet.";
            });
        }

        private void HandleTipsClaimed(int amount)
        {
            if (feedbackText != null)
                feedbackText.text = $"Collected {amount} Coins!";
            RefreshAll();
        }

        private void HandleCoinsChanged(int oldValue, int newValue)
        {
            if (coinsText != null) coinsText.text = newValue.ToString();
        }
    }
}

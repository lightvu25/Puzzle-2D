using System;
using UnityEngine;
using UnityEngine.UI;
using HouseFlow.Level;
using HouseFlow.Mechanical;

namespace HouseFlow.UI
{
    /// <summary>
    /// First-time onboarding overlay for the tutorial level.
    ///
    /// Observes the real gameplay flow only — it never calls Interact(),
    /// never modifies objectives, and never blocks gameplay input.
    /// All visual elements are raycastTarget=false except the Skip button,
    /// so player taps reach the valve through the normal
    /// GameInput → Physics2D → IInteractable path.
    ///
    /// Steps:
    ///   0 — "Tap the valve to open it." (waits for Valve.IsOpen)
    ///   1 — "Water is flowing — deliver it to the drain." (waits for level completion)
    ///
    /// Completion or Skip persists ProfileData.tutorialCompleted.
    /// The overlay re-arms on restart as long as the flag is unset.
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [Header("Config")]
        [Tooltip("The level that hosts the tutorial (Level 1).")]
        [SerializeField] private LevelData tutorialLevel;

        [Header("UI References")]
        [SerializeField] private GameObject content;
        [SerializeField] private Text instructionText;
        [SerializeField] private Text stepText;
        [SerializeField] private RectTransform highlight;
        [SerializeField] private Button skipButton;

        /// <summary>Fired when the tutorial overlay begins (step 0 shown).</summary>
        public event Action OnTutorialStarted;
        /// <summary>Fired when a tutorial step is satisfied (0 = valve opened, 1 = level completed).</summary>
        public event Action<int> OnTutorialStepCompleted;
        /// <summary>Fired when the player presses SKIP.</summary>
        public event Action OnTutorialSkipped;
        /// <summary>Fired when the tutorial finishes via normal level completion.</summary>
        public event Action OnTutorialCompleted;

        private LevelFlowController flowController;
        private Canvas parentCanvas;
        private Valve watchedValve;
        private int step = -1; // -1 = inactive
        private bool subscribed;

        private void Awake()
        {
            if (flowController == null)
                flowController = FindAnyObjectByType<LevelFlowController>();
            parentCanvas = GetComponentInParent<Canvas>();
            if (skipButton != null)
                skipButton.onClick.AddListener(OnSkipClicked);
        }

        private void Start()
        {
            Subscribe();
            HideOverlay();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (subscribed || flowController == null) return;
            subscribed = true;
            flowController.OnLevelStarted += HandleLevelStarted;
            flowController.OnLevelCompleted += HandleLevelCompleted;
            flowController.OnStateChanged += HandleStateChanged;
            flowController.OnReturnToMapRequested += HandleReturnToMap;
        }

        private void Unsubscribe()
        {
            if (!subscribed || flowController == null) return;
            subscribed = false;
            flowController.OnLevelStarted -= HandleLevelStarted;
            flowController.OnLevelCompleted -= HandleLevelCompleted;
            flowController.OnStateChanged -= HandleStateChanged;
            flowController.OnReturnToMapRequested -= HandleReturnToMap;
        }

        private void Update()
        {
            if (step != 0 || watchedValve == null) return;

            // Player performed the real interaction — advance.
            if (watchedValve.IsOpen)
            {
                OnTutorialStepCompleted?.Invoke(0);
                SetStep(1);
                return;
            }

            PositionHighlight();
        }

        // ─────────────────────────────────────────────────────────────
        //  Tutorial lifecycle
        // ─────────────────────────────────────────────────────────────

        private void TryBegin()
        {
            if (tutorialLevel == null || flowController == null) return;
            if (flowController.CurrentLevelData != tutorialLevel) return;

            if (IsTutorialCompleted())
            {
                EndTutorial();
                return;
            }

            // Restart resets the valve to closed, so re-arm at step 0.
            var root = flowController.CurrentRoot;
            watchedValve = (root != null && root.Valves != null && root.Valves.Length > 0)
                ? root.Valves[0]
                : null;

            SetStep(0);
        }

        private void SetStep(int newStep)
        {
            bool wasInactive = step < 0;
            step = newStep;

            if (content != null) content.SetActive(true);
            if (newStep == 0 && wasInactive) OnTutorialStarted?.Invoke();

            if (instructionText != null)
            {
                instructionText.text = newStep == 0
                    ? "Tap the valve to open it."
                    : "Water is flowing — deliver it to the drain!";
            }

            if (stepText != null)
                stepText.text = $"{newStep + 1} / 2";

            if (highlight != null)
                highlight.gameObject.SetActive(newStep == 0 && watchedValve != null);

            if (newStep == 0)
                PositionHighlight();
        }

        private void CompleteAndHide(bool skipped)
        {
            if (skipped)
            {
                OnTutorialSkipped?.Invoke();
            }
            else
            {
                OnTutorialStepCompleted?.Invoke(1);
                OnTutorialCompleted?.Invoke();
            }
            MarkTutorialCompleted();
            EndTutorial();
        }

        private void EndTutorial()
        {
            step = -1;
            watchedValve = null;
            HideOverlay();
        }

        private void HideOverlay()
        {
            if (content != null) content.SetActive(false);
            if (highlight != null) highlight.gameObject.SetActive(false);
        }

        private void PositionHighlight()
        {
            if (highlight == null || watchedValve == null || parentCanvas == null) return;

            var cam = Camera.main;
            if (cam == null) return;

            Vector2 screenPos = cam.WorldToScreenPoint(watchedValve.transform.position);
            Vector2 localPos;
            var canvasCam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform, screenPos, canvasCam, out localPos))
            {
                highlight.anchoredPosition = localPos;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Persistence
        // ─────────────────────────────────────────────────────────────

        private static bool IsTutorialCompleted()
        {
            var profile = SaveManager.loadProfile();
            return profile != null && profile.tutorialCompleted;
        }

        private static void MarkTutorialCompleted()
        {
            // Load fresh from disk — never write through a stale cached profile.
            var profile = SaveManager.loadProfile() ?? new ProfileData();
            profile.tutorialCompleted = true;
            SaveManager.saveProfile(profile);
        }

        // ─────────────────────────────────────────────────────────────
        //  Flow event handlers
        // ─────────────────────────────────────────────────────────────

        private void HandleLevelStarted() => TryBegin();

        private void HandleStateChanged(LevelState state)
        {
            if (state == LevelState.Playing)
            {
                // Covers RestartLevel — OnLevelStarted does not fire on reset.
                TryBegin();
            }
            else if (state == LevelState.Idle)
            {
                // Level unloaded / returned to map — leave flag untouched.
                EndTutorial();
            }
        }

        private void HandleLevelCompleted()
        {
            // Normal completion of the tutorial level ends onboarding.
            if (step >= 0)
                CompleteAndHide(false);
        }

        private void HandleReturnToMap() => EndTutorial();

        private void OnSkipClicked() => CompleteAndHide(true);
    }
}

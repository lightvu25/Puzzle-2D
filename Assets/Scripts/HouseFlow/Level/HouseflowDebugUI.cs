using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using HouseFlow.Objective;
using HouseFlow.Fluid;

namespace HouseFlow.Level
{
    /// <summary>
    /// Lightweight development-only debug overlay for HOUSEFLOW.
    ///
    /// Displays: level ID, state, active/pooled particles, objective progress, FPS.
    /// Toggled with F1 or a UI button.
    ///
    /// This component is compiled in all configurations but the panel is only
    /// shown in the Editor or Development Builds (see DebugPanel.SetActive logic).
    ///
    /// Assign the Panel and Text references in the Inspector.
    /// </summary>
    public class HouseflowDebugUI : MonoBehaviour, IUIPanel
    {
        // ─────────────────────────────────────────────────────────────
        //  Inspector References
        // ─────────────────────────────────────────────────────────────

        [Header("Panel")]
        [Tooltip("Root UI panel to show/hide.")]
        [SerializeField] private GameObject debugPanel;

        [Header("Text Fields")]
        [SerializeField] private Text levelIdText;
        [SerializeField] private Text stateText;
        [SerializeField] private Text activeParticlesText;
        [SerializeField] private Text pooledParticlesText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text fpsText;

        [Header("System References")]
        [Tooltip("The LevelFlowController to read state from.")]
        [SerializeField] private LevelFlowController flowController;

        [Tooltip("The ObjectiveSystem to read progress from.")]
        [SerializeField] private ObjectiveSystem objectiveSystem;

        [Header("Toggle Key")]
        [Tooltip("Keyboard key to toggle the debug panel (Editor + Development Builds only).")]
        [SerializeField] private Key toggleKey = Key.F1;

        // ─────────────────────────────────────────────────────────────
        //  FPS Tracking
        // ─────────────────────────────────────────────────────────────

        private float fpsTimer;
        private int   fpsCount;
        private float lastFps;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            // Hide completely in release builds
            if (debugPanel != null)
                debugPanel.SetActive(false);
            enabled = false;
            return;
#endif
            if (debugPanel != null)
                debugPanel.SetActive(false); // Start hidden; toggle with F1
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Toggle
            if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
                TogglePanel();

            if (debugPanel != null && !debugPanel.activeSelf) return;

            UpdateFps();
            RefreshDisplay();
#endif
        }

        // ─────────────────────────────────────────────────────────────
        //  IUIPanel Implementation
        // ─────────────────────────────────────────────────────────────

        public void Show()
        {
            if (debugPanel != null) debugPanel.SetActive(true);
        }

        public void Hide()
        {
            if (debugPanel != null) debugPanel.SetActive(false);
        }

        public void TogglePanel()
        {
            if (debugPanel != null)
                debugPanel.SetActive(!debugPanel.activeSelf);
        }

        // ─────────────────────────────────────────────────────────────
        //  Internal Refresh
        // ─────────────────────────────────────────────────────────────

        private void RefreshDisplay()
        {
            if (flowController == null) return;

            LevelRoot root = flowController.CurrentRoot;
            LevelData data = flowController.CurrentLevelData;

            SetText(levelIdText, $"Level: {data?.LevelId ?? "—"}");
            SetText(stateText,   $"State: {flowController.CurrentState}");

            var activeParticles = FindObjectsByType<FluidParticle>(FindObjectsSortMode.None);
            int activeCount = 0;
            foreach (var p in activeParticles) if (p.gameObject.activeInHierarchy) activeCount++;
            
            SetText(activeParticlesText, $"Active Particles: {activeCount}");
            SetText(pooledParticlesText, $"Pooled Particles: (Managed)");

            ObjectiveBase obj = objectiveSystem?.PrimaryObjective;
            if (obj is DeliverFluidObjective deliver)
                SetText(progressText, $"Progress: {deliver.CurrentCount} / {deliver.RequiredCount}");
            else
                SetText(progressText, $"Progress: {(obj != null ? $"{obj.Progress:P0}" : "—")}");

            SetText(fpsText, $"FPS: {lastFps:F0}");
        }

        private void UpdateFps()
        {
            fpsTimer += Time.unscaledDeltaTime;
            fpsCount++;
            if (fpsTimer >= 0.5f)
            {
                lastFps   = fpsCount / fpsTimer;
                fpsTimer  = 0f;
                fpsCount  = 0;
            }
        }

        private static void SetText(Text textComp, string value)
        {
            if (textComp != null) textComp.text = value;
        }
    }
}

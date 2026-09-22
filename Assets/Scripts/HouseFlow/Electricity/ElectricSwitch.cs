using System;
using UnityEngine;
using HouseFlow.Mechanical;

namespace HouseFlow.Electricity
{
    /// <summary>
    /// Player-tappable manual knife switch or toggle switch for electrical circuits.
    ///
    /// GDD Reference: Section 6.2 ("Manual knife switches"), Section 9.3 (World 2).
    /// </summary>
    public class ElectricSwitch : MonoBehaviour, IInteractable
    {
        [Header("Switch Configuration")]
        [Tooltip("Whether the switch starts in the closed (conducting) position.")]
        [SerializeField] private bool startsClosed = false;

        [Tooltip("The upstream power source feeding this switch.")]
        [SerializeField] private PowerSource upstreamPowerSource;

        [Header("Animation / Visuals")]
        [Tooltip("Transform representing the physical knife blade or toggle lever.")]
        [SerializeField] private Transform leverVisual;

        [Tooltip("Local Z rotation when the switch is open (disconnected).")]
        [SerializeField] private float openRotationZ = -45f;

        [Tooltip("Local Z rotation when the switch is closed (conducting).")]
        [SerializeField] private float closedRotationZ = 0f;

        [Header("Interaction")]
        [Tooltip("Whether the player can tap to toggle this switch.")]
        [SerializeField] private bool interactable = true;

        // ─────────────────────────────────────────────────────────────
        //  Events
        // ─────────────────────────────────────────────────────────────

        /// <summary>Fired when switch state toggles or upstream power changes.</summary>
        public event Action OnCircuitChanged;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private bool isClosed;

        // ─────────────────────────────────────────────────────────────
        //  Public Properties
        // ─────────────────────────────────────────────────────────────

        public bool IsClosed => isClosed;

        /// <summary>True if switch is physically closed AND receives upstream power.</summary>
        public bool IsEnergized => isClosed && (upstreamPowerSource == null || upstreamPowerSource.IsEnergized);

        public bool IsInteractable => interactable;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            isClosed = startsClosed;

            if (upstreamPowerSource != null)
            {
                upstreamPowerSource.OnPowerChanged += HandleUpstreamPowerChanged;
            }

            ApplyVisuals(immediate: true);
        }

        private void OnDestroy()
        {
            if (upstreamPowerSource != null)
            {
                upstreamPowerSource.OnPowerChanged -= HandleUpstreamPowerChanged;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  IInteractable
        // ─────────────────────────────────────────────────────────────

        public void Interact()
        {
            if (!interactable) return;
            Toggle();
        }

        public void Toggle()
        {
            isClosed = !isClosed;
            ApplyVisuals(immediate: false);
            OnCircuitChanged?.Invoke();
        }

        public void SetClosed(bool closed)
        {
            if (isClosed == closed) return;
            isClosed = closed;
            ApplyVisuals(immediate: false);
            OnCircuitChanged?.Invoke();
        }

        // ─────────────────────────────────────────────────────────────
        //  Internal Handlers
        // ─────────────────────────────────────────────────────────────

        private void HandleUpstreamPowerChanged()
        {
            OnCircuitChanged?.Invoke();
        }

        private void ApplyVisuals(bool immediate)
        {
            if (leverVisual == null) return;

            float targetZ = isClosed ? closedRotationZ : openRotationZ;
            Vector3 euler = leverVisual.localEulerAngles;
            leverVisual.localEulerAngles = new Vector3(euler.x, euler.y, targetZ);
        }

        // ─────────────────────────────────────────────────────────────
        //  Reset
        // ─────────────────────────────────────────────────────────────

        /// <summary>Restores switch to its initial state. Called by LevelRoot.ResetAll().</summary>
        public void ResetSwitch()
        {
            isClosed = startsClosed;
            ApplyVisuals(immediate: true);
            OnCircuitChanged?.Invoke();
        }
    }
}

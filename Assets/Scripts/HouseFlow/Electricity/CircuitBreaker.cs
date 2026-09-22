using System;
using UnityEngine;
using HouseFlow.Mechanical;
using HouseFlow.Fluid;

namespace HouseFlow.Electricity
{
    /// <summary>
    /// Master circuit breaker or fuse box that protects a circuit from overloads and water floods.
    ///
    /// GDD Reference: Section 6.2, Section 9.3 ("circuit overloads trip master breakers; flooded switchboards cause failures").
    /// </summary>
    public class CircuitBreaker : MonoBehaviour, IInteractable
    {
        [Header("Breaker Settings")]
        [Tooltip("Whether the breaker starts tripped (open/no power).")]
        [SerializeField] private bool startsTripped = false;

        [Tooltip("Upstream power source or switch feeding this breaker.")]
        [SerializeField] private PowerSource upstreamPowerSource;

        [Header("Hazard Detection")]
        [Tooltip("If true, water particles entering this breaker's trigger cause an immediate short-circuit trip.")]
        [SerializeField] private bool tripOnWaterContact = true;

        [Header("Interaction")]
        [Tooltip("Whether the player can tap to manually reset the breaker.")]
        [SerializeField] private bool interactable = true;

        [Header("Visuals")]
        [Tooltip("Indicator light or switch visual that toggles color/state when tripped.")]
        [SerializeField] private SpriteRenderer statusIndicator;
        [SerializeField] private Color normalColor = Color.green;
        [SerializeField] private Color trippedColor = Color.red;

        // ─────────────────────────────────────────────────────────────
        //  Events
        // ─────────────────────────────────────────────────────────────

        public event Action OnBreakerTripped;
        public event Action OnCircuitChanged;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private bool isTripped;

        // ─────────────────────────────────────────────────────────────
        //  Public Properties
        // ─────────────────────────────────────────────────────────────

        public bool IsTripped => isTripped;

        /// <summary>True if not tripped AND upstream power is active.</summary>
        public bool IsEnergized => !isTripped && (upstreamPowerSource == null || upstreamPowerSource.IsEnergized);

        public bool IsInteractable => interactable;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            isTripped = startsTripped;

            if (upstreamPowerSource != null)
            {
                upstreamPowerSource.OnPowerChanged += HandleUpstreamChanged;
            }

            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (upstreamPowerSource != null)
            {
                upstreamPowerSource.OnPowerChanged -= HandleUpstreamChanged;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTripped || !tripOnWaterContact) return;

            FluidParticle particle = other.GetComponent<FluidParticle>();
            if (particle != null && particle.FluidType == FluidType.Water)
            {
                Trip();
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  IInteractable
        // ─────────────────────────────────────────────────────────────

        public void Interact()
        {
            if (!interactable) return;

            if (isTripped)
            {
                ResetBreakerSwitch();
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────────

        public void Trip()
        {
            if (isTripped) return;

            isTripped = true;
            UpdateVisuals();
            OnBreakerTripped?.Invoke();
            OnCircuitChanged?.Invoke();
            Debug.Log($"[CircuitBreaker] '{name}' tripped!");
        }

        public void ResetBreakerSwitch()
        {
            if (!isTripped) return;

            isTripped = false;
            UpdateVisuals();
            OnCircuitChanged?.Invoke();
            Debug.Log($"[CircuitBreaker] '{name}' reset to normal.");
        }

        // ─────────────────────────────────────────────────────────────
        //  Reset
        // ─────────────────────────────────────────────────────────────

        public void ResetBreaker()
        {
            isTripped = startsTripped;
            UpdateVisuals();
            OnCircuitChanged?.Invoke();
        }

        private void HandleUpstreamChanged()
        {
            OnCircuitChanged?.Invoke();
        }

        private void UpdateVisuals()
        {
            if (statusIndicator != null)
            {
                statusIndicator.color = isTripped ? trippedColor : normalColor;
            }
        }
    }
}

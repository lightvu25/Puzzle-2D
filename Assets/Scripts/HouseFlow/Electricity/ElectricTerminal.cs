using System;
using System.Collections.Generic;
using UnityEngine;
using HouseFlow.Fluid;

namespace HouseFlow.Electricity
{
    /// <summary>
    /// Contact electrode terminal for electrical wiring and fluid conductivity bridging.
    ///
    /// GDD Reference: Section 6.2 ("contact terminals"), Section 9.3 (World 2 Archetype:
    /// "water stream pools between two open electrode terminals to complete the circuit").
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ElectricTerminal : MonoBehaviour
    {
        [Header("Terminal Role")]
        [Tooltip("If true, this terminal acts as a power emitter (e.g. connected to a battery). " +
                 "If false, this terminal is a receiver waiting to be energized by fluid bridging.")]
        [SerializeField] private bool isEmitter = false;

        [Tooltip("The upstream source supplying electricity to this terminal (if an emitter).")]
        [SerializeField] private PowerSource upstreamPowerSource;

        [Tooltip("Optional switch controlling power to this terminal.")]
        [SerializeField] private ElectricSwitch upstreamSwitch;

        [Tooltip("Optional paired terminal for fluid bridge completion checks.")]
        [SerializeField] private ElectricTerminal pairedTerminal;

        [Header("Electrification Settings")]
        [Tooltip("Whether to electrify non-electrified water particles that touch this energized terminal.")]
        [SerializeField] private bool electrifyTouchingFluid = true;

        [Header("Visuals")]
        [Tooltip("Visual indicator or terminal spark renderer.")]
        [SerializeField] private SpriteRenderer terminalRenderer;
        [SerializeField] private Color energizedColor = new Color(0.3f, 0.9f, 1f, 1f);
        [SerializeField] private Color unenergizedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        // ─────────────────────────────────────────────────────────────
        //  Events
        // ─────────────────────────────────────────────────────────────

        public event Action OnTerminalStateChanged;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private readonly HashSet<FluidParticle> activeTouchingParticles = new HashSet<FluidParticle>();
        private bool isBridged;

        // ─────────────────────────────────────────────────────────────
        //  Public Properties
        // ─────────────────────────────────────────────────────────────

        public bool IsEmitter => isEmitter;

        /// <summary>
        /// True if this terminal has active electricity flowing through it.
        /// Emitter: depends on upstream power and switch.
        /// Receiver: depends on receiving electrified fluid or being bridged.
        /// </summary>
        public bool IsEnergized
        {
            get
            {
                if (isEmitter)
                {
                    bool powerOn = upstreamPowerSource == null || upstreamPowerSource.IsEnergized;
                    bool switchOn = upstreamSwitch == null || upstreamSwitch.IsEnergized;
                    return powerOn && switchOn;
                }

                return isBridged;
            }
        }

        public bool HasFluidContact => activeTouchingParticles.Count > 0;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            if (upstreamPowerSource != null)
                upstreamPowerSource.OnPowerChanged += HandleUpstreamChanged;

            if (upstreamSwitch != null)
                upstreamSwitch.OnCircuitChanged += HandleUpstreamChanged;

            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (upstreamPowerSource != null)
                upstreamPowerSource.OnPowerChanged -= HandleUpstreamChanged;

            if (upstreamSwitch != null)
                upstreamSwitch.OnCircuitChanged -= HandleUpstreamChanged;
        }

        // ─────────────────────────────────────────────────────────────
        //  Trigger Interactions
        // ─────────────────────────────────────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            FluidParticle particle = other.GetComponent<FluidParticle>();
            if (particle == null || particle.FluidType != FluidType.Water) return;

            activeTouchingParticles.Add(particle);
            EvaluateFluidState(particle);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            FluidParticle particle = other.GetComponent<FluidParticle>();
            if (particle == null || particle.FluidType != FluidType.Water) return;

            EvaluateFluidState(particle);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            FluidParticle particle = other.GetComponent<FluidParticle>();
            if (particle == null) return;

            activeTouchingParticles.Remove(particle);
            RecheckBridgingState();
        }

        private void EvaluateFluidState(FluidParticle particle)
        {
            if (isEmitter && IsEnergized)
            {
                // Energize the fluid particle
                if (electrifyTouchingFluid && !particle.IsElectrified)
                {
                    particle.SetElectrified(true);
                }
            }
            else if (!isEmitter)
            {
                // Check if the fluid is electrified or if paired terminal is active
                bool shouldBridge = particle.IsElectrified;

                if (!shouldBridge && pairedTerminal != null && pairedTerminal.IsEnergized && pairedTerminal.HasFluidContact)
                {
                    // Bridging: Water connects both terminals
                    shouldBridge = true;
                }

                if (shouldBridge != isBridged)
                {
                    isBridged = shouldBridge;
                    UpdateVisuals();
                    OnTerminalStateChanged?.Invoke();
                }
            }
        }

        private void RecheckBridgingState()
        {
            if (isEmitter) return;

            bool hadPower = isBridged;
            bool stillHasElectrified = false;

            activeTouchingParticles.RemoveWhere(p => p == null || !p.gameObject.activeInHierarchy);

            foreach (var p in activeTouchingParticles)
            {
                if (p.IsElectrified)
                {
                    stillHasElectrified = true;
                    break;
                }
            }

            if (!stillHasElectrified && pairedTerminal != null && pairedTerminal.IsEnergized && pairedTerminal.HasFluidContact && activeTouchingParticles.Count > 0)
            {
                stillHasElectrified = true;
            }

            isBridged = stillHasElectrified;

            if (hadPower != isBridged)
            {
                UpdateVisuals();
                OnTerminalStateChanged?.Invoke();
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Internal Handlers
        // ─────────────────────────────────────────────────────────────

        private void HandleUpstreamChanged()
        {
            UpdateVisuals();
            OnTerminalStateChanged?.Invoke();
        }

        private void UpdateVisuals()
        {
            if (terminalRenderer != null)
            {
                terminalRenderer.color = IsEnergized ? energizedColor : unenergizedColor;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Reset
        // ─────────────────────────────────────────────────────────────

        public void ResetTerminal()
        {
            activeTouchingParticles.Clear();
            isBridged = false;
            UpdateVisuals();
            OnTerminalStateChanged?.Invoke();
        }
    }
}

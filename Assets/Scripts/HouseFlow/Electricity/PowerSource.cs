using System;
using UnityEngine;

namespace HouseFlow.Electricity
{
    /// <summary>
    /// Represents an electrical energy source (battery, wall junction, or generator).
    /// Acts as the root of an electrical circuit.
    ///
    /// GDD Reference: Section 6.2 ("Dry-cell batteries, generators"), Section 9.3 (World 2).
    /// </summary>
    public class PowerSource : MonoBehaviour
    {
        [Header("Power Settings")]
        [Tooltip("Whether this power source starts energized at level start.")]
        [SerializeField] private bool startsEnergized = true;

        [Tooltip("Voltage output of this power source. GDD standard baseline is 12V.")]
        [SerializeField, Min(0f)] private float voltage = 12f;

        // ─────────────────────────────────────────────────────────────
        //  Events
        // ─────────────────────────────────────────────────────────────

        /// <summary>Fired whenever the power state (on/off) changes.</summary>
        public event Action OnPowerChanged;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private bool isEnergized;

        // ─────────────────────────────────────────────────────────────
        //  Public Properties
        // ─────────────────────────────────────────────────────────────

        public bool IsEnergized => isEnergized;
        public float Voltage   => isEnergized ? voltage : 0f;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            isEnergized = startsEnergized;
        }

        // ─────────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────────

        public void SetEnergized(bool energized)
        {
            if (isEnergized == energized) return;

            isEnergized = energized;
            OnPowerChanged?.Invoke();
        }

        public void Toggle()
        {
            SetEnergized(!isEnergized);
        }

        /// <summary>Restores power source to its initial state. Called by LevelRoot.ResetAll().</summary>
        public void ResetSource()
        {
            SetEnergized(startsEnergized);
        }
    }
}

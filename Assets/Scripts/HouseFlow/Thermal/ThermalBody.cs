using System;
using UnityEngine;
using UnityEngine.Events;

namespace HouseFlow.Thermal
{
    /// <summary>
    /// Represents the thermal state of a GameObject.
    /// Handles receiving heat, tracking temperature, and ambient cooling.
    /// 
    /// PERFORMANCE DESIGN:
    /// This component disables its own Update() loop when it reaches ambient temperature,
    /// ensuring zero CPU overhead for hundreds of unheated water particles.
    /// </summary>
    [DisallowMultipleComponent]
    public class ThermalBody : MonoBehaviour, IThermalReceiver
    {
        // ─────────────────────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────────────────────

        [Header("Thermal Properties")]
        [Tooltip("Multiplier applied to the base cooling rate. 1.0 = normal cooling, 0.5 = cools twice as slowly.")]
        [Min(0f)]
        [SerializeField] private float coolingRateMultiplier = 1.0f;

        [Header("Events (Optional)")]
        [Tooltip("Fired when temperature changes. Highly recommended to leave empty for bulk fluid particles to save performance.")]
        public UnityEvent<float> onTemperatureChanged;

        /// <summary>
        /// Fast C# event for code-based subscriptions (e.g., FluidParticle listening for boiling).
        /// Has zero overhead compared to UnityEvents.
        /// </summary>
        public event Action<float> OnTemperatureChangedFast;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private float currentTemperature;
        private float ambientTemperature;
        private float baseCoolingRate;
        private bool  isInitialized;

        // ─────────────────────────────────────────────────────────────
        //  IThermalReceiver
        // ─────────────────────────────────────────────────────────────

        public float Temperature => currentTemperature;

        public void ApplyHeat(float amount)
        {
            if (!isInitialized) return;

            float previous = currentTemperature;
            currentTemperature += amount;

            // If we just heated up above ambient, wake up the cooling loop!
            if (currentTemperature > ambientTemperature && !enabled)
            {
                enabled = true;
            }

            OnTemperatureChangedFast?.Invoke(currentTemperature);

            // Only fire events if someone is actually listening, to save CPU on fluids
            if (onTemperatureChanged != null && onTemperatureChanged.GetPersistentEventCount() > 0)
            {
                onTemperatureChanged.Invoke(currentTemperature);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes or resets the thermal state.
        /// Called by FluidParticle.Spawn() or LevelRoot for static bodies.
        /// </summary>
        public void Initialize(float ambientTemp, float baseCoolRate, float startingTemp = -1f)
        {
            ambientTemperature = ambientTemp;
            baseCoolingRate = baseCoolRate;

            currentTemperature = (startingTemp >= 0f) ? startingTemp : ambientTemperature;
            isInitialized = true;

            // If we start hot, we need to cool down. If we start at ambient, sleep.
            enabled = (currentTemperature > ambientTemperature);
        }

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Update()
        {
            if (!isInitialized) return;

            // Ambient cooling
            if (currentTemperature > ambientTemperature)
            {
                float coolingDrop = baseCoolingRate * coolingRateMultiplier * Time.deltaTime;
                currentTemperature -= coolingDrop;

                if (currentTemperature <= ambientTemperature)
                {
                    currentTemperature = ambientTemperature;
                    enabled = false; // Sleep to save CPU
                }

                OnTemperatureChangedFast?.Invoke(currentTemperature);

                if (onTemperatureChanged != null && onTemperatureChanged.GetPersistentEventCount() > 0)
                {
                    onTemperatureChanged.Invoke(currentTemperature);
                }
            }
        }
    }
}

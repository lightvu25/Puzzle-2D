using System;
using UnityEngine;
using HouseFlow.Fluid;

namespace HouseFlow.Electricity
{
    /// <summary>
    /// Motorized pump or electrical actuator powered by circuit energy.
    ///
    /// GDD Reference: Section 6.2 ("electric motors"), Section 7 ("starts motorized fluid pumps"),
    /// Section 9.3 ("conductive fluid completes the circuit, powering the motorized pump to drain an isolated reservoir").
    /// </summary>
    public class ElectricPump : MonoBehaviour
    {
        [Header("Electrical Supply")]
        [Tooltip("Power source supplying this pump (optional).")]
        [SerializeField] private PowerSource powerSource;

        [Tooltip("Switch controlling this pump (optional).")]
        [SerializeField] private ElectricSwitch controlSwitch;

        [Tooltip("Conductive terminal/electrode feeding this pump (optional).")]
        [SerializeField] private ElectricTerminal feedTerminal;

        [Header("Pump Mechanics")]
        [Tooltip("Target WaterSource that will start/stop emitting when this pump is powered.")]
        [SerializeField] private WaterSource controlledSource;

        [Tooltip("Optional suction or discharge trigger zone that applies directional force to fluid particles.")]
        [SerializeField] private Collider2D suctionCollider;

        [Tooltip("Impulse force applied to fluid particles in the suction zone.")]
        [SerializeField] private float pumpForce = 5f;

        [Tooltip("Direction particles are moved (local space).")]
        [SerializeField] private Vector2 pumpDirection = Vector2.up;

        [Header("Visuals")]
        [Tooltip("Indicator or motor visual that rotates/glows when active.")]
        [SerializeField] private Transform motorVisual;
        [SerializeField] private float rotationSpeed = 360f;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private bool isRunning;

        // ─────────────────────────────────────────────────────────────
        //  Public Properties
        // ─────────────────────────────────────────────────────────────

        public bool IsRunning => isRunning;

        public bool HasPower
        {
            get
            {
                if (feedTerminal != null) return feedTerminal.IsEnergized;
                if (controlSwitch != null) return controlSwitch.IsEnergized;
                if (powerSource != null) return powerSource.IsEnergized;
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (powerSource != null)
                powerSource.OnPowerChanged += EvaluatePower;

            if (controlSwitch != null)
                controlSwitch.OnCircuitChanged += EvaluatePower;

            if (feedTerminal != null)
                feedTerminal.OnTerminalStateChanged += EvaluatePower;

            if (suctionCollider != null)
                suctionCollider.isTrigger = true;

            EvaluatePower();
        }

        private void OnDestroy()
        {
            if (powerSource != null)
                powerSource.OnPowerChanged -= EvaluatePower;

            if (controlSwitch != null)
                controlSwitch.OnCircuitChanged -= EvaluatePower;

            if (feedTerminal != null)
                feedTerminal.OnTerminalStateChanged -= EvaluatePower;
        }

        private void Update()
        {
            if (!isRunning) return;

            if (motorVisual != null)
            {
                motorVisual.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!isRunning) return;

            FluidParticle particle = other.GetComponent<FluidParticle>();
            if (particle != null && particle.Rigidbody != null)
            {
                Vector2 worldDir = transform.TransformDirection(pumpDirection).normalized;
                particle.Rigidbody.AddForce(worldDir * pumpForce * Time.fixedDeltaTime, ForceMode2D.Force);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Power Logic
        // ─────────────────────────────────────────────────────────────

        private void EvaluatePower()
        {
            bool powered = HasPower;
            if (isRunning == powered) return;

            isRunning = powered;

            if (controlledSource != null)
            {
                controlledSource.SetEmissionEnabled(isRunning);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Reset
        // ─────────────────────────────────────────────────────────────

        public void ResetPump()
        {
            isRunning = false;
            EvaluatePower();
        }
    }
}

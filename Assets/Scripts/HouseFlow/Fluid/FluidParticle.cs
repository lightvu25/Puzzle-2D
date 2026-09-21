using UnityEngine;
using HouseFlow.Level;
using HouseFlow.Thermal;

namespace HouseFlow.Fluid
{
    /// <summary>
    /// Represents a single fluid particle in the HOUSEFLOW physics simulation.
    ///
    /// Design Principles:
    ///   - Simulation is completely independent from rendering.
    ///   - State is reset by the pool, not by Destroy/Instantiate.
    ///   - Future states (thermal, charged) can be added here without restructuring the pool.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class FluidParticle : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────────────────────

        [Header("Fluid Properties")]
        [Tooltip("The substance type of this particle.")]
        [SerializeField] private FluidType fluidType = FluidType.Water;

        [Header("Visuals")]
        [SerializeField] private Color waterColor = new Color(0f, 0.5f, 1f, 1f);
        [SerializeField] private Color steamColor = new Color(0.9f, 0.9f, 0.9f, 0.7f);

        // ─────────────────────────────────────────────────────────────
        //  Component Cache
        // ─────────────────────────────────────────────────────────────

        private Rigidbody2D      rb;
        private CircleCollider2D col;
        private ThermalBody      thermalBody;
        private SpriteRenderer   sr;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private bool isDelivered;
        private LevelPhysicsConfig activeConfig;

        // ─────────────────────────────────────────────────────────────
        //  Public Properties
        // ─────────────────────────────────────────────────────────────

        public FluidType FluidType  => fluidType;
        public bool IsDelivered     => isDelivered;
        public Rigidbody2D Rigidbody => rb;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            rb  = GetComponent<Rigidbody2D>();
            col = GetComponent<CircleCollider2D>();
            thermalBody = GetComponent<ThermalBody>();
            sr = GetComponentInChildren<SpriteRenderer>();

            if (thermalBody != null)
            {
                thermalBody.OnTemperatureChangedFast += HandleTemperatureChanged;
            }
        }

        private void OnDestroy()
        {
            if (thermalBody != null)
            {
                thermalBody.OnTemperatureChangedFast -= HandleTemperatureChanged;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Phase Transition
        // ─────────────────────────────────────────────────────────────

        private void HandleTemperatureChanged(float temperature)
        {
            if (fluidType == FluidType.Water && temperature >= activeConfig.boilingTemperature)
            {
                SetFluidType(FluidType.Steam);
            }
            else if (fluidType == FluidType.Steam && temperature < activeConfig.boilingTemperature)
            {
                SetFluidType(FluidType.Water);
            }
        }

        private void SetFluidType(FluidType newType)
        {
            fluidType = newType;

            if (fluidType == FluidType.Water)
            {
                rb.mass           = activeConfig.particleMass;
                rb.linearDamping  = activeConfig.particleLinearDrag;
                rb.gravityScale   = activeConfig.gravityScale;
                if (sr != null) sr.color = waterColor;
            }
            else if (fluidType == FluidType.Steam)
            {
                rb.mass           = activeConfig.steamMass;
                rb.linearDamping  = activeConfig.steamLinearDrag;
                rb.gravityScale   = activeConfig.steamGravityScale;
                if (sr != null) sr.color = steamColor;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Pool API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by the WaterSource when this particle is spawned. Resets physics and state.
        /// </summary>
        internal void Spawn(LevelPhysicsConfig physicsConfig, FluidType initialType = FluidType.Water)
        {
            isDelivered = false;
            activeConfig = physicsConfig;

            // Spawn with the requested type (allows Boilers to emit Steam directly)
            SetFluidType(initialType);
            rb.angularDamping = physicsConfig.particleAngularDrag;

            // Reset physics state
            rb.linearVelocity  = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated       = true;

            // Reset thermal state
            if (thermalBody != null)
            {
                thermalBody.Initialize(physicsConfig.ambientTemperature, physicsConfig.defaultFluidCoolingRate);
            }
        }

        /// <summary>
        /// Returns this particle to the ObjectPoolManager.
        /// </summary>
        public void ReturnToPool()
        {
            isDelivered = false;
            rb.simulated = false;
            rb.linearVelocity  = Vector2.zero;
            rb.angularVelocity = 0f;
            ObjectPoolManager.ReturnObjectToPool(gameObject);
        }

        /// <summary>
        /// Called by FluidTarget when this particle is counted as delivered.
        /// Marks the particle so it won't be counted twice.
        /// </summary>
        public void MarkDelivered()
        {
            isDelivered = true;
        }
    }
}

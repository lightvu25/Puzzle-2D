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
        [SerializeField] private Color electrifiedColor = new Color(0.3f, 0.9f, 1f, 1f);

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

        private bool               isDelivered;
        private bool               isElectrified;
        private LevelPhysicsConfig activeConfig;

        /// <summary>
        /// Counts down the remaining lifetime of this particle while it is Steam.
        /// -1 means the timer is not running (particle is Water, or lifetime is disabled).
        /// </summary>
        private float steamLifetimeTimer = -1f;

        // ─────────────────────────────────────────────────────────────
        //  Public Properties
        // ─────────────────────────────────────────────────────────────

        public FluidType FluidType   => fluidType;
        public bool IsDelivered      => isDelivered;
        public bool IsElectrified    => isElectrified;
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

                // Cancel any running Steam lifetime timer.
                steamLifetimeTimer = -1f;
                // No need for an Update loop while we are Water.
                enabled = false;
            }
            else if (fluidType == FluidType.Steam)
            {
                rb.mass           = activeConfig.steamMass;
                rb.linearDamping  = activeConfig.steamLinearDrag;
                rb.gravityScale   = activeConfig.steamGravityScale;
                if (sr != null) sr.color = steamColor;

                // Start the Steam lifetime timer if configured.
                // steamLifetimeSec == 0 means infinite (timer stays at -1, Update stays off).
                if (activeConfig.steamLifetimeSec > 0f)
                {
                    steamLifetimeTimer = activeConfig.steamLifetimeSec;
                    enabled = true; // Wake up Update so the timer runs.
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Update — Steam Lifetime
        // ─────────────────────────────────────────────────────────────

        private void Update()
        {
            // This Update only runs when steamLifetimeTimer > 0 (enabled in SetFluidType).
            // It is disabled when the particle is Water, keeping CPU cost at zero for Water.
            if (steamLifetimeTimer < 0f)
            {
                enabled = false;
                return;
            }

            steamLifetimeTimer -= Time.deltaTime;

            if (steamLifetimeTimer <= 0f)
            {
                // Lifetime expired. Return the particle to the pool.
                // This works even if CoolingSource is currently touching this particle —
                // the HeatSource/CoolingSource receiver lists clean themselves up on null/inactive.
                ReturnToPool();
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Pool API
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Sets or clears the electrified status of this fluid particle.
        /// </summary>
        public void SetElectrified(bool electrified)
        {
            if (isElectrified == electrified) return;

            isElectrified = electrified;
            UpdateVisualColor();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isElectrified) return;

            FluidParticle other = collision.collider.GetComponent<FluidParticle>();
            if (other != null && !other.IsElectrified && other.FluidType == FluidType.Water)
            {
                other.SetElectrified(true);
            }
        }

        private void UpdateVisualColor()
        {
            if (sr == null) return;

            if (fluidType == FluidType.Steam)
            {
                sr.color = steamColor;
            }
            else if (isElectrified)
            {
                sr.color = electrifiedColor;
            }
            else
            {
                sr.color = waterColor;
            }
        }

        /// <summary>
        /// Called by the WaterSource when this particle is spawned. Resets physics and state.
        /// </summary>
        internal void Spawn(LevelPhysicsConfig physicsConfig, FluidType initialType = FluidType.Water)
        {
            isDelivered   = false;
            isElectrified = false;
            activeConfig  = physicsConfig;

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
            isDelivered        = false;
            isElectrified      = false;
            steamLifetimeTimer = -1f;  // Clear timer so next Spawn starts clean.
            enabled            = false; // Stop Update until next Spawn activates it.
            rb.simulated       = false;
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

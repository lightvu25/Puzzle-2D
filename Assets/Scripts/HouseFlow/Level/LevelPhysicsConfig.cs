using System;
using UnityEngine;

namespace HouseFlow.Level
{
    /// <summary>
    /// Physics configuration that can be applied per-level.
    /// All values override the project-level Physics2D defaults for this level.
    /// </summary>
    [Serializable]
    public struct LevelPhysicsConfig
    {
        [Header("Gravity")]
        [Tooltip("Multiplier applied to global Physics2D gravity for all fluid particles in this level. 1.0 = normal gravity.")]
        [Min(0f)]
        public float gravityScale;

        [Header("Particle Dynamics")]
        [Tooltip("Mass of each fluid particle Rigidbody2D.")]
        [Min(0.001f)]
        public float particleMass;

        [Tooltip("Linear drag applied to each fluid particle. Higher values slow particles faster.")]
        [Min(0f)]
        public float particleLinearDrag;

        [Tooltip("Angular drag applied to each fluid particle.")]
        [Min(0f)]
        public float particleAngularDrag;

        [Header("Thermal")]
        [Tooltip("Baseline ambient temperature of the level. Fluids cool down to this temperature.")]
        public float ambientTemperature;

        [Tooltip("Temperature at which Water converts to Steam.")]
        public float boilingTemperature;

        [Tooltip("Default rate at which fluids cool down towards ambient temperature (degrees per second).")]
        [Min(0f)]
        public float defaultFluidCoolingRate;

        [Header("Steam Dynamics")]
        [Tooltip("Gravity multiplier applied to Steam particles. Negative values make it rise.")]
        public float steamGravityScale;

        [Tooltip("Mass of Steam particles.")]
        [Min(0.001f)]
        public float steamMass;

        [Tooltip("Linear drag applied to Steam particles.")]
        [Min(0f)]
        public float steamLinearDrag;

        [Tooltip("How long a Steam particle lives before returning to pool. " +
                 "Set to 0 for infinite lifetime (legacy behaviour). GDD default: 6.0s")]
        [Min(0f)]
        public float steamLifetimeSec;

        [Header("Budget")]
        [Tooltip("Maximum number of live fluid particles each WaterSource may keep in play. " +
                 "When the cap is reached the oldest live particle is recycled (returned to the " +
                 "pool and immediately respawned) — zero allocation, bounded physics cost. " +
                 "Set to 0 for unlimited (legacy behaviour).")]
        [Min(0)]
        public int maxActiveParticles;

        [Header("Pneumatics")]
        [Tooltip("Duct friction factor μ used in the GDD formula: v_out = v_in × (1 - μ × L/D). " +
                 "Higher values reduce exit velocity over longer/narrower ducts. GDD-implied default: 0.05")]
        [Min(0f)]
        public float ductFrictionFactor;

        [Tooltip("Maximum open-room airflow dissipation distance in world units (Box2D meters). " +
                 "GDD specifies 3.5m (350dp). Used as a designer reference and collider-sizing hint.")]
        [Min(0.1f)]
        public float airDissipationDistance;

        /// <summary>
        /// Returns a sensible default configuration suitable for a standard water level.
        /// </summary>
        public static LevelPhysicsConfig Default => new LevelPhysicsConfig
        {
            gravityScale            = 1f,
            particleMass            = 0.1f,
            particleLinearDrag      = 0.5f,
            particleAngularDrag     = 0.05f,
            ambientTemperature      = 20f,
            boilingTemperature      = 100f,
            defaultFluidCoolingRate = 5f,
            steamGravityScale       = -0.2f,
            steamMass               = 0.02f,
            steamLinearDrag         = 1.5f,
            steamLifetimeSec        = 6.0f,   // GDD Appendix A: lifetime 6.0s
            maxActiveParticles      = 150,    // hard cap per source; enough headroom for delivery objectives
            ductFrictionFactor      = 0.05f,  // GDD-implied default μ
            airDissipationDistance  = 3.5f,   // GDD Appendix A: 3.5m / 350dp
        };
    }
}

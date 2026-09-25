using UnityEngine;

namespace HouseFlow.Fluid
{
    /// <summary>
    /// Emits fluid particles from a configurable spawn point.
    ///
    /// The WaterSource is passive — it does not decide when to emit.
    /// A Valve (or other mechanical device) calls SetEmissionEnabled() to open/close the source.
    ///
    /// All emission values are configurable from the Inspector — no hardcoded rates.
    /// </summary>
    public class WaterSource : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────────────────────

        [Header("Emission Settings")]
        [Tooltip("Whether this source starts in the emitting state.")]
        [SerializeField] private bool startsEmitting = false;

        [Tooltip("Number of particles emitted per second.")]
        [SerializeField, Min(0.1f)] private float emissionRate = 5f;

        [Tooltip("Direction particles are launched on spawn (local space, normalised at runtime).")]
        [SerializeField] private Vector2 emissionDirection = Vector2.down;

        [Tooltip("Force applied to each particle on spawn.")]
        [SerializeField, Min(0f)] private float emissionForce = 3f;

        [Tooltip("Maximum number of particles this source may emit over its lifetime. 0 = unlimited.")]
        [SerializeField, Min(0)] private int maxEmittedParticles = 0;

        [Header("Fluid")]
        [Tooltip("The prefab to spawn. Should have a FluidParticle component.")]
        [SerializeField] private GameObject fluidParticlePrefab;
        [Tooltip("The substance type emitted by this source.")]
        [SerializeField] private FluidType fluidType = FluidType.Water;

        [Header("Spawn Point")]
        [Tooltip("Transform from which particles are spawned. Defaults to this GameObject's position if not assigned.")]
        [SerializeField] private Transform spawnPoint;

        [Tooltip("Random offset radius around the spawn point (avoids particle stacking).")]
        [SerializeField, Min(0f)] private float spawnRadius = 0.05f;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private HouseFlow.Level.LevelPhysicsConfig currentPhysicsConfig;
        private bool              emissionEnabled;
        private float             emissionTimer;
        private int               totalEmitted;

        /// <summary>
        /// Live particles emitted by this source, oldest first. Used to enforce
        /// LevelPhysicsConfig.maxActiveParticles via steal-oldest recycling.
        /// Entries may be returned to the pool by other systems (dead zones,
        /// steam lifetime, level reset) and are pruned on emit.
        /// </summary>
        private readonly System.Collections.Generic.List<FluidParticle> liveParticles =
            new System.Collections.Generic.List<FluidParticle>();

        // ─────────────────────────────────────────────────────────────
        //  Public Properties
        // ─────────────────────────────────────────────────────────────

        public bool EmissionEnabled => emissionEnabled;
        public int  TotalEmitted    => totalEmitted;
        public FluidType FluidType  => fluidType;

        // ─────────────────────────────────────────────────────────────
        //  Initialisation (called by LevelRoot)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Injects the physics configuration for emitted particles. Called by LevelRoot.Awake().
        /// </summary>
        public void InitializeSource(HouseFlow.Level.LevelPhysicsConfig config)
        {
            currentPhysicsConfig = config;
        }

        private void Awake()
        {
            emissionEnabled = startsEmitting;
            emissionTimer   = 0f;
            totalEmitted    = 0;

            if (spawnPoint == null)
                spawnPoint = transform;
        }

        // ─────────────────────────────────────────────────────────────
        //  Emission Control
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Opens or closes this water source. Called by Valve or other mechanical devices.
        /// </summary>
        public void SetEmissionEnabled(bool enabled)
        {
            emissionEnabled = enabled;
        }

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Update()
        {
            if (!emissionEnabled || fluidParticlePrefab == null) return;
            if (maxEmittedParticles > 0 && totalEmitted >= maxEmittedParticles) return;

            emissionTimer += Time.deltaTime;

            float interval = 1f / emissionRate;
            while (emissionTimer >= interval)
            {
                emissionTimer -= interval;
                EmitParticle();

                if (maxEmittedParticles > 0 && totalEmitted >= maxEmittedParticles)
                    break;
            }
        }

        private void EmitParticle()
        {
            // Prune particles already reclaimed by other systems (dead zone,
            // steam lifetime, level reset) so the cap reflects live count only.
            for (int i = liveParticles.Count - 1; i >= 0; i--)
            {
                var tracked = liveParticles[i];
                if (tracked == null || !tracked.gameObject.activeSelf)
                    liveParticles.RemoveAt(i);
            }

            // Enforce the per-level live-particle budget: recycle the oldest
            // live particles until there is room for a new spawn — this bounds
            // the concurrent count, not just queue growth.
            int maxActive = currentPhysicsConfig.maxActiveParticles;
            if (maxActive > 0)
            {
                while (liveParticles.Count >= maxActive)
                {
                    var oldest = liveParticles[0];
                    liveParticles.RemoveAt(0);
                    if (oldest != null && oldest.gameObject.activeSelf)
                        oldest.ReturnToPool();
                }
            }

            Vector3 offset  = (Vector3)(Random.insideUnitCircle * spawnRadius);
            Vector3 spawnPos = spawnPoint.position + offset;

            GameObject obj = ObjectPoolManager.SpawnObject(fluidParticlePrefab, spawnPos, Quaternion.identity, ObjectPoolManager.PoolType.ParticleSystem);
            if (obj == null) return;

            FluidParticle particle = obj.GetComponent<FluidParticle>();
            if (particle != null)
            {
                particle.Spawn(currentPhysicsConfig, fluidType);
                liveParticles.Add(particle);
            }

            Vector2 dir = emissionDirection.sqrMagnitude > 0f
                ? emissionDirection.normalized
                : Vector2.down;

            // Direction in world space (transform the local direction)
            Vector2 worldDir = transform.TransformDirection(dir);
            if (particle != null)
            {
                particle.Rigidbody.AddForce(worldDir * emissionForce, ForceMode2D.Impulse);
            }

            totalEmitted++;
        }

        // ─────────────────────────────────────────────────────────────
        //  Reset
        // ─────────────────────────────────────────────────────────────

        /// <summary>Restores this source to its initial configuration. Called by LevelRoot.ResetAll().</summary>
        public void ResetSource()
        {
            emissionEnabled = startsEmitting;
            emissionTimer   = 0f;
            totalEmitted    = 0;
            liveParticles.Clear(); // LevelRoot.ResetAll already returns actives to the pool
        }

        // ─────────────────────────────────────────────────────────────
        //  Editor Gizmos
        // ─────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = emissionEnabled ? Color.cyan : Color.gray;
            Vector3 pos  = spawnPoint != null ? spawnPoint.position : transform.position;
            Gizmos.DrawWireSphere(pos, spawnRadius);

            Vector2 dir = emissionDirection.sqrMagnitude > 0f
                ? emissionDirection.normalized
                : Vector2.down;
            Vector3 worldDir = transform.TransformDirection(dir);
            Gizmos.DrawRay(pos, worldDir * 0.5f);
        }
    }
}

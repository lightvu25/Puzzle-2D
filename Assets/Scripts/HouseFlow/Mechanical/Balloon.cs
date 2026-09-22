using UnityEngine;

namespace HouseFlow.Mechanical
{
    /// <summary>
    /// A free-floating inflatable body that rises when inside an open-room airflow zone.
    ///
    /// GDD Reference (Section 6.3): "Balloons"
    ///
    /// Design:
    ///   - Relies on Unity's AreaEffector2D (via AirflowSource) to push this Rigidbody2D naturally.
    ///   - Designer tunes gravityScale and mass so the airflow force lifts it when the fan is active.
    ///   - No code coupling to AirflowSource — physics interaction is implicit via the effector trigger.
    ///   - The "inflated" state is modelled as gravityScale transitioning from deflated to inflated value.
    ///   - Update() self-disables when gravityScale has stabilised (ThermalBody pattern).
    ///
    /// Performance:
    ///   - This component's Update() is only active while the balloon is changing state.
    ///   - At rest (deflated or fully inflated), it costs zero per-frame CPU.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Balloon : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────────────────────

        [Header("Gravity Scale")]
        [Tooltip("Gravity scale when deflated / at rest (usually slightly positive so balloon sinks).")]
        [SerializeField] private float deflatedGravityScale = 0.3f;

        [Tooltip("Gravity scale at full inflation (negative = rises, 0 = neutral buoyancy).")]
        [SerializeField] private float inflatedGravityScale = -0.5f;

        [Tooltip("How quickly (per second) the balloon gravity scale moves toward the target.")]
        [Min(0.01f)]
        [SerializeField] private float inflationSpeed = 0.8f;

        [Tooltip("How quickly (per second) the balloon deflates when not in an airflow zone.")]
        [Min(0.01f)]
        [SerializeField] private float deflationSpeed = 0.4f;

        [Header("Movement Constraints")]
        [Tooltip("Optional: constrain the balloon to a maximum world Y position so it doesn't leave the level.")]
        [SerializeField] private bool limitMaxY = false;
        [SerializeField] private float maxY = 10f;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private Rigidbody2D rb;
        private int         airflowZoneCount;   // How many AirflowSource zones currently overlap this balloon.
        private float       currentGravityScale;
        private Vector3     startLocalPosition;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            rb                   = GetComponent<Rigidbody2D>();
            currentGravityScale  = deflatedGravityScale;
            rb.gravityScale      = currentGravityScale;
            startLocalPosition   = transform.localPosition;

            // Start idle — no state change needed until a fan activates.
            enabled = false;
        }

        private void Update()
        {
            float target = airflowZoneCount > 0 ? inflatedGravityScale : deflatedGravityScale;
            float speed  = airflowZoneCount > 0 ? inflationSpeed       : deflationSpeed;

            currentGravityScale = Mathf.MoveTowards(currentGravityScale, target, speed * Time.deltaTime);
            rb.gravityScale     = currentGravityScale;

            // Clamp max Y position if configured.
            if (limitMaxY && transform.position.y > maxY)
            {
                Vector2 pos = rb.position;
                pos.y       = maxY;
                rb.position = pos;

                // Kill upward velocity to prevent continued pushing against the ceiling.
                if (rb.linearVelocity.y > 0f)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }

            // Self-disable when stable (at target gravity scale with minimal velocity change).
            bool atTarget = Mathf.Approximately(currentGravityScale, target);
            if (atTarget)
                enabled = false;
        }

        // ─────────────────────────────────────────────────────────────
        //  Airflow Detection
        //  AreaEffector2D pushes Rigidbody2D automatically.
        //  We only need to track entry/exit to drive gravityScale interpolation.
        // ─────────────────────────────────────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<AirflowSource>() == null &&
                !other.usedByEffector) return;

            airflowZoneCount++;
            enabled = true; // Wake up Update to inflate.
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<AirflowSource>() == null &&
                !other.usedByEffector) return;

            airflowZoneCount = Mathf.Max(0, airflowZoneCount - 1);
            enabled = true; // Wake up Update to deflate.
        }

        // ─────────────────────────────────────────────────────────────
        //  Reset
        // ─────────────────────────────────────────────────────────────

        /// <summary>Restores balloon to its initial position and deflated state. Called by LevelRoot.ResetAll().</summary>
        public void ResetBalloon()
        {
            airflowZoneCount    = 0;
            currentGravityScale = deflatedGravityScale;
            rb.gravityScale     = currentGravityScale;
            rb.linearVelocity   = Vector2.zero;
            rb.angularVelocity  = 0f;
            transform.localPosition = startLocalPosition;
            enabled = false;
        }

        // ─────────────────────────────────────────────────────────────
        //  Editor Gizmos
        // ─────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, 0.3f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.4f,
                $"[Balloon] gScale={rb?.gravityScale:F2} zones={airflowZoneCount}");
#endif
        }
    }
}

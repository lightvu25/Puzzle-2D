using System;
using UnityEngine;

namespace HouseFlow.Mechanical
{
    /// <summary>
    /// Represents an open-room airflow source (fan or vent) that pushes objects in a direction.
    /// Acts as a clean wrapper around Unity's AreaEffector2D.
    ///
    /// Phase 3 extension: Implements IAirflowProvider to act as the root of a duct chain.
    /// Separates open-room ForceMagnitude (for AreaEffector2D) from duct ExitVelocity (m/s).
    /// </summary>
    [RequireComponent(typeof(AreaEffector2D))]
    [RequireComponent(typeof(Collider2D))]
    public class AirflowSource : MonoBehaviour, IAirflowProvider
    {
        [Header("Airflow Settings")]
        [Tooltip("Should this airflow start active?")]
        [SerializeField] private bool startsActive = true;

        [Header("Duct Injection")]
        [Tooltip("The initial velocity (v_in) injected into attached ducts (m/s). " +
                 "This is completely separate from the AreaEffector2D force magnitude.")]
        [Min(0f)]
        [SerializeField] private float baseAirflowVelocity = 10f;

        // ─────────────────────────────────────────────────────────────
        //  Events (IAirflowProvider)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Fired when the active state or flow changes. Downstream ducts subscribe to this.
        /// </summary>
        public event Action OnFlowChanged;

        // ─────────────────────────────────────────────────────────────
        //  Internal State
        // ─────────────────────────────────────────────────────────────

        private AreaEffector2D effector;
        private Collider2D     col;

        // ─────────────────────────────────────────────────────────────
        //  Public Properties (IAirflowProvider)
        // ─────────────────────────────────────────────────────────────

        public bool IsAirflowActive => effector != null && effector.enabled;
        
        public float ExitVelocity => IsAirflowActive ? baseAirflowVelocity : 0f;

        // ─────────────────────────────────────────────────────────────
        //  Public Properties (Open-Room)
        // ─────────────────────────────────────────────────────────────

        public bool IsActive => IsAirflowActive;

        /// <summary>The magnitude of the force applied by the AreaEffector2D.</summary>
        public float ForceMagnitude => effector != null ? effector.forceMagnitude : 0f;

        /// <summary>The angle (degrees) of the force vector. 0 = right, 90 = up.</summary>
        public float ForceAngle => effector != null ? effector.forceAngle : 0f;

        /// <summary>The normalised world-space force direction derived from ForceAngle.</summary>
        public Vector2 ForceDirection
        {
            get
            {
                float rad = effector.forceAngle * Mathf.Deg2Rad;
                return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            effector = GetComponent<AreaEffector2D>();
            col      = GetComponent<Collider2D>();

            col.isTrigger      = true;
            col.usedByEffector = true;

            SetActive(startsActive);
        }

        // ─────────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────────

        public void SetActive(bool active)
        {
            if (effector == null) return;

            bool changed = effector.enabled != active;
            effector.enabled = active;

            if (changed)
                OnFlowChanged?.Invoke();
        }

        public void ResetSource()
        {
            // SetActive handles invoking OnFlowChanged if the state actually changes.
            // If we are already at startsActive, downstream ducts shouldn't need a recalculation 
            // from us (they will reset themselves via LevelRoot or cascade).
            SetActive(startsActive);
        }
    }
}

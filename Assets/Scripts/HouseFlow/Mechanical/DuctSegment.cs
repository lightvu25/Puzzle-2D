using System;
using UnityEngine;
using HouseFlow.Level;

namespace HouseFlow.Mechanical
{
    /// <summary>
    /// Represents a rigid pneumatic duct exit nozzle.
    ///
    /// GDD Reference: "Flow velocity inside rigid ducting is modeled as a 1D vector line 
    /// segment with linear velocity dampening: v_out = v_in × (1 − μ × L/D)"
    ///
    /// Design:
    ///   - This is NOT a runtime graph. It is a static pre-computed nozzle.
    ///   - It subscribes to an upstream AirflowSource and PneumaticGate.
    ///   - Uses a velocity-targeting approach (PD controller) rather than raw force
    ///     to ensure Rigidbody2D behaviour matches the GDD's velocity requirement.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DuctSegment : MonoBehaviour, IAirflowProvider
    {
        // ─────────────────────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────────────────────

        [Header("Upstream Connections")]
        [Tooltip("The airflow provider feeding this duct chain (AirflowSource or another DuctSegment).")]
        [SerializeField] private MonoBehaviour upstreamProviderRef;
        private IAirflowProvider upstreamProvider => upstreamProviderRef as IAirflowProvider;

        [Tooltip("Optional one-way gate immediately upstream of this duct. If closed, airflow is 0.")]
        [SerializeField] private PneumaticGate upstreamGate;

        [Header("Duct Physics (GDD Formula)")]
        [Tooltip("Length of the duct (L).")]
        [Min(0f)]
        [SerializeField] private float ductLength = 5f;

        [Tooltip("Hydraulic diameter of the duct (D).")]
        [Min(0.1f)]
        [SerializeField] private float hydraulicDiameter = 1f;

        [Header("Nozzle Behavior")]
        [Tooltip("The direction air exits this nozzle (local space).")]
        [SerializeField] private Vector2 localExitDirection = Vector2.up;

        [Tooltip("How aggressively to accelerate bodies to the target velocity.")]
        [SerializeField] private float accelerationFactor = 50f;

        // ─────────────────────────────────────────────────────────────
        //  Events (IAirflowProvider)
        // ─────────────────────────────────────────────────────────────

        public event Action OnFlowChanged;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private float currentExitVelocity;
        private Vector2 worldExitDirection;
        private bool isAirflowActive;

        // ─────────────────────────────────────────────────────────────
        //  Public Properties (IAirflowProvider)
        // ─────────────────────────────────────────────────────────────

        public bool IsAirflowActive => isAirflowActive;
        public float ExitVelocity => currentExitVelocity;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;

            if (upstreamProvider != null)
            {
                upstreamProvider.OnFlowChanged += HandleUpstreamStateChanged;
            }

            if (upstreamGate != null)
            {
                upstreamGate.OnGateStateChanged += HandleUpstreamStateChanged;
            }

            RecalculateVelocity();
        }

        private void OnDestroy()
        {
            if (upstreamProvider != null)
            {
                upstreamProvider.OnFlowChanged -= HandleUpstreamStateChanged;
            }

            if (upstreamGate != null)
            {
                upstreamGate.OnGateStateChanged -= HandleUpstreamStateChanged;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!isAirflowActive || currentExitVelocity <= 0f) return;

            Rigidbody2D rb = other.attachedRigidbody;
            if (rb == null) return;

            // --- VELOCITY-TARGETING PHYSICS ---
            Vector2 targetVel = worldExitDirection * currentExitVelocity;
            Vector2 velDiff = targetVel - rb.linearVelocity;

            float diffInDir = Vector2.Dot(velDiff, worldExitDirection);

            if (diffInDir > 0f)
            {
                // Apply a proportional force to close the velocity gap, scaled by mass.
                Vector2 force = worldExitDirection * (diffInDir * rb.mass * accelerationFactor);
                rb.AddForce(force, ForceMode2D.Force);
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Logic
        // ─────────────────────────────────────────────────────────────

        private void HandleUpstreamStateChanged(bool _state)
        {
            RecalculateVelocity();
        }

        private void HandleUpstreamStateChanged()
        {
            RecalculateVelocity();
        }

        private void RecalculateVelocity()
        {
            worldExitDirection = transform.TransformDirection(localExitDirection).normalized;

            // 1. Check if blocked
            bool sourceActive = upstreamProvider != null && upstreamProvider.IsAirflowActive;
            bool gateOpen = upstreamGate == null || upstreamGate.IsOpen;

            bool wasActive = isAirflowActive;
            float oldVelocity = currentExitVelocity;

            isAirflowActive = sourceActive && gateOpen;

            if (!isAirflowActive)
            {
                currentExitVelocity = 0f;
            }
            else
            {
                // 2. GDD Formula: v_out = v_in × (1 − μ × L/D)
                float v_in = upstreamProvider.ExitVelocity;
                float mu = LevelPhysicsConfig.Default.ductFrictionFactor;

                LevelRoot root = GetComponentInParent<LevelRoot>();
                if (root != null)
                {
                    mu = root.PhysicsConfig.ductFrictionFactor;
                }

                float dampening = 1f - (mu * (ductLength / hydraulicDiameter));
                dampening = Mathf.Clamp01(dampening);

                currentExitVelocity = v_in * dampening;
            }

            // If state or velocity changed, fire event to notify downstream ducts/bladders
            if (wasActive != isAirflowActive || !Mathf.Approximately(oldVelocity, currentExitVelocity))
            {
                OnFlowChanged?.Invoke();
            }
        }

        public void ForceRecalculate()
        {
            RecalculateVelocity();
        }

        // ─────────────────────────────────────────────────────────────
        //  Editor Gizmos
        // ─────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            worldExitDirection = transform.TransformDirection(localExitDirection).normalized;
            Gizmos.color = Color.cyan;
            
            Vector3 start = transform.position;
            Vector3 end = start + (Vector3)worldExitDirection * 1.5f;
            
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.1f);

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                UnityEditor.Handles.Label(end, $"[Duct Exit] v_out={currentExitVelocity:F1}");
            }
#endif
        }
    }
}

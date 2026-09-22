using System;
using UnityEngine;

namespace HouseFlow.Mechanical
{
    /// <summary>
    /// A player-interactable one-way pneumatic flap gate that allows or blocks airflow and objects.
    ///
    /// GDD Reference (Section 6.3): "One-way pneumatic flap gate"
    ///
    /// Design:
    ///   - Implements IInteractable so LevelFlowController's tap raycast detects it automatically.
    ///   - Has a physical Collider2D (the flap) that enables/disables to block or allow passage.
    ///   - Fires OnGateStateChanged so DuctSegments in the chain can recalculate downstream pressure.
    ///   - Follows the established Reset pattern: ResetGate() called by LevelRoot.ResetAll().
    /// </summary>
    public class PneumaticGate : MonoBehaviour, IInteractable
    {
        // ─────────────────────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────────────────────

        [Header("State")]
        [Tooltip("Whether the gate starts open (allows airflow and objects through).")]
        [SerializeField] private bool startsOpen = false;

        [Header("Physics")]
        [Tooltip("The collider that physically blocks airflow and objects when the gate is closed. " +
                 "Assign the flap's Collider2D here. When open, this collider is disabled.")]
        [SerializeField] private Collider2D flapCollider;

        [Header("Animation")]
        [Tooltip("Transform of the visual flap that rotates when toggled. May be the same GameObject.")]
        [SerializeField] private Transform flapVisual;

        [Tooltip("Target local Z rotation when open (degrees).")]
        [SerializeField] private float openRotationZ = 90f;

        [Tooltip("Target local Z rotation when closed (degrees).")]
        [SerializeField] private float closedRotationZ = 0f;

        [Header("Interaction")]
        [Tooltip("Whether the player can tap this gate to toggle it.")]
        [SerializeField] private bool interactable = true;

        // ─────────────────────────────────────────────────────────────
        //  Events
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Fired when the gate opens or closes. Argument is the new open state.
        /// DuctSegment subscribes to this to recalculate downstream pressure.
        /// </summary>
        public event Action<bool> OnGateStateChanged;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private bool isOpen;

        // ─────────────────────────────────────────────────────────────
        //  IInteractable
        // ─────────────────────────────────────────────────────────────

        public bool IsInteractable => interactable;

        public void Interact()
        {
            if (!interactable) return;
            Toggle();
        }

        // ─────────────────────────────────────────────────────────────
        //  Public State
        // ─────────────────────────────────────────────────────────────

        public bool IsOpen => isOpen;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            isOpen = startsOpen;

            if (flapCollider != null)
            {
                // Integrate PlatformEffector2D for true one-way physical behavior if assigned by the designer
                var effector = flapCollider.GetComponent<PlatformEffector2D>();
                if (effector != null)
                {
                    flapCollider.usedByEffector = true;
                }
            }

            ApplyState(isOpen, immediate: true);
        }

        // ─────────────────────────────────────────────────────────────
        //  Gate Logic
        // ─────────────────────────────────────────────────────────────

        private void Toggle()
        {
            isOpen = !isOpen;
            ApplyState(isOpen, immediate: false);
            OnGateStateChanged?.Invoke(isOpen);
        }

        private void ApplyState(bool open, bool immediate)
        {
            // Enable/disable the physical flap collider.
            // If closed AND equipped with a PlatformEffector2D, it behaves as a one-way solid flap.
            if (flapCollider != null)
                flapCollider.enabled = !open;

            // Rotate the visual flap.
            if (flapVisual != null)
            {
                float targetZ = open ? openRotationZ : closedRotationZ;

                if (immediate)
                {
                    Vector3 angles = flapVisual.localEulerAngles;
                    flapVisual.localEulerAngles = new Vector3(angles.x, angles.y, targetZ);
                }
                else
                {
                    // Simple instant toggle
                    Vector3 angles = flapVisual.localEulerAngles;
                    flapVisual.localEulerAngles = new Vector3(angles.x, angles.y, targetZ);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Reset
        // ─────────────────────────────────────────────────────────────

        /// <summary>Restores gate to its initial state. Called by LevelRoot.ResetAll().</summary>
        public void ResetGate()
        {
            isOpen = startsOpen;
            ApplyState(isOpen, immediate: true);
            
            // Explicitly fire state change during reset to guarantee downstream ducts 
            // deterministically recalculate their velocity without stale data.
            OnGateStateChanged?.Invoke(isOpen);
        }

        // ─────────────────────────────────────────────────────────────
        //  Editor Gizmos
        // ─────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isOpen ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0.3f, 0f, 0.5f);
            Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.1f, 0f));

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f,
                isOpen ? "[Gate: OPEN]" : "[Gate: CLOSED]");
#endif
        }
    }
}

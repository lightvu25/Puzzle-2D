using System;
using System.Collections.Generic;
using UnityEngine;

namespace HouseFlow.Fluid
{
    /// <summary>
    /// Detects fluid particles entering its trigger zone and counts valid deliveries.
    ///
    /// Key rules:
    ///   - Only counts particles of the accepted FluidType.
    ///   - Each particle is counted at most once (tracked by instance ID).
    ///   - Fires OnParticleDelivered each time a new particle is counted.
    ///   - Fires OnCapacityReached if the container fills to its configured capacity.
    ///
    /// Requires a 2D Trigger Collider on this GameObject or a child.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FluidTarget : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────────────────────

        [Header("Identity")]
        [Tooltip("Unique identifier that ObjectiveDefinition.targetId must match for this target to be tracked.")]
        [SerializeField] private string targetId;

        [Header("Fluid")]
        [Tooltip("Only particles of this fluid type will be counted.")]
        [SerializeField] private FluidType acceptedFluidType = FluidType.Water;

        [Header("Capacity")]
        [Tooltip("Maximum number of particles this container can hold. 0 = unlimited.")]
        [SerializeField, Min(0)] private int capacity = 0;

        // ─────────────────────────────────────────────────────────────
        //  Events
        // ─────────────────────────────────────────────────────────────

        /// <summary>Fires every time a valid particle is delivered. Argument = new total.</summary>
        public event Action<int> OnParticleDelivered;

        /// <summary>Fires once when the container reaches its capacity (if capacity > 0).</summary>
        public event Action OnCapacityReached;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private int             currentCount;
        private readonly HashSet<int> deliveredParticleIds = new HashSet<int>();

        // ─────────────────────────────────────────────────────────────
        //  Public Properties
        // ─────────────────────────────────────────────────────────────

        public string     TargetId             => targetId;
        public FluidType  AcceptedFluidType    => acceptedFluidType;
        public int        CurrentParticleCount => currentCount;
        public int        Capacity             => capacity;
        public bool       IsAtCapacity         => capacity > 0 && currentCount >= capacity;

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            // Ensure the collider is set as a trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && !col.isTrigger)
            {
                Debug.LogWarning($"[FluidTarget] '{name}': Collider2D is not a trigger. Setting isTrigger = true.", this);
                col.isTrigger = true;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Collision Detection
        // ─────────────────────────────────────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsAtCapacity) return;

            FluidParticle particle = other.GetComponent<FluidParticle>();
            if (particle == null) return;
            if (particle.FluidType != acceptedFluidType) return;
            if (particle.IsDelivered) return;
            if (!deliveredParticleIds.Add(particle.GetInstanceID())) return; // Already counted

            particle.MarkDelivered();
            currentCount++;

            OnParticleDelivered?.Invoke(currentCount);

            if (capacity > 0 && currentCount >= capacity)
                OnCapacityReached?.Invoke();
        }

        // ─────────────────────────────────────────────────────────────
        //  Reset
        // ─────────────────────────────────────────────────────────────

        /// <summary>Resets the delivery count. Called by LevelRoot.ResetAll().</summary>
        public void ResetTarget()
        {
            currentCount = 0;
            deliveredParticleIds.Clear();
        }

        // ─────────────────────────────────────────────────────────────
        //  Editor Gizmos
        // ─────────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.4f);
            Collider2D col = GetComponent<Collider2D>();
            if (col is BoxCollider2D box)
            {
                Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
            }

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
                $"[{targetId}] {currentCount}/{(capacity > 0 ? capacity.ToString() : "∞")}");
#endif
        }
    }
}

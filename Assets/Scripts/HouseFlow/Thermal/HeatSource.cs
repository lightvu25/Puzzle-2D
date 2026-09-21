using System.Collections.Generic;
using UnityEngine;

namespace HouseFlow.Thermal
{
    /// <summary>
    /// Applies heat over time to any IThermalReceiver within its trigger collider.
    /// 
    /// PERFORMANCE DESIGN:
    /// Uses OnTriggerEnter/Exit to manage a cached list of receivers, rather than
    /// relying on expensive per-frame OnTriggerStay physics calls.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HeatSource : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────────────────────

        [Header("Heating Properties")]
        [Tooltip("Amount of temperature added per second to objects in the trigger.")]
        [Min(0f)]
        [SerializeField] private float heatRate = 10f;

        [Tooltip("Does this heat source start active?")]
        [SerializeField] private bool startsActive = true;

        [Tooltip("Optional: Maximum temperature this source can heat an object to. (0 = no limit)")]
        [Min(0f)]
        [SerializeField] private float maxTemperature = 0f;

        // ─────────────────────────────────────────────────────────────
        //  Runtime State
        // ─────────────────────────────────────────────────────────────

        private bool isActive;
        private List<IThermalReceiver> receivers = new List<IThermalReceiver>();

        // ─────────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────────

        public bool IsActive => isActive;

        public void SetActive(bool active)
        {
            isActive = active;
        }

        public void ResetSource()
        {
            isActive = startsActive;
            receivers.Clear(); // Cleared in case objects were reset/pooled
        }

        // ─────────────────────────────────────────────────────────────
        //  Unity Lifecycle
        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            isActive = startsActive;
        }

        private void Update()
        {
            if (!isActive || receivers.Count == 0) return;

            float heatThisFrame = heatRate * Time.deltaTime;

            // Iterate backwards to allow safe, zero-allocation removal of null/inactive objects
            for (int i = receivers.Count - 1; i >= 0; i--)
            {
                var receiver = receivers[i];

                // Unity's null check also handles destroyed objects.
                // We also check if the object was deactivated (e.g., returned to pool).
                if (receiver == null || !((MonoBehaviour)receiver).gameObject.activeInHierarchy)
                {
                    receivers.RemoveAt(i);
                    continue;
                }

                if (maxTemperature <= 0f || receiver.Temperature < maxTemperature)
                {
                    receiver.ApplyHeat(heatThisFrame);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var receiver = other.GetComponentInParent<IThermalReceiver>();
            if (receiver != null && !receivers.Contains(receiver))
            {
                receivers.Add(receiver);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var receiver = other.GetComponentInParent<IThermalReceiver>();
            if (receiver != null)
            {
                receivers.Remove(receiver);
            }
        }
    }
}

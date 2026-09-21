using System.Collections.Generic;
using UnityEngine;

namespace HouseFlow.Thermal
{
    /// <summary>
    /// Extracts heat over time from any IThermalReceiver within its trigger collider.
    /// Used for Chill Plates to force rapid steam condensation.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CoolingSource : MonoBehaviour
    {
        [Header("Cooling Properties")]
        [Tooltip("Amount of temperature subtracted per second from objects in the trigger.")]
        [Min(0f)]
        [SerializeField] private float coolingRate = 50f;

        [Tooltip("Does this cooling source start active?")]
        [SerializeField] private bool startsActive = true;

        [Tooltip("Optional: Minimum temperature this source can cool an object to.")]
        [SerializeField] private float minTemperature = 20f;

        private bool isActive;
        private List<IThermalReceiver> receivers = new List<IThermalReceiver>();

        public bool IsActive => isActive;

        public void SetActive(bool active)
        {
            isActive = active;
        }

        public void ResetSource()
        {
            isActive = startsActive;
            receivers.Clear();
        }

        private void Awake()
        {
            isActive = startsActive;
        }

        private void Update()
        {
            if (!isActive || receivers.Count == 0) return;

            float coolingThisFrame = coolingRate * Time.deltaTime;

            for (int i = receivers.Count - 1; i >= 0; i--)
            {
                var receiver = receivers[i];

                if (receiver == null || !((MonoBehaviour)receiver).gameObject.activeInHierarchy)
                {
                    receivers.RemoveAt(i);
                    continue;
                }

                if (receiver.Temperature > minTemperature)
                {
                    // Apply negative heat to cool it down
                    receiver.ApplyHeat(-coolingThisFrame);
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

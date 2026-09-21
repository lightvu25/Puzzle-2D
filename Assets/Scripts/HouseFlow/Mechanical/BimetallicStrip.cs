using UnityEngine;
using HouseFlow.Thermal;

namespace HouseFlow.Mechanical
{
    /// <summary>
    /// A mechanical strip that bends/moves when heated.
    /// Acts as a thermal door or switch.
    /// </summary>
    [RequireComponent(typeof(ThermalBody))]
    public class BimetallicStrip : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The visual/physical part that moves.")]
        [SerializeField] private Transform movingPart;

        [Tooltip("Temperature required to fully activate/bend.")]
        [SerializeField] private float activationTemperature = 100f;

        [Tooltip("Local position when cold (ambient).")]
        [SerializeField] private Vector3 coldLocalPosition;

        [Tooltip("Local position when hot (activated).")]
        [SerializeField] private Vector3 hotLocalPosition;

        [Tooltip("How fast it interpolates between states.")]
        [SerializeField] private float animationSpeed = 5f;

        private ThermalBody thermalBody;
        private Vector3 targetPosition;

        private void Awake()
        {
            thermalBody = GetComponent<ThermalBody>();
            if (movingPart == null) movingPart = transform;
            
            targetPosition = coldLocalPosition;
            movingPart.localPosition = coldLocalPosition;

            thermalBody.OnTemperatureChangedFast += HandleTemperature;
        }

        private void OnDestroy()
        {
            if (thermalBody != null)
                thermalBody.OnTemperatureChangedFast -= HandleTemperature;
        }

        private void HandleTemperature(float temperature)
        {
            // Calculate a normalized activation ratio based on an assumed 20C ambient floor
            // In a real scenario, we might want a min/max range, but a simple >= check is requested by design.
            // For smooth bending, we'll interpolate between ambient (e.g. 20) and activation.
            
            // To prevent magic numbers, we just assume 0 to activation, or just toggle.
            // A Bimetallic strip usually bends gradually.
            float ratio = Mathf.Clamp01(temperature / activationTemperature);
            targetPosition = Vector3.Lerp(coldLocalPosition, hotLocalPosition, ratio);
            
            // Optimization: If the target changed, we could enable an Update loop to lerp it.
            // To keep it simple and robust, we'll just lerp in Update.
        }

        private void Update()
        {
            if (movingPart == null) return;
            
            // Smoothly move towards the target position
            if (Vector3.SqrMagnitude(movingPart.localPosition - targetPosition) > 0.0001f)
            {
                movingPart.localPosition = Vector3.Lerp(movingPart.localPosition, targetPosition, Time.deltaTime * animationSpeed);
            }
        }
    }
}

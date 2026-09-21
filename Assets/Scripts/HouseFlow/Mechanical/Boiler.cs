using UnityEngine;
using HouseFlow.Fluid;
using HouseFlow.Thermal;
using HouseFlow.Level;

namespace HouseFlow.Mechanical
{
    /// <summary>
    /// A mechanical Boiler that requires heat to produce Steam.
    /// It links its internal ThermalBody to a WaterSource set to emit Steam.
    /// </summary>
    [RequireComponent(typeof(ThermalBody))]
    public class Boiler : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The WaterSource attached to this boiler. (Must be configured to emit Steam in Inspector).")]
        [SerializeField] private WaterSource steamOutput;

        [Tooltip("Temperature required to start producing steam.")]
        [SerializeField] private float activationTemperature = 100f;

        private ThermalBody thermalBody;
        private bool isProducing;

        private void Awake()
        {
            thermalBody = GetComponent<ThermalBody>();
            
            // Subscribe to temperature changes to toggle the source
            thermalBody.OnTemperatureChangedFast += HandleTemperature;
        }

        private void OnDestroy()
        {
            if (thermalBody != null)
                thermalBody.OnTemperatureChangedFast -= HandleTemperature;
        }

        private void HandleTemperature(float temperature)
        {
            if (steamOutput == null) return;

            bool shouldProduce = temperature >= activationTemperature;

            if (shouldProduce != isProducing)
            {
                isProducing = shouldProduce;
                steamOutput.SetEmissionEnabled(isProducing);
            }
        }

        public void ResetBoiler()
        {
            isProducing = false;
            if (steamOutput != null)
                steamOutput.SetEmissionEnabled(false);
        }
    }
}

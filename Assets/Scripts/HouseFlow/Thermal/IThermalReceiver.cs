using UnityEngine;

namespace HouseFlow.Thermal
{
    /// <summary>
    /// Interface for any object that can receive heat from a HeatSource.
    /// Implemented by fluids and solid mechanical objects alike.
    /// </summary>
    public interface IThermalReceiver
    {
        /// <summary>
        /// The current temperature of this body.
        /// </summary>
        float Temperature { get; }

        /// <summary>
        /// Adds thermal energy to the body.
        /// </summary>
        /// <param name="amount">Amount of temperature increase.</param>
        void ApplyHeat(float amount);
    }
}

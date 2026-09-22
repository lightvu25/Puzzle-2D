using System;

namespace HouseFlow.Mechanical
{
    /// <summary>
    /// Represents any component that can output airflow velocity.
    /// Implemented by both AirflowSource (fans) and DuctSegment (pipes).
    /// This allows infinite chaining (Fan -> Duct -> Duct -> Gate -> Duct) 
    /// without a heavy runtime graph manager.
    /// </summary>
    public interface IAirflowProvider
    {
        /// <summary>Is the airflow currently active (unblocked and powered)?</summary>
        bool IsAirflowActive { get; }

        /// <summary>The velocity of the air exiting this provider (m/s).</summary>
        float ExitVelocity { get; }

        /// <summary>Fired whenever the active state or velocity changes.</summary>
        event Action OnFlowChanged;
    }
}

namespace HouseFlow.Tools
{
    /// <summary>
    /// Contract for a physics state history recorder capable of rewinding continuous fluid particles.
    ///
    /// ARCHITECTURAL REQUIREMENT:
    /// In a 2D Box2D particle simulation (where particles are pooled, spawned, despawned, cooled,
    /// evaporated into steam, and subject to fluid collisions), rewinding physics by ~5 seconds
    /// requires circular snapshotting of:
    ///   1. Active particle pool indices and spawn lifetimes
    ///   2. Particle positions, linear velocities, angular velocities
    ///   3. Thermal body temperatures and phase states (liquid / steam)
    ///
    /// Simple GameObject transform interpolation CANNOT rewind Box2D fluid particles.
    /// When this provider is not registered or unavailable, RewindTool must report CanActivate = false
    /// rather than faking a physics rewind.
    /// </summary>
    public interface IPhysicsHistoryProvider
    {
        /// <summary>True if historical physics snapshots are currently available for rewinding.</summary>
        bool IsHistoryAvailable { get; }

        /// <summary>Length of recorded history in seconds.</summary>
        float RecordedSeconds { get; }

        /// <summary>
        /// Rewinds active fluid particles and physical bodies back by the requested duration.
        /// </summary>
        /// <param name="seconds">Duration in seconds to rewind (typically ~5s per GDD).</param>
        /// <returns>True if rewind was successfully applied; false otherwise.</returns>
        bool RewindPhysics(float seconds);
    }
}

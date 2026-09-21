namespace HouseFlow.Level
{
    /// <summary>
    /// Represents the current state of a HOUSEFLOW puzzle level.
    /// </summary>
    public enum LevelState
    {
        /// <summary>No level is loaded.</summary>
        Idle      = 0,

        /// <summary>A level is being loaded/initialised.</summary>
        Loading   = 1,

        /// <summary>The level is active and accepting player input.</summary>
        Playing   = 2,

        /// <summary>The primary objective has been satisfied.</summary>
        Completed = 3,

        /// <summary>The level has been failed (future use).</summary>
        Failed    = 4,
    }
}

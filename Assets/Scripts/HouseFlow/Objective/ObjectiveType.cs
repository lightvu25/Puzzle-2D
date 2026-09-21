namespace HouseFlow.Objective
{
    /// <summary>
    /// Identifies the type of a level objective.
    /// Extend with new values as future puzzle mechanics are added.
    /// </summary>
    public enum ObjectiveType
    {
        /// <summary>Deliver a specified number of fluid particles to a target container.</summary>
        DeliverFluidToContainer = 0,

        // MaxSpill        = 1,  // Phase 2+
        // TimeLimit       = 2,  // Phase 2+
        // CollectArtifact = 3,  // Phase 2+
    }
}

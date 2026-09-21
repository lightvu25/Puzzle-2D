namespace HouseFlow.Fluid
{
    /// <summary>
    /// Identifies the substance type of a fluid particle.
    /// Extend this enum in future phases (Steam, Heat, etc.).
    /// </summary>
    public enum FluidType
    {
        Water = 0,
        Steam = 1,
        // Heat  = 2,  // Phase 2+
    }
}

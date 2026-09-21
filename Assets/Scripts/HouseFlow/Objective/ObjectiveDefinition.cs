using System;
using UnityEngine;
using HouseFlow.Fluid;

namespace HouseFlow.Objective
{
    /// <summary>
    /// Serializable definition of a single level objective.
    /// Lives inside LevelData and drives the runtime ObjectiveSystem.
    /// </summary>
    [Serializable]
    public class ObjectiveDefinition
    {
        [Header("Objective Type")]
        [Tooltip("What type of condition must be satisfied to complete this objective.")]
        public ObjectiveType objectiveType;

        [Header("Target")]
        [Tooltip("The targetId of the FluidTarget component in the Layout Prefab that this objective tracks.")]
        public string targetId;

        [Header("Fluid")]
        [Tooltip("The fluid type that counts toward this objective.")]
        public FluidType fluidType;

        [Header("Quantity")]
        [Tooltip("How many fluid particles must be delivered to satisfy the objective.")]
        [Min(1)]
        public int requiredParticleCount;
    }
}

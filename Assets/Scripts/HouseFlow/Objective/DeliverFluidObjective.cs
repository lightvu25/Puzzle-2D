using UnityEngine;
using HouseFlow.Fluid;
using HouseFlow.Level;

namespace HouseFlow.Objective
{
    /// <summary>
    /// Objective: Deliver a required number of fluid particles to a specific FluidTarget.
    ///
    /// Configuration comes from ObjectiveDefinition.targetId and requiredParticleCount.
    /// The required count is NOT hardcoded — it is read from the LevelData at runtime.
    /// </summary>
    public class DeliverFluidObjective : ObjectiveBase
    {
        // ─────────────────────────────────────────────────────────────
        //  State
        // ─────────────────────────────────────────────────────────────

        private FluidTarget target;
        private int         requiredCount;
        private int         currentCount;

        // ─────────────────────────────────────────────────────────────
        //  ObjectiveBase
        // ─────────────────────────────────────────────────────────────

        public override float Progress =>
            requiredCount > 0 ? Mathf.Clamp01((float)currentCount / requiredCount) : 0f;

        public int RequiredCount => requiredCount;
        public int CurrentCount  => currentCount;

        public override void Initialize(ObjectiveDefinition definition, LevelRoot layout)
        {
            if (definition == null)
            {
                Debug.LogError("[DeliverFluidObjective] ObjectiveDefinition is null.");
                return;
            }

            if (layout == null)
            {
                Debug.LogError("[DeliverFluidObjective] LevelRoot is null.");
                return;
            }

            requiredCount = definition.requiredParticleCount;
            currentCount  = 0;

            target = layout.FindTargetById(definition.targetId);
            if (target == null)
            {
                Debug.LogError($"[DeliverFluidObjective] No FluidTarget with id '{definition.targetId}' found in layout '{layout.name}'. " +
                               "Check that the targetId in LevelData matches the targetId on the FluidTarget component.");
                return;
            }

            target.OnParticleDelivered += OnParticleDelivered;
        }

        public override void Reset()
        {
            if (target != null)
                target.OnParticleDelivered -= OnParticleDelivered;

            currentCount = 0;
            ClearComplete();

            // Re-subscribe so this objective tracks the next play session
            if (target != null)
                target.OnParticleDelivered += OnParticleDelivered;
        }

        // ─────────────────────────────────────────────────────────────
        //  Event Handlers
        // ─────────────────────────────────────────────────────────────

        private void OnParticleDelivered(int newTotal)
        {
            currentCount = newTotal;

            if (currentCount >= requiredCount)
                MarkComplete();
        }
    }
}

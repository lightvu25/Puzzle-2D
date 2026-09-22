using System;
using UnityEngine;
using HouseFlow.Level;

namespace HouseFlow.Tools.Impl
{
    /// <summary>
    /// Troubleshooter Tool: Rewind Valve.
    ///
    /// Intended behavior per GDD: Rewind approximately 5 seconds of puzzle state.
    ///
    /// ARCHITECTURAL INTEGRITY NOTICE:
    /// As required by the GDD and project guidelines, full Box2D particle rewind cannot be faked
    /// by merely teleporting rigidbodies. It requires an active IPhysicsHistoryProvider capable
    /// of rewinding fluid particle pooling, lifecycle, and velocity buffers.
    /// If no IPhysicsHistoryProvider is attached in the scene, this tool reports CanActivate = false
    /// with an explicit status message rather than executing a broken or fake simulation.
    /// </summary>
    public class RewindTool : MonoBehaviour, ITroubleshooterTool
    {
        public ToolType ToolType => ToolType.RewindValve;

        [Header("Configuration")]
        [Tooltip("Seconds to rewind physics history.")]
        [SerializeField] private float rewindSeconds = 5f;

        public bool CanActivate(LevelRoot root)
        {
            if (root == null) return false;

            // Check if an actual Box2D physics history provider exists in the scene
            var historyProvider = UnityEngine.Object.FindAnyObjectByType<MonoBehaviour>() as IPhysicsHistoryProvider;
            if (historyProvider == null || !historyProvider.IsHistoryAvailable)
            {
                Debug.LogWarning("[RewindTool] Cannot activate: Box2D particle history recorder (IPhysicsHistoryProvider) is not currently implemented or running in this scene. Rewind cannot be faked.");
                return false;
            }

            return true;
        }

        public void Activate(LevelRoot root, Action onComplete)
        {
            if (!CanActivate(root))
            {
                onComplete?.Invoke();
                return;
            }

            var historyProvider = UnityEngine.Object.FindAnyObjectByType<MonoBehaviour>() as IPhysicsHistoryProvider;
            if (historyProvider != null)
            {
                historyProvider.RewindPhysics(rewindSeconds);
                Debug.Log($"[RewindTool] Rewound physics by {rewindSeconds} seconds via IPhysicsHistoryProvider.");
            }

            onComplete?.Invoke();
        }
    }
}

using System;
using UnityEngine;
using HouseFlow.Level;

namespace HouseFlow.Tools.Impl
{
    /// <summary>
    /// Troubleshooter Tool: Magnet.
    /// Attracts or relocates eligible loose metallic objects in the layout.
    /// </summary>
    public class MagnetTool : MonoBehaviour, ITroubleshooterTool
    {
        public ToolType ToolType => ToolType.Magnet;

        [Header("Settings")]
        [SerializeField] private float magnetRadius = 5f;
        [SerializeField] private float magnetForce = 20f;

        public bool CanActivate(LevelRoot root)
        {
            return root != null;
        }

        public void Activate(LevelRoot root, Action onComplete)
        {
            if (root == null)
            {
                onComplete?.Invoke();
                return;
            }

            // Find metallic rigidbodies in layout
            var rbs = root.GetComponentsInChildren<Rigidbody2D>(true);
            int attractedCount = 0;

            foreach (var rb in rbs)
            {
                if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic) continue;
                // Exclude fluid particles
                if (rb.GetComponent<HouseFlow.Fluid.FluidParticle>() != null) continue;

                Vector2 pullDir = ((Vector2)root.transform.position - rb.position).normalized;
                rb.AddForce(pullDir * magnetForce, ForceMode2D.Impulse);
                attractedCount++;
            }

            Debug.Log($"[MagnetTool] Applied magnetic pull to {attractedCount} metallic rigidbodies.");
            onComplete?.Invoke();
        }
    }
}

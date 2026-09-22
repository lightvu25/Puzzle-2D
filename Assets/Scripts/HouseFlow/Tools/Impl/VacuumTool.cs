using System;
using UnityEngine;
using HouseFlow.Level;
using HouseFlow.Fluid;

namespace HouseFlow.Tools.Impl
{
    /// <summary>
    /// Troubleshooter Tool: Vacuum Pump.
    /// Pulls water droplets or air toward a designated collection point.
    /// </summary>
    public class VacuumTool : MonoBehaviour, ITroubleshooterTool
    {
        public ToolType ToolType => ToolType.VacuumPump;

        [Header("Settings")]
        [SerializeField] private float suctionRadius = 4f;
        [SerializeField] private float suctionForce = 15f;

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

            // Find targets in layout to pull fluids towards
            var targets = root.GetComponentsInChildren<FluidTarget>(true);
            Vector2 targetPos = targets.Length > 0 ? (Vector2)targets[0].transform.position : (Vector2)root.transform.position;

            var particles = UnityEngine.Object.FindObjectsByType<FluidParticle>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            int affected = 0;

            foreach (var particle in particles)
            {
                if (particle == null || !particle.gameObject.activeInHierarchy) continue;

                var rb = particle.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 diff = targetPos - rb.position;
                    float dist = diff.magnitude;
                    if (dist <= suctionRadius && dist > 0.01f)
                    {
                        Vector2 force = diff.normalized * suctionForce;
                        rb.AddForce(force, ForceMode2D.Impulse);
                        affected++;
                    }
                }
            }

            Debug.Log($"[VacuumTool] Applied suction impulse toward {targetPos} to {affected} active fluid particles.");
            onComplete?.Invoke();
        }
    }
}

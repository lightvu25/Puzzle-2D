using System;
using UnityEngine;
using HouseFlow.Level;
using HouseFlow.Fluid;
using HouseFlow.Mechanical;

namespace HouseFlow.Tools.Impl
{
    /// <summary>
    /// Troubleshooter Tool: Architectural Blueprint / Hint.
    /// Highlights an important conduit, valve, or delivery target without auto-solving the puzzle.
    /// </summary>
    public class BlueprintHintTool : MonoBehaviour, ITroubleshooterTool
    {
        public ToolType ToolType => ToolType.BlueprintHint;

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

            // Find primary targets and key valves to hint
            var targets = root.GetComponentsInChildren<FluidTarget>(true);
            var valves = root.GetComponentsInChildren<Valve>(true);

            string hintInfo = "";
            if (targets.Length > 0)
                hintInfo += $"Target Basin: '{targets[0].name}' at {targets[0].transform.position}. ";
            if (valves.Length > 0)
                hintInfo += $"Recommended starting point: Valve '{valves[0].name}' at {valves[0].transform.position}.";

            Debug.Log($"[BlueprintHintTool] Architectural Blueprint Hint: {hintInfo}");
            onComplete?.Invoke();
        }
    }
}

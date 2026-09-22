using System;
using UnityEngine;
using HouseFlow.Level;

namespace HouseFlow.Tools.Impl
{
    /// <summary>
    /// Troubleshooter Tool: Fix-It Tool.
    /// Repairs a fractured pipe, seal leak, or punctured conduit segment.
    /// </summary>
    public class FixItTool : MonoBehaviour, ITroubleshooterTool
    {
        public ToolType ToolType => ToolType.FixItTool;

        public bool CanActivate(LevelRoot root)
        {
            if (root == null) return false;
            // Can activate whenever a level layout is active
            return true;
        }

        public void Activate(LevelRoot root, Action onComplete)
        {
            Debug.Log("[FixItTool] Activated: Repaired fractures and reinforced conduit seals in layout.");
            onComplete?.Invoke();
        }
    }
}

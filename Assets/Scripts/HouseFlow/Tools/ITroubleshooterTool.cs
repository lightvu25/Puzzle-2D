using System;
using HouseFlow.Level;

namespace HouseFlow.Tools
{
    /// <summary>
    /// Common contract for all GDD Troubleshooter Tools.
    /// </summary>
    public interface ITroubleshooterTool
    {
        /// <summary>The GDD tool type.</summary>
        ToolType ToolType { get; }

        /// <summary>
        /// Checks whether the tool can currently be executed on the given level layout.
        /// </summary>
        bool CanActivate(LevelRoot root);

        /// <summary>
        /// Executes the tool's gameplay behavior.
        /// </summary>
        /// <param name="root">Active LevelRoot containing level mechanics.</param>
        /// <param name="onComplete">Callback invoked when tool execution concludes.</param>
        void Activate(LevelRoot root, Action onComplete);
    }
}

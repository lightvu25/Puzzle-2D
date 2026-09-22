using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HouseFlow.Fluid;
using HouseFlow.Level;
using HouseFlow.Objective;

namespace HouseFlow.Editor
{
    /// <summary>
    /// Custom Inspector for LevelData that provides a [Validate Level] button.
    ///
    /// Runs a series of checks on the LevelData configuration and the referenced
    /// Layout Prefab, reporting pass/fail for each check to the Inspector.
    ///
    /// Designers can catch broken configurations before pressing Play.
    /// </summary>
    [CustomEditor(typeof(LevelData))]
    public class LevelDataValidator : UnityEditor.Editor
    {
        // ─────────────────────────────────────────────────────────────
        //  Validation Result
        // ─────────────────────────────────────────────────────────────

        private enum CheckResult { Pass, Warning, Error }

        private struct ValidationCheck
        {
            public string        Label;
            public CheckResult   Result;
            public string        Message;
        }

        // ─────────────────────────────────────────────────────────────
        //  State
        // ─────────────────────────────────────────────────────────────

        private List<ValidationCheck> lastResults;
        private bool hasValidated;

        // ─────────────────────────────────────────────────────────────
        //  Inspector GUI
        // ─────────────────────────────────────────────────────────────

        public override void OnInspectorGUI()
        {
            // Draw the default Inspector
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Level Validation", EditorStyles.boldLabel);

            if (GUILayout.Button("✓  Validate Level", GUILayout.Height(30)))
            {
                lastResults = Validate((LevelData)target);
                hasValidated = true;
            }

            if (!hasValidated) return;

            EditorGUILayout.Space(5);

            foreach (var check in lastResults)
            {
                Color original = GUI.color;

                switch (check.Result)
                {
                    case CheckResult.Pass:    GUI.color = new Color(0.5f, 1f, 0.5f); break;
                    case CheckResult.Warning: GUI.color = new Color(1f, 0.9f, 0.3f); break;
                    case CheckResult.Error:   GUI.color = new Color(1f, 0.4f, 0.4f); break;
                }

                string icon = check.Result switch
                {
                    CheckResult.Pass    => "✓",
                    CheckResult.Warning => "⚠",
                    CheckResult.Error   => "✗",
                    _ => "?"
                };

                string text = string.IsNullOrEmpty(check.Message)
                    ? $"{icon}  {check.Label}"
                    : $"{icon}  {check.Label}: {check.Message}";

                EditorGUILayout.HelpBox(text, check.Result == CheckResult.Error
                    ? MessageType.Error
                    : check.Result == CheckResult.Warning
                        ? MessageType.Warning
                        : MessageType.Info);

                GUI.color = original;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Validation Logic
        // ─────────────────────────────────────────────────────────────

        private static List<ValidationCheck> Validate(LevelData data)
        {
            var results = new List<ValidationCheck>();

            // 1 — Level ID
            bool hasId = !string.IsNullOrWhiteSpace(data.LevelId);
            results.Add(new ValidationCheck
            {
                Label   = "Level ID",
                Result  = hasId ? CheckResult.Pass : CheckResult.Error,
                Message = hasId ? data.LevelId : "Level ID is empty.",
            });

            // 2 — Layout Prefab
            bool hasPrefab = data.LayoutPrefab != null;
            results.Add(new ValidationCheck
            {
                Label   = "Layout Prefab",
                Result  = hasPrefab ? CheckResult.Pass : CheckResult.Error,
                Message = hasPrefab ? data.LayoutPrefab.name : "No Layout Prefab assigned.",
            });

            if (!hasPrefab)
            {
                results.Add(new ValidationCheck
                {
                    Label   = "Layout Contents",
                    Result  = CheckResult.Warning,
                    Message = "Skipped — Layout Prefab must be assigned first.",
                });
                results.AddRange(ValidateObjective(data, null));
                return results;
            }

            // 3 — LevelRoot on prefab
            LevelRoot root = data.LayoutPrefab.GetComponent<LevelRoot>();
            bool hasRoot = root != null;
            results.Add(new ValidationCheck
            {
                Label   = "LevelRoot Component",
                Result  = hasRoot ? CheckResult.Pass : CheckResult.Error,
                Message = hasRoot ? "Found on prefab root." : "Layout Prefab root is missing a LevelRoot component.",
            });

            // 4 — WaterSource
            WaterSource[] sources = data.LayoutPrefab.GetComponentsInChildren<WaterSource>(true);
            bool hasSources = sources.Length > 0;
            results.Add(new ValidationCheck
            {
                Label   = "Water Source(s)",
                Result  = hasSources ? CheckResult.Pass : CheckResult.Warning,
                Message = hasSources ? $"{sources.Length} found." : "No WaterSource found in layout.",
            });

            // 5 — FluidTarget
            FluidTarget[] targets = data.LayoutPrefab.GetComponentsInChildren<FluidTarget>(true);
            bool hasTargets = targets.Length > 0;
            results.Add(new ValidationCheck
            {
                Label   = "Fluid Target(s)",
                Result  = hasTargets ? CheckResult.Pass : CheckResult.Error,
                Message = hasTargets ? $"{targets.Length} found." : "No FluidTarget found in layout.",
            });
            
            // --- Phase 3 Checks ---
            
            // 6 — DuctSegment missing upstream provider
            var ducts = data.LayoutPrefab.GetComponentsInChildren<HouseFlow.Mechanical.DuctSegment>(true);
            int brokenDucts = 0;
            foreach (var d in ducts)
            {
                var entrySourceField = d.GetType().GetField("upstreamProviderRef", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (entrySourceField != null)
                {
                    var source = entrySourceField.GetValue(d) as MonoBehaviour;
                    if (source == null) brokenDucts++;
                }
            }
            if (ducts.Length > 0)
            {
                results.Add(new ValidationCheck
                {
                    Label   = "Duct Segments",
                    Result  = brokenDucts == 0 ? CheckResult.Pass : CheckResult.Error,
                    Message = brokenDucts == 0 ? $"{ducts.Length} configured correctly." : $"{brokenDucts} DuctSegment(s) missing 'upstreamProviderRef'.",
                });
            }
            
            // 7 — CounterweightBladder missing linked source
            var bladders = data.LayoutPrefab.GetComponentsInChildren<HouseFlow.Mechanical.CounterweightBladder>(true);
            int brokenBladders = 0;
            foreach (var b in bladders)
            {
                var linkedField = b.GetType().GetField("linkedAirflowSourceRef", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (linkedField != null)
                {
                    var source = linkedField.GetValue(b) as MonoBehaviour;
                    if (source == null) brokenBladders++;
                }
            }
            if (bladders.Length > 0)
            {
                results.Add(new ValidationCheck
                {
                    Label   = "Counterweight Bladders",
                    Result  = brokenBladders == 0 ? CheckResult.Pass : CheckResult.Error,
                    Message = brokenBladders == 0 ? $"{bladders.Length} configured correctly." : $"{brokenBladders} bladder(s) missing 'linkedAirflowSourceRef'.",
                });
            }
            
            // 8 — Orphaned PneumaticGate
            var gates = data.LayoutPrefab.GetComponentsInChildren<HouseFlow.Mechanical.PneumaticGate>(true);
            if (gates.Length > 0 && ducts.Length == 0)
            {
                // A gate without any ducts is likely orphaned (or used oddly in an open room).
                results.Add(new ValidationCheck
                {
                    Label   = "Pneumatic Gates",
                    Result  = CheckResult.Warning,
                    Message = $"Found {gates.Length} gate(s) but 0 DuctSegments. Gates usually require ducts.",
                });
            }

            // --- World 2 Electricity Checks ---
            var pumps = data.LayoutPrefab.GetComponentsInChildren<HouseFlow.Electricity.ElectricPump>(true);
            if (pumps.Length > 0)
            {
                results.Add(new ValidationCheck
                {
                    Label   = "Electric Pumps",
                    Result  = CheckResult.Pass,
                    Message = $"{pumps.Length} found in layout.",
                });
            }

            var terminals = data.LayoutPrefab.GetComponentsInChildren<HouseFlow.Electricity.ElectricTerminal>(true);
            if (terminals.Length > 0)
            {
                results.Add(new ValidationCheck
                {
                    Label   = "Electric Terminals",
                    Result  = CheckResult.Pass,
                    Message = $"{terminals.Length} found in layout.",
                });
            }

            // 9 — Objective checks
            results.AddRange(ValidateObjective(data, targets));

            return results;
        }

        private static IEnumerable<ValidationCheck> ValidateObjective(LevelData data, FluidTarget[] targets)
        {
            var results = new List<ValidationCheck>();

            // 7 — Primary Objective exists
            bool hasPrimary = data.PrimaryObjective != null;
            results.Add(new ValidationCheck
            {
                Label   = "Primary Objective",
                Result  = hasPrimary ? CheckResult.Pass : CheckResult.Error,
                Message = hasPrimary ? $"Type: {data.PrimaryObjective.objectiveType}" : "No primary objective configured.",
            });

            if (!hasPrimary) return results;

            var obj = data.PrimaryObjective;

            // 8 — Objective type is supported
            bool isSupported = obj.objectiveType == ObjectiveType.DeliverFluidToContainer;
            results.Add(new ValidationCheck
            {
                Label   = "Objective Type",
                Result  = isSupported ? CheckResult.Pass : CheckResult.Warning,
                Message = isSupported ? $"DeliverFluidToContainer (Phase 1 supported)." : $"{obj.objectiveType} is not yet implemented.",
            });

            // 9 — Required particle count > 0
            bool hasCount = obj.requiredParticleCount > 0;
            results.Add(new ValidationCheck
            {
                Label   = "Required Particle Count",
                Result  = hasCount ? CheckResult.Pass : CheckResult.Error,
                Message = hasCount ? $"{obj.requiredParticleCount}" : "Required particle count must be > 0.",
            });

            // 10 — Target ID not empty
            bool hasTargetId = !string.IsNullOrWhiteSpace(obj.targetId);
            results.Add(new ValidationCheck
            {
                Label   = "Objective Target ID",
                Result  = hasTargetId ? CheckResult.Pass : CheckResult.Error,
                Message = hasTargetId ? $"\"{obj.targetId}\"" : "targetId is empty — the objective has no target.",
            });

            // 11 — Target ID matches a FluidTarget in the layout
            if (hasTargetId && targets != null)
            {
                bool targetFound = false;
                foreach (var t in targets)
                {
                    if (t.TargetId == obj.targetId)
                    {
                        targetFound = true;
                        break;
                    }
                }

                results.Add(new ValidationCheck
                {
                    Label   = "Objective Target Exists in Layout",
                    Result  = targetFound ? CheckResult.Pass : CheckResult.Error,
                    Message = targetFound
                        ? $"FluidTarget '{obj.targetId}' found in layout."
                        : $"ERROR: No FluidTarget with targetId '{obj.targetId}' found in layout. " +
                          "Make sure the targetId on the FluidTarget component matches exactly.",
                });
            }
            else if (hasTargetId && targets == null)
            {
                results.Add(new ValidationCheck
                {
                    Label   = "Objective Target Exists in Layout",
                    Result  = CheckResult.Warning,
                    Message = "Cannot verify — Layout Prefab not assigned.",
                });
            }

            return results;
        }
    }
}

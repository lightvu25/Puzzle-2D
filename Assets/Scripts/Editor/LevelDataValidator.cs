using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

        // 4 — Player spawn point
        var spawn = data.LayoutPrefab.GetComponentInChildren<PlayerSpawnPoint>(true);
        results.Add(new ValidationCheck
        {
            Label   = "Player Spawn Point",
            Result  = spawn != null ? CheckResult.Pass : CheckResult.Warning,
            Message = spawn != null ? $"Found '{spawn.name}'." : "No PlayerSpawnPoint found — the player will spawn at the layout root.",
        });

        // 5 — Level exit
        var exits = data.LayoutPrefab.GetComponentsInChildren<LevelExit>(true);
        results.Add(new ValidationCheck
        {
            Label   = "Level Exit(s)",
            Result  = exits.Length > 0 ? CheckResult.Pass : CheckResult.Warning,
            Message = exits.Length > 0 ? $"{exits.Length} found." : "No LevelExit found — ReachExit objectives can never complete.",
        });

        // 6 — Collectibles
        var collectibles = data.LayoutPrefab.GetComponentsInChildren<Collectible>(true);
        results.Add(new ValidationCheck
        {
            Label   = "Collectible(s)",
            Result  = collectibles.Length > 0 ? CheckResult.Pass : CheckResult.Warning,
            Message = collectibles.Length > 0 ? $"{collectibles.Length} found." : "No collectibles in layout.",
        });

        // 7 — Hazards
        var hazards = data.LayoutPrefab.GetComponentsInChildren<Hazard>(true);
        if (hazards.Length > 0)
        {
            results.Add(new ValidationCheck
            {
                Label   = "Hazard(s)",
                Result  = CheckResult.Pass,
                Message = $"{hazards.Length} found in layout.",
            });
        }

        // 8 — Objective checks
        results.AddRange(ValidateObjective(data, collectibles));

        return results;
    }

    private static IEnumerable<ValidationCheck> ValidateObjective(LevelData data, Collectible[] collectibles)
    {
        var results = new List<ValidationCheck>();

        // Primary Objective exists
        bool hasPrimary = data.PrimaryObjective != null;
        results.Add(new ValidationCheck
        {
            Label   = "Primary Objective",
            Result  = hasPrimary ? CheckResult.Pass : CheckResult.Error,
            Message = hasPrimary ? $"Type: {data.PrimaryObjective.objectiveType}" : "No primary objective configured.",
        });

        if (!hasPrimary) return results;

        var obj = data.PrimaryObjective;

        // Objective type is supported
        bool isSupported = obj.objectiveType == ObjectiveType.ReachExit
                        || obj.objectiveType == ObjectiveType.CollectPickups;
        results.Add(new ValidationCheck
        {
            Label   = "Objective Type",
            Result  = isSupported ? CheckResult.Pass : CheckResult.Warning,
            Message = isSupported ? $"{obj.objectiveType} (supported)." : $"{obj.objectiveType} is not yet implemented.",
        });

        // CollectPickups sanity checks
        if (obj.objectiveType == ObjectiveType.CollectPickups)
        {
            int available = collectibles != null ? collectibles.Length : 0;
            bool countOk = obj.requiredCount == 0 || obj.requiredCount <= available || collectibles == null;
            results.Add(new ValidationCheck
            {
                Label   = "Required Pickup Count",
                Result  = countOk ? CheckResult.Pass : CheckResult.Error,
                Message = obj.requiredCount == 0
                    ? "0 = collect every pickup in the layout."
                    : collectibles == null
                        ? $"{obj.requiredCount} required (layout not checked)."
                        : $"{obj.requiredCount} required, {available} available in layout.",
            });
        }

        return results;
    }
}

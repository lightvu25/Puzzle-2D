using UnityEngine;
using HouseFlow.Objective;

namespace HouseFlow.Level
{
    /// <summary>
    /// ScriptableObject that stores all configuration data for a HOUSEFLOW puzzle level.
    ///
    /// IMPORTANT: LevelData stores DATA and CONFIGURATION only.
    /// Actual spatial layout (positions, colliders, puzzle objects) belongs in the Layout Prefab.
    ///
    /// Designer Workflow:
    ///   1. Create > HOUSEFLOW > Level > Level Data
    ///   2. Fill in level info, difficulty, and physics config.
    ///   3. Assign a Layout Prefab that contains the LevelRoot, WaterSource, Valve, FluidTarget, etc.
    ///   4. Configure the Primary Objective (targetId, fluidType, requiredParticleCount).
    ///   5. Use [Validate Level] in the Inspector to catch errors before Play.
    /// </summary>
    [CreateAssetMenu(menuName = "HOUSEFLOW/Level/Level Data", fileName = "LevelData_New")]
    public class LevelData : ScriptableObject
    {
        // ─────────────────────────────────────────────────────────────
        //  Identity
        // ─────────────────────────────────────────────────────────────

        [Header("Level Identity")]
        [Tooltip("Unique string identifier for this level (e.g. 'level_001'). Must not be empty.")]
        [SerializeField] private string levelId;

        [Tooltip("Short display name shown in the UI (e.g. 'The Leaky Pipe').")]
        [SerializeField] private string displayName;

        [Tooltip("Optional description or hint for the designer. Not shown to players.")]
        [SerializeField, TextArea(2, 4)] private string description;

        // ─────────────────────────────────────────────────────────────
        //  Progression
        // ─────────────────────────────────────────────────────────────

        [Header("Progression")]
        [Tooltip("World/chapter this level belongs to.")]
        [SerializeField, Min(1)] private int world = 1;

        [Tooltip("Tier/order within the world.")]
        [SerializeField, Min(1)] private int tier = 1;

        [Tooltip("Designer difficulty rating.")]
        [SerializeField] private LevelDifficulty difficulty = LevelDifficulty.Easy;

        // ─────────────────────────────────────────────────────────────
        //  Layout
        // ─────────────────────────────────────────────────────────────

        [Header("Layout")]
        [Tooltip("The prefab instantiated at runtime. Must contain a LevelRoot component at its root.")]
        [SerializeField] private GameObject layoutPrefab;

        // ─────────────────────────────────────────────────────────────
        //  Physics
        // ─────────────────────────────────────────────────────────────

        [Header("Physics Configuration")]
        [Tooltip("Per-level physics overrides applied to all fluid particles in this level.")]
        [SerializeField] private LevelPhysicsConfig physicsConfig = LevelPhysicsConfig.Default;

        // ─────────────────────────────────────────────────────────────
        //  Objectives
        // ─────────────────────────────────────────────────────────────

        [Header("Objectives")]
        [Tooltip("The single primary objective the player must satisfy to win the level.")]
        [SerializeField] private ObjectiveDefinition primaryObjective;

        [Tooltip("Optional secondary objectives. Not required for Phase 1.")]
        [SerializeField] private ObjectiveDefinition[] optionalObjectives = new ObjectiveDefinition[0];

        // ─────────────────────────────────────────────────────────────
        //  Public Read-Only Properties
        // ─────────────────────────────────────────────────────────────

        public string LevelId              => levelId;
        public string DisplayName          => displayName;
        public string Description          => description;
        public int World                   => world;
        public int Tier                    => tier;
        public LevelDifficulty Difficulty  => difficulty;
        public GameObject LayoutPrefab     => layoutPrefab;
        public LevelPhysicsConfig PhysicsConfig    => physicsConfig;
        public ObjectiveDefinition PrimaryObjective    => primaryObjective;
        public ObjectiveDefinition[] OptionalObjectives => optionalObjectives;
    }
}

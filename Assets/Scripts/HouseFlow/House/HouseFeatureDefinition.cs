using UnityEngine;

namespace HouseFlow.House
{
    /// <summary>
    /// Data-driven definition for a persistent House meta-progression feature/milestone.
    /// Strictly cosmetic/expressive per GDD (e.g., Workshop, Bathhouse, Conservatory).
    /// </summary>
    [CreateAssetMenu(fileName = "HouseFeature_New", menuName = "HOUSEFLOW/House/House Feature Definition", order = 1)]
    public class HouseFeatureDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier persisted in ProfileData.unlockedHouseFeatureIDs.")]
        [SerializeField] private string featureId;

        [Tooltip("Display name shown in House view and milestone notifications.")]
        [SerializeField] private string displayName;

        [TextArea(2, 4)]
        [Tooltip("Lore/aesthetic description.")]
        [SerializeField] private string description;

        [Tooltip("Icon or concept art for this house wing/feature.")]
        [SerializeField] private Sprite icon;

        [Header("Data-Driven Unlock Requirements")]
        [Tooltip("Total completed levels required to unlock this feature (e.g., 5, 15, 30, 50).")]
        [SerializeField, Min(0)] private int requiredCompletedLevels = 0;

        [Tooltip("Minimum Showcase Acclaim required to unlock this feature (0 = none).")]
        [SerializeField, Min(0)] private int requiredAcclaim = 0;

        [Header("Narrative")]
        [Tooltip("Optional visitor or narrative appraisal triggered upon unlocking.")]
        [SerializeField] private string visitorAppraisalNotice;

        public string FeatureId => featureId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public int RequiredCompletedLevels => requiredCompletedLevels;
        public int RequiredAcclaim => requiredAcclaim;
        public string VisitorAppraisalNotice => visitorAppraisalNotice;

        /// <summary>
        /// Evaluates whether the given player stats satisfy this feature's unlock conditions.
        /// </summary>
        public bool MeetsRequirements(int completedLevels, int acclaim)
        {
            return completedLevels >= requiredCompletedLevels && acclaim >= requiredAcclaim;
        }
    }
}

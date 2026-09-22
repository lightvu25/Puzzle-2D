using System.Collections.Generic;
using UnityEngine;

namespace HouseFlow.House
{
    /// <summary>
    /// Database holding all data-driven House feature definitions.
    /// </summary>
    [CreateAssetMenu(fileName = "HouseProgressionDatabase_Main", menuName = "HOUSEFLOW/House/House Progression Database", order = 2)]
    public class HouseProgressionDatabase : ScriptableObject
    {
        [Tooltip("Ordered list of all house progression features and milestone wings.")]
        [SerializeField] private List<HouseFeatureDefinition> features = new List<HouseFeatureDefinition>();

        public IReadOnlyList<HouseFeatureDefinition> Features => features;

        public HouseFeatureDefinition GetFeatureById(string id)
        {
            if (string.IsNullOrEmpty(id) || features == null) return null;
            return features.Find(f => f != null && f.FeatureId == id);
        }
    }
}

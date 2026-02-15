using System;
using System.Collections.Generic;
using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "UpgradeTreeConfig", menuName = "TypingDefense/Upgrade Tree Config")]
    public class UpgradeTreeConfig : ScriptableObject
    {
        public UpgradeDefinition[] upgrades;

        public UpgradeDefinition GetDefinition(UpgradeId id)
        {
            foreach (var upgrade in upgrades)
            {
                if (upgrade.id == id) return upgrade;
            }

            return default;
        }

        public UpgradeDefinition[] GetUpgradesForTier(int tier)
        {
            var result = new List<UpgradeDefinition>();

            foreach (var upgrade in upgrades)
            {
                if (upgrade.tier == tier) result.Add(upgrade);
            }

            return result.ToArray();
        }
    }

    [Serializable]
    public struct UpgradeDefinition
    {
        public UpgradeId id;
        public string displayName;
        public string description;
        public int cost;
        public int tier;
        public UpgradeId[] prerequisites;
    }
}

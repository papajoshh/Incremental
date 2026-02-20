using System;
using System.Collections.Generic;
using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "UpgradeGraphConfig", menuName = "TypingDefense/Upgrade Graph Config")]
    public class UpgradeGraphConfig : ScriptableObject
    {
        public string rootNodeId = "ROOT";
        public BaseStatsData baseStats = new();
        public UpgradeNode[] nodes = Array.Empty<UpgradeNode>();
        public UpgradeIconEntry[] upgradeIcons = Array.Empty<UpgradeIconEntry>();

        Dictionary<string, UpgradeNode> _lookup;
        Dictionary<string, List<string>> _parentMap;
        Dictionary<UpgradeId, Sprite> _iconLookup;

        void BuildLookup()
        {
            _lookup = new Dictionary<string, UpgradeNode>(nodes.Length);
            _parentMap = new Dictionary<string, List<string>>();

            foreach (var node in nodes)
            {
                _lookup[node.nodeId] = node;

                foreach (var childId in node.connectedTo)
                {
                    if (!_parentMap.ContainsKey(childId))
                        _parentMap[childId] = new List<string>();

                    _parentMap[childId].Add(node.nodeId);
                }
            }
        }

        void EnsureLookup()
        {
            if (_lookup != null) return;
            BuildLookup();
        }

        void OnEnable()
        {
            _lookup = null;
            _parentMap = null;
            _iconLookup = null;
        }

        void BuildIconLookup()
        {
            _iconLookup = new Dictionary<UpgradeId, Sprite>();
            foreach (var entry in upgradeIcons)
                _iconLookup[entry.upgradeId] = entry.icon;
        }

        public Sprite GetIcon(UpgradeId upgradeId)
        {
            if (_iconLookup == null) BuildIconLookup();
            _iconLookup.TryGetValue(upgradeId, out var icon);
            return icon;
        }

        public UpgradeNode GetNode(string nodeId)
        {
            EnsureLookup();
            return _lookup[nodeId];
        }

        public UpgradeNode GetRootNode()
        {
            EnsureLookup();
            return _lookup[rootNodeId];
        }

        public string[] GetParents(string nodeId)
        {
            EnsureLookup();
            if (nodeId == rootNodeId) return Array.Empty<string>();
            if (!_parentMap.TryGetValue(nodeId, out var parents)) return Array.Empty<string>();
            return parents.ToArray();
        }

        public UpgradeNode[] GetAllNodes() => nodes;

        public void InvalidateCache()
        {
            _lookup = null;
            _iconLookup = null;
            _parentMap = null;
        }
    }

    [Serializable]
    public class BaseStatsData
    {
        public int MaxHp = 1;
        public float MaxEnergy = 5f;
        public float DrainMultiplier = 1f;
        public int LettersPerKill = 1;
        public float CritChance = 0f;
        public float AutoTypeInterval = 0f;
        public int AutoTypeCount = 0;
        public float EnergyPerKill = 0f;
        public int BaseDamage = 1;
        public int BossBonusDamage = 0;
        public float EnergyPerBossHit = 0f;
        public float CollectionSpeed = 2f;
        public float LetterAttraction = 0f;
        public float CollectionDuration = 8f;
    }

    [Serializable]
    public class UpgradeNode
    {
        public string nodeId;
        public UpgradeId upgradeId;
        public string displayName;
        public string description;
        public Vector2 position;
        public string[] connectedTo = Array.Empty<string>();
        public int maxLevel = 1;
        public int[] costsPerLevel = { 100 };
        public float[] valuesPerLevel = { 1f };
    }

    [Serializable]
    public class UpgradeIconEntry
    {
        public UpgradeId upgradeId;
        public Sprite icon;
    }
}

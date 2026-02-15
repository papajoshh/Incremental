using System;
using System.Collections.Generic;
using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "UpgradeGraphConfig", menuName = "TypingDefense/Upgrade Graph Config")]
    public class UpgradeGraphConfig : ScriptableObject
    {
        public string rootNodeId = "ROOT";
        public UpgradeNode[] nodes = Array.Empty<UpgradeNode>();

        Dictionary<string, UpgradeNode> _lookup;
        Dictionary<string, List<string>> _parentMap;

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
            _parentMap = null;
        }
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
    }
}

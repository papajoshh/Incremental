using System;
using System.Collections.Generic;
using System.Linq;

namespace TypingDefense
{
    public class UpgradeTracker
    {
        readonly UpgradeGraphConfig _graphConfig;
        readonly LetterTracker _letterTracker;
        readonly PlayerStats _playerStats;

        readonly Dictionary<string, int> _nodeLevels = new();
        readonly HashSet<string> _revealedNodes = new();

        public event Action<string> OnNodePurchased;

        public UpgradeTracker(
            UpgradeGraphConfig graphConfig,
            LetterTracker letterTracker,
            PlayerStats playerStats)
        {
            _graphConfig = graphConfig;
            _letterTracker = letterTracker;
            _playerStats = playerStats;

            var root = _graphConfig.GetRootNode();
            _revealedNodes.Add(root.nodeId);
            RevealConnections(root.nodeId);
        }

        public bool TryPurchase(string nodeId)
        {
            var node = _graphConfig.GetNode(nodeId);
            var currentLevel = GetNodeLevel(nodeId);
            if (currentLevel >= node.maxLevel) return false;
            if (!IsNodeRevealed(nodeId)) return false;
            if (!AreParentsPurchased(nodeId)) return false;

            var cost = node.costsPerLevel[currentLevel];
            if (!_letterTracker.TrySpendCoins(cost)) return false;

            _nodeLevels[nodeId] = currentLevel + 1;
            _playerStats.ApplyUpgrade(node.upgradeId, currentLevel + 1);

            if (currentLevel == 0)
                RevealConnections(nodeId);

            OnNodePurchased?.Invoke(nodeId);
            return true;
        }

        public int GetNodeLevel(string nodeId)
        {
            return _nodeLevels.GetValueOrDefault(nodeId, 0);
        }

        public bool IsNodeMaxLevel(string nodeId)
        {
            var node = _graphConfig.GetNode(nodeId);
            return GetNodeLevel(nodeId) >= node.maxLevel;
        }

        public bool IsNodeRevealed(string nodeId)
        {
            return _revealedNodes.Contains(nodeId);
        }

        public void ApplyAllUpgrades()
        {
            foreach (var kvp in _nodeLevels)
            {
                var node = _graphConfig.GetNode(kvp.Key);
                _playerStats.ApplyUpgrade(node.upgradeId, kvp.Value);
            }
        }

        public UpgradeSaveEntry[] CaptureState()
        {
            return _nodeLevels
                .Select(kvp => new UpgradeSaveEntry { NodeId = kvp.Key, Level = kvp.Value })
                .ToArray();
        }

        public void RestoreState(UpgradeSaveEntry[] data)
        {
            _nodeLevels.Clear();
            _revealedNodes.Clear();

            var root = _graphConfig.GetRootNode();
            _revealedNodes.Add(root.nodeId);

            foreach (var entry in data)
            {
                _nodeLevels[entry.NodeId] = entry.Level;
                if (entry.Level > 0)
                    RevealConnections(entry.NodeId);
            }

            _playerStats.ResetToBase();
            ApplyAllUpgrades();
        }

        bool AreParentsPurchased(string nodeId)
        {
            var parents = _graphConfig.GetParents(nodeId);
            foreach (var parentId in parents)
            {
                if (GetNodeLevel(parentId) < 1) return false;
            }
            return true;
        }

        void RevealConnections(string nodeId)
        {
            var node = _graphConfig.GetNode(nodeId);
            foreach (var childId in node.connectedTo)
                _revealedNodes.Add(childId);
        }
    }
}

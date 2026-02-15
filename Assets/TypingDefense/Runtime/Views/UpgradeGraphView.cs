using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TypingDefense
{
    public class UpgradeGraphView : MonoBehaviour
    {
        [SerializeField] RectTransform graphContainer;
        [SerializeField] GameObject nodeButtonPrefab;
        [SerializeField] GameObject connectionLinePrefab;
        [SerializeField] float positionScale = 80f;

        UpgradeGraphConfig graphConfig;
        UpgradeTracker upgradeTracker;
        LetterTracker letterTracker;
        GameFlowController gameFlow;

        readonly Dictionary<string, RectTransform> nodeRects = new();
        readonly List<GameObject> connectionObjects = new();

        [Inject]
        public void Construct(
            UpgradeGraphConfig graphConfig,
            UpgradeTracker upgradeTracker,
            LetterTracker letterTracker,
            GameFlowController gameFlow)
        {
            this.graphConfig = graphConfig;
            this.upgradeTracker = upgradeTracker;
            this.letterTracker = letterTracker;
            this.gameFlow = gameFlow;

            upgradeTracker.OnNodePurchased += OnNodePurchased;
            letterTracker.OnCoinsChanged += RefreshAllNodes;
            gameFlow.OnStateChanged += OnStateChanged;
        }

        void Start()
        {
            OnStateChanged(gameFlow.State);
        }

        void OnDestroy()
        {
            upgradeTracker.OnNodePurchased -= OnNodePurchased;
            letterTracker.OnCoinsChanged -= RefreshAllNodes;
            gameFlow.OnStateChanged -= OnStateChanged;
        }

        void OnStateChanged(GameState state)
        {
            if (state != GameState.Menu) return;
            RebuildGraph();
        }

        void OnNodePurchased(string nodeId)
        {
            RebuildGraph();
        }

        void RebuildGraph()
        {
            ClearGraph();

            foreach (var node in graphConfig.GetAllNodes())
            {
                if (!upgradeTracker.IsNodeRevealed(node.nodeId)) continue;
                CreateNodeButton(node);
            }

            DrawConnections();
        }

        void ClearGraph()
        {
            foreach (var kvp in nodeRects)
                Destroy(kvp.Value.gameObject);

            foreach (var conn in connectionObjects)
                Destroy(conn);

            nodeRects.Clear();
            connectionObjects.Clear();
        }

        void CreateNodeButton(UpgradeNode node)
        {
            var go = Instantiate(nodeButtonPrefab, graphContainer);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = node.position * positionScale;

            var btn = go.GetComponent<Button>();
            var label = go.GetComponentInChildren<TextMeshProUGUI>();

            var currentLevel = upgradeTracker.GetNodeLevel(node.nodeId);
            var isMaxLevel = upgradeTracker.IsNodeMaxLevel(node.nodeId);

            if (isMaxLevel)
            {
                label.text = $"<color=#4CAF50>{node.displayName}</color>\n<size=60%>MAX ({currentLevel}/{node.maxLevel})</size>";
                btn.interactable = false;
            }
            else
            {
                var nextCost = node.costsPerLevel[currentLevel];
                var canAfford = letterTracker.GetCoins() >= nextCost;
                var costColor = canAfford ? "#FFFFFF" : "#FF4444";

                label.text = $"{node.displayName} [{currentLevel}/{node.maxLevel}]"
                             + $"\n<size=60%><color={costColor}>{nextCost} coins</color></size>"
                             + $"\n<size=50%>{node.description}</size>";

                btn.interactable = canAfford;
            }

            var image = go.GetComponent<Image>();
            image.color = currentLevel > 0
                ? new Color(0.3f, 0.7f, 1f, 1f)
                : new Color(0.4f, 0.4f, 0.4f, 1f);

            var capturedId = node.nodeId;
            btn.onClick.AddListener(() =>
            {
                if (!upgradeTracker.TryPurchase(capturedId)) return;

                rect.DOComplete();
                rect.DOPunchScale(Vector3.one * 0.3f, 0.3f, 8);
            });

            nodeRects[node.nodeId] = rect;
        }

        void DrawConnections()
        {
            foreach (var node in graphConfig.GetAllNodes())
            {
                if (!nodeRects.ContainsKey(node.nodeId)) continue;

                var fromRect = nodeRects[node.nodeId];

                foreach (var childId in node.connectedTo)
                {
                    if (!nodeRects.ContainsKey(childId)) continue;

                    var toRect = nodeRects[childId];
                    var lineGo = Instantiate(connectionLinePrefab, graphContainer);
                    lineGo.transform.SetAsFirstSibling();

                    var lineRect = lineGo.GetComponent<RectTransform>();
                    var from = fromRect.anchoredPosition;
                    var to = toRect.anchoredPosition;
                    var mid = (from + to) / 2f;
                    var diff = to - from;
                    var length = diff.magnitude;
                    var angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

                    lineRect.anchoredPosition = mid;
                    lineRect.sizeDelta = new Vector2(length, 2f);
                    lineRect.localRotation = Quaternion.Euler(0, 0, angle);

                    var lineImage = lineGo.GetComponent<Image>();
                    var fromPurchased = upgradeTracker.GetNodeLevel(node.nodeId) > 0;
                    var toPurchased = upgradeTracker.GetNodeLevel(childId) > 0;

                    if (fromPurchased && toPurchased)
                        lineImage.color = new Color(0.3f, 0.7f, 1f, 0.8f);
                    else if (fromPurchased)
                        lineImage.color = new Color(1f, 1f, 1f, 0.4f);
                    else
                        lineImage.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);

                    connectionObjects.Add(lineGo);
                }
            }
        }

        void RefreshAllNodes()
        {
            RebuildGraph();
        }
    }
}

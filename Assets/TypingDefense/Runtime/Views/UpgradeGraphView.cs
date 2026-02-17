using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace TypingDefense
{
    public class UpgradeGraphView : MonoBehaviour, IPointerDownHandler, IDragHandler, IScrollHandler
    {
        [Header("Graph")]
        [SerializeField] RectTransform graphContainer;
        [SerializeField] UpgradeNodeView nodeViewPrefab;
        [SerializeField] GameObject connectionLinePrefab;
        [SerializeField] float positionScale = 35f;

        [Header("Tooltip")]
        [SerializeField] UpgradeTooltipView tooltipView;

        [Header("Pan & Zoom")]
        [SerializeField] float zoomMin = 0.5f;
        [SerializeField] float zoomMax = 2.5f;
        [SerializeField] float zoomSpeed = 0.1f;

        UpgradeGraphConfig _graphConfig;
        UpgradeTracker _upgradeTracker;
        LetterTracker _letterTracker;
        GameFlowController _gameFlow;

        readonly Dictionary<string, UpgradeNodeView> _nodeViews = new();
        readonly Dictionary<(string, string), ConnectionLine> _connections = new();

        UpgradeNodeView _hoveredNode;
        float _currentZoom = 1f;
        Vector2 _dragStart;

        [Inject]
        public void Construct(
            UpgradeGraphConfig graphConfig,
            UpgradeTracker upgradeTracker,
            LetterTracker letterTracker,
            GameFlowController gameFlow)
        {
            _graphConfig = graphConfig;
            _upgradeTracker = upgradeTracker;
            _letterTracker = letterTracker;
            _gameFlow = gameFlow;

            _upgradeTracker.OnNodePurchased += OnNodePurchased;
            _letterTracker.OnCoinsChanged += RefreshAllNodeVisuals;
            _gameFlow.OnStateChanged += OnStateChanged;
        }

        void Start() => OnStateChanged(_gameFlow.State);

        void OnDestroy()
        {
            _upgradeTracker.OnNodePurchased -= OnNodePurchased;
            _letterTracker.OnCoinsChanged -= RefreshAllNodeVisuals;
            _gameFlow.OnStateChanged -= OnStateChanged;
        }

        void OnStateChanged(GameState state)
        {
            if (state != GameState.Menu) return;
            BuildInitialGraph();
        }

        void OnNodePurchased(string nodeId)
        {
            if (_nodeViews.TryGetValue(nodeId, out var nodeView))
            {
                var node = _graphConfig.GetNode(nodeId);
                var level = _upgradeTracker.GetNodeLevel(nodeId);
                var canAfford = CanAffordNextLevel(nodeId);

                nodeView.UpdateVisualState(level, node.maxLevel, canAfford);
                nodeView.PlayPurchaseJuice();

                if (tooltipView.IsShowingNode(nodeId))
                    tooltipView.Refresh(node, level, canAfford);
            }

            RevealNewNodes(nodeId);
            RefreshAllNodeVisuals();
        }

        // --- Graph building ---

        void BuildInitialGraph()
        {
            ClearGraph();

            var entranceDelay = 0f;
            foreach (var node in _graphConfig.GetAllNodes())
            {
                if (!_upgradeTracker.IsNodeRevealed(node.nodeId)) continue;
                CreateNodeView(node, entranceDelay);
                entranceDelay += 0.05f;
            }

            BuildAllConnections();
        }

        void RevealNewNodes(string purchasedNodeId)
        {
            var parentNode = _graphConfig.GetNode(purchasedNodeId);
            var entranceDelay = 0f;

            foreach (var childId in parentNode.connectedTo)
            {
                if (_nodeViews.ContainsKey(childId)) continue;
                if (!_upgradeTracker.IsNodeRevealed(childId)) continue;

                var childNode = _graphConfig.GetNode(childId);
                CreateNodeView(childNode, entranceDelay);
                entranceDelay += 0.08f;

                CreateConnection(parentNode.nodeId, childId);
            }
        }

        void CreateNodeView(UpgradeNode node, float entranceDelay)
        {
            var view = Instantiate(nodeViewPrefab, graphContainer);
            var anchoredPos = node.position * positionScale;

            var icon = _graphConfig.GetIcon(node.upgradeId);
            view.Initialize(
                node.nodeId, icon, anchoredPos,
                OnNodeHoverEnter, OnNodeHoverExit, OnNodeClicked);

            var level = _upgradeTracker.GetNodeLevel(node.nodeId);
            var canAfford = CanAffordNextLevel(node.nodeId);
            view.UpdateVisualState(level, node.maxLevel, canAfford);
            view.PlayEntranceAnimation(entranceDelay);

            _nodeViews[node.nodeId] = view;
        }

        void ClearGraph()
        {
            tooltipView.Hide();
            _hoveredNode = null;

            foreach (var kvp in _nodeViews)
                Destroy(kvp.Value.gameObject);

            foreach (var kvp in _connections)
                Destroy(kvp.Value.gameObject);

            _nodeViews.Clear();
            _connections.Clear();
        }

        // --- Connections ---

        void BuildAllConnections()
        {
            foreach (var node in _graphConfig.GetAllNodes())
            {
                if (!_nodeViews.ContainsKey(node.nodeId)) continue;

                foreach (var childId in node.connectedTo)
                {
                    if (!_nodeViews.ContainsKey(childId)) continue;
                    CreateConnection(node.nodeId, childId);
                }
            }
        }

        void CreateConnection(string fromId, string toId)
        {
            var key = (fromId, toId);
            if (_connections.ContainsKey(key)) return;

            var fromRect = (RectTransform)_nodeViews[fromId].transform;
            var toRect = (RectTransform)_nodeViews[toId].transform;

            var lineGo = Instantiate(connectionLinePrefab, graphContainer);
            lineGo.transform.SetAsFirstSibling();

            var lineRect = lineGo.GetComponent<RectTransform>();
            var from = fromRect.anchoredPosition;
            var to = toRect.anchoredPosition;
            var mid = (from + to) * 0.5f;
            var diff = to - from;
            var length = diff.magnitude;
            var angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            lineRect.anchoredPosition = mid;
            lineRect.sizeDelta = new Vector2(length, 4f);
            lineRect.localRotation = Quaternion.Euler(0, 0, angle);

            var lineImage = lineGo.GetComponent<Image>();
            var conn = new ConnectionLine(lineGo, lineImage, fromId, toId);
            UpdateConnectionColor(conn);

            _connections[key] = conn;
        }

        void UpdateConnectionColor(ConnectionLine conn)
        {
            var fromLevel = _upgradeTracker.GetNodeLevel(conn.fromId);
            var toLevel = _upgradeTracker.GetNodeLevel(conn.toId);

            if (fromLevel > 0 && toLevel > 0)
                conn.image.color = new Color(0.3f, 0.7f, 1f, 0.8f);
            else if (fromLevel > 0)
                conn.image.color = new Color(1f, 1f, 1f, 0.4f);
            else
                conn.image.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        }

        void RefreshAllConnectionColors()
        {
            foreach (var kvp in _connections)
                UpdateConnectionColor(kvp.Value);
        }

        // --- Visual refresh (no rebuild) ---

        void RefreshAllNodeVisuals()
        {
            foreach (var kvp in _nodeViews)
            {
                var node = _graphConfig.GetNode(kvp.Key);
                var level = _upgradeTracker.GetNodeLevel(kvp.Key);
                var canAfford = CanAffordNextLevel(kvp.Key);
                kvp.Value.UpdateVisualState(level, node.maxLevel, canAfford);
            }

            RefreshAllConnectionColors();
            RefreshTooltipIfVisible();
        }

        void RefreshTooltipIfVisible()
        {
            if (_hoveredNode == null) return;

            var node = _graphConfig.GetNode(_hoveredNode.NodeId);
            var level = _upgradeTracker.GetNodeLevel(_hoveredNode.NodeId);
            var canAfford = CanAffordNextLevel(_hoveredNode.NodeId);
            tooltipView.Refresh(node, level, canAfford);
        }

        // --- Hover & Click ---

        void OnNodeHoverEnter(UpgradeNodeView nodeView)
        {
            _hoveredNode = nodeView;

            var node = _graphConfig.GetNode(nodeView.NodeId);
            var level = _upgradeTracker.GetNodeLevel(nodeView.NodeId);
            var canAfford = CanAffordNextLevel(nodeView.NodeId);

            var worldPos = nodeView.transform.position;
            var screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);

            tooltipView.Show(node, level, canAfford, screenPos);
        }

        void OnNodeHoverExit(UpgradeNodeView nodeView)
        {
            _hoveredNode = null;
            tooltipView.Hide();
        }

        void OnNodeClicked(string nodeId)
        {
            _upgradeTracker.TryPurchase(nodeId);
        }

        // --- Pan & Zoom ---

        public void OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform, eventData.position, eventData.pressEventCamera, out _dragStart);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform, eventData.position, eventData.pressEventCamera, out var current);

            var delta = current - _dragStart;
            graphContainer.anchoredPosition += delta;
            _dragStart = current;
        }

        public void OnScroll(PointerEventData eventData)
        {
            var previousZoom = _currentZoom;
            _currentZoom += eventData.scrollDelta.y * zoomSpeed;
            _currentZoom = Mathf.Clamp(_currentZoom, zoomMin, zoomMax);

            if (Mathf.Approximately(previousZoom, _currentZoom)) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                graphContainer, eventData.position, eventData.pressEventCamera, out var localMouse);

            var scaleFactor = _currentZoom / previousZoom;
            graphContainer.localScale = Vector3.one * _currentZoom;

            var newLocalMouse = localMouse * scaleFactor;
            var correction = (localMouse - newLocalMouse) * _currentZoom;
            graphContainer.anchoredPosition += correction;
        }

        // --- Helpers ---

        bool CanAffordNextLevel(string nodeId)
        {
            var node = _graphConfig.GetNode(nodeId);
            var level = _upgradeTracker.GetNodeLevel(nodeId);
            if (level >= node.maxLevel) return false;
            return _letterTracker.GetCoins() >= node.costsPerLevel[level];
        }

        readonly struct ConnectionLine
        {
            public readonly GameObject gameObject;
            public readonly Image image;
            public readonly string fromId;
            public readonly string toId;

            public ConnectionLine(GameObject go, Image img, string from, string to)
            {
                gameObject = go;
                image = img;
                fromId = from;
                toId = to;
            }
        }
    }
}

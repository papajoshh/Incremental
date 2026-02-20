using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace TypingDefense
{
    public class UpgradeGraphView : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler,
        IDragHandler, IEndDragHandler,
        IScrollHandler
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
        [SerializeField] float panMargin = 100f;
        [SerializeField] float dragThreshold = 5f;

        const float PanDecay = 0.92f;
        const float PanMinSpeed = 0.5f;
        const float ElasticOvershoot = 0.3f;
        const float ElasticSnapDuration = 0.25f;
        const float ZoomLerpDuration = 0.12f;
        const float SettleDuration = 0.3f;
        const float TooltipSuppressDuration = 0.1f;
        const float ZoomBounceAmount = 0.02f;

        UpgradeGraphConfig _graphConfig;
        UpgradeTracker _upgradeTracker;
        LetterTracker _letterTracker;
        GameFlowController _gameFlow;

        readonly Dictionary<string, UpgradeNodeView> _nodeViews = new();
        readonly Dictionary<(string, string), ConnectionLine> _connections = new();

        UpgradeNodeView _hoveredNode;
        float _currentZoom = 1f;
        float _targetZoom = 1f;
        Vector2 _dragStart;
        Vector2 _dragOrigin;
        bool _isDragging;
        bool _dragExceededThreshold;
        Vector2 _panVelocity;
        Vector2 _boundsMin;
        Vector2 _boundsMax;
        float _suppressTooltipUntil;
        Tween _zoomTween;
        Tween _elasticTween;
        Tween _settleTween;

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

            _zoomTween?.Kill();
            _elasticTween?.Kill();
            _settleTween?.Kill();
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
            RecalculateNodeBounds();
            ResetView();
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

            RecalculateNodeBounds();
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
            lineImage.raycastTarget = false;
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
            if (_isDragging) return;
            if (Time.unscaledTime < _suppressTooltipUntil) return;

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
            if (_isDragging) return;

            _hoveredNode = null;
            tooltipView.Hide();
        }

        void OnNodeClicked(string nodeId)
        {
            if (_dragExceededThreshold) return;
            _upgradeTracker.TryPurchase(nodeId);
        }

        // --- Pan & Zoom ---

        void Update()
        {
            if (_isDragging) return;
            if (_panVelocity.sqrMagnitude < PanMinSpeed * PanMinSpeed) return;

            _panVelocity *= PanDecay;
            graphContainer.anchoredPosition += _panVelocity * Time.unscaledDeltaTime;

            if (_panVelocity.sqrMagnitude < PanMinSpeed * PanMinSpeed)
            {
                _panVelocity = Vector2.zero;
                SnapBackIfOutOfBounds();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _elasticTween?.Kill();
            _settleTween?.Kill();
            _panVelocity = Vector2.zero;

            var parentRect = (RectTransform)transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out _dragStart);
            _dragOrigin = _dragStart;
            _isDragging = true;
            _dragExceededThreshold = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;
            _suppressTooltipUntil = Time.unscaledTime + TooltipSuppressDuration;
            SnapBackIfOutOfBounds();
        }

        public void OnDrag(PointerEventData eventData)
        {
            var parentRect = (RectTransform)transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out var current);

            if (!_dragExceededThreshold)
            {
                if ((current - _dragOrigin).sqrMagnitude >= dragThreshold * dragThreshold)
                {
                    _dragExceededThreshold = true;
                    tooltipView.Hide();
                }
                else
                {
                    _dragStart = current;
                    return;
                }
            }

            var delta = current - _dragStart;
            _panVelocity = delta / Time.unscaledDeltaTime;
            graphContainer.anchoredPosition += delta;
            ClampPositionWithElasticOvershoot();
            _dragStart = current;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;
            _suppressTooltipUntil = Time.unscaledTime + TooltipSuppressDuration;
            SnapBackIfOutOfBounds();
        }

        public void OnScroll(PointerEventData eventData)
        {
            var previousTarget = _targetZoom;
            _targetZoom += eventData.scrollDelta.y * zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, zoomMin, zoomMax);

            if (Mathf.Approximately(previousTarget, _targetZoom)) return;

            var hitLimit = Mathf.Approximately(_targetZoom, zoomMin) || Mathf.Approximately(_targetZoom, zoomMax);
            if (hitLimit)
                graphContainer.DOPunchScale(Vector3.one * ZoomBounceAmount, 0.2f, 6);

            var parentRect = (RectTransform)transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out var pivotInParent);

            var zoomFrom = _currentZoom;
            var zoomTo = _targetZoom;

            _zoomTween?.Kill();
            _zoomTween = DOTween.To(() => _currentZoom, z =>
            {
                var prevZoom = _currentZoom;
                _currentZoom = z;
                graphContainer.localScale = Vector3.one * _currentZoom;

                var ratio = _currentZoom / prevZoom;
                var posRelativeToPivot = graphContainer.anchoredPosition - pivotInParent;
                graphContainer.anchoredPosition = pivotInParent + posRelativeToPivot * ratio;
            }, zoomTo, ZoomLerpDuration).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        // --- Bounds & Elastic ---

        void RecalculateNodeBounds()
        {
            if (_nodeViews.Count == 0)
            {
                _boundsMin = Vector2.zero;
                _boundsMax = Vector2.zero;
                return;
            }

            _boundsMin = new Vector2(float.MaxValue, float.MaxValue);
            _boundsMax = new Vector2(float.MinValue, float.MinValue);

            foreach (var kvp in _nodeViews)
            {
                var pos = ((RectTransform)kvp.Value.transform).anchoredPosition;
                _boundsMin = Vector2.Min(_boundsMin, pos);
                _boundsMax = Vector2.Max(_boundsMax, pos);
            }

            _boundsMin -= Vector2.one * panMargin;
            _boundsMax += Vector2.one * panMargin;
        }

        Vector2 CalculateCentroid()
        {
            if (_nodeViews.Count == 0) return Vector2.zero;

            var sum = Vector2.zero;
            foreach (var kvp in _nodeViews)
                sum += ((RectTransform)kvp.Value.transform).anchoredPosition;

            return sum / _nodeViews.Count;
        }

        Vector2 ClampedPosition(Vector2 pos)
        {
            var parentRect = (RectTransform)transform;
            var parentSize = parentRect.rect.size;
            var halfParent = parentSize * 0.5f;

            var scaledMin = _boundsMin * _currentZoom;
            var scaledMax = _boundsMax * _currentZoom;

            var clampMinX = halfParent.x - scaledMax.x;
            var clampMaxX = -halfParent.x - scaledMin.x;
            var clampMinY = halfParent.y - scaledMax.y;
            var clampMaxY = -halfParent.y - scaledMin.y;

            if (clampMinX > clampMaxX) clampMinX = clampMaxX = (clampMinX + clampMaxX) * 0.5f;
            if (clampMinY > clampMaxY) clampMinY = clampMaxY = (clampMinY + clampMaxY) * 0.5f;

            return new Vector2(
                Mathf.Clamp(pos.x, clampMinX, clampMaxX),
                Mathf.Clamp(pos.y, clampMinY, clampMaxY));
        }

        void ClampPositionWithElasticOvershoot()
        {
            var clamped = ClampedPosition(graphContainer.anchoredPosition);
            var diff = graphContainer.anchoredPosition - clamped;

            if (diff == Vector2.zero) return;

            var parentSize = ((RectTransform)transform).rect.size;
            var maxOvershoot = parentSize * ElasticOvershoot;
            diff.x = Mathf.Clamp(diff.x, -maxOvershoot.x, maxOvershoot.x);
            diff.y = Mathf.Clamp(diff.y, -maxOvershoot.y, maxOvershoot.y);
            graphContainer.anchoredPosition = clamped + diff;
        }

        void SnapBackIfOutOfBounds()
        {
            var clamped = ClampedPosition(graphContainer.anchoredPosition);
            if (graphContainer.anchoredPosition == clamped) return;

            _panVelocity = Vector2.zero;
            _elasticTween?.Kill();
            _elasticTween = graphContainer
                .DOAnchorPos(clamped, ElasticSnapDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        // --- Reset View ---

        void ResetView()
        {
            var centroid = CalculateCentroid();

            _currentZoom = 0.9f;
            _targetZoom = 1f;
            graphContainer.localScale = Vector3.one * _currentZoom;
            graphContainer.anchoredPosition = -centroid * _currentZoom + new Vector2(0f, -15f);
            _panVelocity = Vector2.zero;

            _settleTween?.Kill();
            _settleTween = DOTween.Sequence()
                .Append(DOTween.To(() => _currentZoom, z =>
                {
                    _currentZoom = z;
                    graphContainer.localScale = Vector3.one * _currentZoom;
                    graphContainer.anchoredPosition = -centroid * _currentZoom;
                }, 1f, SettleDuration).SetEase(Ease.OutBack))
                .SetUpdate(true)
                .OnComplete(() => _targetZoom = _currentZoom);
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

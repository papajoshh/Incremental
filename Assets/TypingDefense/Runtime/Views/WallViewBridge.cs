using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class WallViewBridge : MonoBehaviour
    {
        WallConfig _wallConfig;
        WallTracker _tracker;
        WallManager _wallManager;
        WallSegmentView.Factory _segmentFactory;
        CameraShaker _cameraShaker;
        PlayerStats _playerStats;
        ArenaView _arenaView;
        UpgradeTracker _upgradeTracker;

        readonly Dictionary<WallSegmentId, WallSegmentView> _activeViews = new();

        [Inject]
        public void Construct(
            WallConfig wallConfig,
            WallTracker tracker,
            WallManager wallManager,
            WallSegmentView.Factory segmentFactory,
            CameraShaker cameraShaker,
            PlayerStats playerStats,
            ArenaView arenaView,
            UpgradeTracker upgradeTracker)
        {
            _wallConfig = wallConfig;
            _tracker = tracker;
            _wallManager = wallManager;
            _segmentFactory = segmentFactory;
            _cameraShaker = cameraShaker;
            _playerStats = playerStats;
            _arenaView = arenaView;
            _upgradeTracker = upgradeTracker;

            _tracker.OnSegmentBroken += OnSegmentBroken;
            _tracker.OnRingCompleted += OnRingCompleted;
            _wallManager.OnSegmentCharMatched += OnSegmentCharMatched;
            _upgradeTracker.OnNodePurchased += OnNodePurchased;
        }
        
        void Start()
        {
            SpawnAllSegments();
        }

        void OnDestroy()
        {
            _tracker.OnSegmentBroken -= OnSegmentBroken;
            _tracker.OnRingCompleted -= OnRingCompleted;
            _wallManager.OnSegmentCharMatched -= OnSegmentCharMatched;
            _upgradeTracker.OnNodePurchased -= OnNodePurchased;
        }

        void SpawnAllSegments()
        {
            var center = _arenaView.CenterPosition;

            foreach (var id in _tracker.EnumerateAllSegments())
            {
                if (_tracker.IsBroken(id)) continue;

                var ring = _wallConfig.rings[id.Ring];
                var halfW = ring.width / 2f;
                var halfH = ring.height / 2f;

                GetSegmentEndpoints(center, halfW, halfH, id.Side, id.Index, ring.GetSegmentsForSide(id.Side),
                    out var startPoint, out var endPoint);

                var word = _wallManager.GetWallWord(id);
                var view = _segmentFactory.Create();
                view.Setup(id, startPoint, endPoint, word.Text);

                var revealed = _playerStats.WallRevealLevel > id.Ring;
                view.SetRevealed(revealed);

                _activeViews[id] = view;
            }
        }

        void GetSegmentEndpoints(Vector3 center, float halfW, float halfH,
            int side, int index, int segsPerSide,
            out Vector3 startPoint, out Vector3 endPoint)
        {
            switch (side)
            {
                case 0:
                {
                    var segWidth = (halfW * 2f) / segsPerSide;
                    var x0 = center.x - halfW + segWidth * index;
                    var x1 = x0 + segWidth;
                    var y = center.y + halfH;
                    startPoint = new Vector3(x0, y, center.z);
                    endPoint = new Vector3(x1, y, center.z);
                    return;
                }
                case 1:
                {
                    var segWidth = (halfW * 2f) / segsPerSide;
                    var x0 = center.x - halfW + segWidth * index;
                    var x1 = x0 + segWidth;
                    var y = center.y - halfH;
                    startPoint = new Vector3(x0, y, center.z);
                    endPoint = new Vector3(x1, y, center.z);
                    return;
                }
                case 2:
                {
                    var segHeight = (halfH * 2f) / segsPerSide;
                    var y0 = center.y - halfH + segHeight * index;
                    var y1 = y0 + segHeight;
                    var x = center.x - halfW;
                    startPoint = new Vector3(x, y0, center.z);
                    endPoint = new Vector3(x, y1, center.z);
                    return;
                }
                default:
                {
                    var segHeight = (halfH * 2f) / segsPerSide;
                    var y0 = center.y - halfH + segHeight * index;
                    var y1 = y0 + segHeight;
                    var x = center.x + halfW;
                    startPoint = new Vector3(x, y0, center.z);
                    endPoint = new Vector3(x, y1, center.z);
                    return;
                }
            }
        }

        public Vector3 GetSegmentPosition(WallSegmentId id)
        {
            return _activeViews[id].MidpointPosition;
        }

        public bool HasView(WallSegmentId id) => _activeViews.ContainsKey(id);

        public void SetSegmentTargeted(WallSegmentId id, bool targeted)
        {
            if (!_activeViews.TryGetValue(id, out var view)) return;
            view.SetTargeted(targeted);
        }

        void OnSegmentBroken(WallSegmentId id)
        {
            if (!_activeViews.TryGetValue(id, out var view)) return;

            view.PlayBreakAnimation();
            _activeViews.Remove(id);
            _cameraShaker.Shake(0.3f, 0.25f, 16);

            foreach (var kvp in _activeViews)
            {
                if (kvp.Key.Ring != id.Ring || kvp.Key.Side != id.Side) continue;
                kvp.Value.PlayNeighborFlinch();
            }
        }

        void OnRingCompleted(int ring)
        {
            Time.timeScale = 0f;
            DOVirtual.DelayedCall(0.12f, () => Time.timeScale = 1f).SetUpdate(true);

            _cameraShaker.Shake(0.6f, 0.5f, 20);
            _cameraShaker.ZoomPunch(0.08f, 0.5f);
        }

        void OnSegmentCharMatched(WallSegmentId id, int matchedCount)
        {
            if (!_activeViews.TryGetValue(id, out var view)) return;

            var word = _wallManager.GetWallWord(id);
            view.UpdateMatchProgress(matchedCount, word.Text);
        }

        void OnNodePurchased(string nodeId)
        {
            RefreshRevealState();
        }

        public void RefreshRevealState()
        {
            var delay = 0f;
            foreach (var kvp in _activeViews)
            {
                var revealed = _playerStats.WallRevealLevel > kvp.Key.Ring;
                if (revealed == kvp.Value.IsRevealed) continue;

                kvp.Value.SetRevealedDelayed(revealed, delay);
                delay += 0.05f;
            }
        }
    }
}

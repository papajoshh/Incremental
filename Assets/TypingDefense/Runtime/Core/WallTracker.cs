using System;
using System.Collections.Generic;
using UnityEngine;

namespace TypingDefense
{
    public class WallTracker
    {
        readonly WallConfig _config;
        readonly bool[] _broken;
        readonly int _totalSegments;
        readonly int[] _ringOffsets;

        public event Action<WallSegmentId> OnSegmentBroken;
        public event Action<int> OnRingCompleted;

        public WallTracker(WallConfig config)
        {
            _config = config;

            _ringOffsets = new int[config.rings.Length];
            var total = 0;
            for (var i = 0; i < config.rings.Length; i++)
            {
                _ringOffsets[i] = total;
                total += config.rings[i].segmentsPerSide * 4;
            }

            _totalSegments = total;
            _broken = new bool[_totalSegments];
        }

        public int TotalSegments => _totalSegments;

        public int ToFlatIndex(WallSegmentId id)
        {
            return _ringOffsets[id.Ring] + id.Side * _config.rings[id.Ring].segmentsPerSide + id.Index;
        }

        public WallSegmentId FromFlatIndex(int flatIndex)
        {
            for (var ring = _config.rings.Length - 1; ring >= 0; ring--)
            {
                if (flatIndex < _ringOffsets[ring]) continue;

                var local = flatIndex - _ringOffsets[ring];
                var segsPerSide = _config.rings[ring].segmentsPerSide;
                var side = local / segsPerSide;
                var index = local % segsPerSide;
                return new WallSegmentId(ring, side, index);
            }

            return new WallSegmentId(0, 0, 0);
        }

        public bool IsBroken(WallSegmentId id) => _broken[ToFlatIndex(id)];

        public void BreakSegment(WallSegmentId id)
        {
            var flat = ToFlatIndex(id);
            if (_broken[flat]) return;

            _broken[flat] = true;
            OnSegmentBroken?.Invoke(id);

            if (IsRingComplete(id.Ring))
                OnRingCompleted?.Invoke(id.Ring);
        }

        public bool IsSideBroken(int ring, int side)
        {
            var segsPerSide = _config.rings[ring].segmentsPerSide;
            var offset = _ringOffsets[ring] + side * segsPerSide;

            for (var i = 0; i < segsPerSide; i++)
            {
                if (!_broken[offset + i]) return false;
            }

            return true;
        }

        public bool IsSidePartiallyBroken(int ring, int side)
        {
            var segsPerSide = _config.rings[ring].segmentsPerSide;
            var offset = _ringOffsets[ring] + side * segsPerSide;

            for (var i = 0; i < segsPerSide; i++)
            {
                if (_broken[offset + i]) return true;
            }

            return false;
        }

        public bool IsRingComplete(int ring)
        {
            for (var side = 0; side < 4; side++)
            {
                if (!IsSideBroken(ring, side)) return false;
            }

            return true;
        }

        public int GetHighestCompletedRing()
        {
            for (var ring = _config.rings.Length - 1; ring >= 0; ring--)
            {
                if (IsRingComplete(ring)) return ring;
            }

            return -1;
        }

        public Rect GetBlackHoleBounds()
        {
            var topExtent = _config.rings[0].height / 2f;
            var bottomExtent = _config.rings[0].height / 2f;
            var leftExtent = _config.rings[0].width / 2f;
            var rightExtent = _config.rings[0].width / 2f;

            for (var ring = 0; ring < _config.rings.Length; ring++)
            {
                if (IsSideBroken(ring, 0)) topExtent = GetNextRingHalfH(ring);
                else { topExtent = _config.rings[ring].height / 2f; break; }
            }

            for (var ring = 0; ring < _config.rings.Length; ring++)
            {
                if (IsSideBroken(ring, 1)) bottomExtent = GetNextRingHalfH(ring);
                else { bottomExtent = _config.rings[ring].height / 2f; break; }
            }

            for (var ring = 0; ring < _config.rings.Length; ring++)
            {
                if (IsSideBroken(ring, 2)) leftExtent = GetNextRingHalfW(ring);
                else { leftExtent = _config.rings[ring].width / 2f; break; }
            }

            for (var ring = 0; ring < _config.rings.Length; ring++)
            {
                if (IsSideBroken(ring, 3)) rightExtent = GetNextRingHalfW(ring);
                else { rightExtent = _config.rings[ring].width / 2f; break; }
            }

            return new Rect(-leftExtent, -bottomExtent, leftExtent + rightExtent, bottomExtent + topExtent);
        }

        public Vector2 GetSpawnBoundsHalfSize()
        {
            var completedRing = GetHighestCompletedRing();
            var activeRing = Mathf.Min(completedRing + 1, _config.rings.Length - 1);
            var rc = _config.rings[activeRing];
            return new Vector2(rc.width / 2f, rc.height / 2f);
        }

        public List<(int ring, int side)> GetSidesWithBrokenSegments()
        {
            var result = new List<(int, int)>();

            for (var ring = 0; ring < _config.rings.Length; ring++)
            {
                for (var side = 0; side < 4; side++)
                {
                    if (IsSidePartiallyBroken(ring, side))
                        result.Add((ring, side));
                }
            }

            return result;
        }

        public IEnumerable<WallSegmentId> EnumerateAllSegments()
        {
            for (var ring = 0; ring < _config.rings.Length; ring++)
            {
                var segsPerSide = _config.rings[ring].segmentsPerSide;
                for (var side = 0; side < 4; side++)
                {
                    for (var index = 0; index < segsPerSide; index++)
                        yield return new WallSegmentId(ring, side, index);
                }
            }
        }

        public bool[] CaptureState() => (bool[])_broken.Clone();

        public void RestoreState(bool[] data)
        {
            var count = Mathf.Min(data.Length, _broken.Length);
            for (var i = 0; i < count; i++)
                _broken[i] = data[i];
        }

        float GetNextRingHalfW(int currentRing)
        {
            var next = currentRing + 1;
            if (next >= _config.rings.Length)
                return _config.rings[currentRing].width / 2f + 4f;
            return _config.rings[next].width / 2f;
        }

        float GetNextRingHalfH(int currentRing)
        {
            var next = currentRing + 1;
            if (next >= _config.rings.Length)
                return _config.rings[currentRing].height / 2f + 4f;
            return _config.rings[next].height / 2f;
        }
    }
}

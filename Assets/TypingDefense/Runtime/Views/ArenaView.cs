using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class ArenaView : MonoBehaviour
    {
        [SerializeField] Transform centerPoint;
        [SerializeField] float edgeMargin = 1f;
        [SerializeField] float cameraBoundsMargin = 2f;

        WallTracker _wallTracker;
        WallConfig _wallConfig;

        public Vector3 CenterPosition => centerPoint.position;

        [Inject]
        public void Construct(WallTracker wallTracker, WallConfig wallConfig)
        {
            _wallTracker = wallTracker;
            _wallConfig = wallConfig;
        }

        public Rect GetBHBounds()
        {
            var wallRect = _wallTracker.GetBlackHoleBounds();
            var center = centerPoint.position;

            return new Rect(
                center.x + wallRect.xMin + edgeMargin,
                center.y + wallRect.yMin + edgeMargin,
                wallRect.width - edgeMargin * 2f,
                wallRect.height - edgeMargin * 2f
            );
        }

        public Rect GetCameraBounds()
        {
            var bh = GetBHBounds();
            return new Rect(
                bh.xMin + cameraBoundsMargin,
                bh.yMin + cameraBoundsMargin,
                Mathf.Max(bh.width - cameraBoundsMargin * 2f, 0.1f),
                Mathf.Max(bh.height - cameraBoundsMargin * 2f, 0.1f)
            );
        }

        public Vector3 ClampToInterior(Vector3 position)
        {
            var bounds = GetBHBounds();
            position.x = Mathf.Clamp(position.x, bounds.xMin, bounds.xMax);
            position.y = Mathf.Clamp(position.y, bounds.yMin, bounds.yMax);
            position.z = centerPoint.position.z;
            return position;
        }

        public Vector3 GetRandomInteriorPosition()
        {
            var halfSize = _wallTracker.GetSpawnBoundsHalfSize();
            var center = centerPoint.position;
            return new Vector3(
                Random.Range(center.x - halfSize.x + edgeMargin, center.x + halfSize.x - edgeMargin),
                Random.Range(center.y - halfSize.y + edgeMargin, center.y + halfSize.y - edgeMargin),
                center.z);
        }

        public Vector3 GetRandomEdgePosition()
        {
            var halfSize = _wallTracker.GetSpawnBoundsHalfSize();
            var center = centerPoint.position;
            var halfW = halfSize.x + edgeMargin;
            var halfH = halfSize.y + edgeMargin;
            var side = Random.Range(0, 4);
            return GetEdgePositionOnSideInternal(side, center, halfW, halfH);
        }

        public Vector3 GetEdgePositionOnSide(int side, int ring)
        {
            var center = centerPoint.position;
            var rc = _wallConfig.rings[ring];
            var halfW = rc.width / 2f + edgeMargin;
            var halfH = rc.height / 2f + edgeMargin;
            return GetEdgePositionOnSideInternal(side, center, halfW, halfH);
        }

        Vector3 GetEdgePositionOnSideInternal(int side, Vector3 center, float halfW, float halfH)
        {
            return side switch
            {
                0 => new Vector3(Random.Range(center.x - halfW, center.x + halfW), center.y + halfH, center.z),
                1 => new Vector3(Random.Range(center.x - halfW, center.x + halfW), center.y - halfH, center.z),
                2 => new Vector3(center.x - halfW, Random.Range(center.y - halfH, center.y + halfH), center.z),
                _ => new Vector3(center.x + halfW, Random.Range(center.y - halfH, center.y + halfH), center.z),
            };
        }
    }
}

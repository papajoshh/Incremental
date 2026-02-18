using UnityEngine;

namespace TypingDefense
{
    public class ArenaView : MonoBehaviour
    {
        [SerializeField] Transform centerPoint;
        [SerializeField] float arenaWidth = 16f;
        [SerializeField] float arenaHeight = 9f;
        [SerializeField] float edgeMargin = 1f;

        public Vector3 CenterPosition => centerPoint.position;

        public Vector3 GetRandomInteriorPosition()
        {
            var center = centerPoint.position;
            var halfW = arenaWidth / 2f - edgeMargin;
            var halfH = arenaHeight / 2f - edgeMargin;
            return new Vector3(
                Random.Range(center.x - halfW, center.x + halfW),
                Random.Range(center.y - halfH, center.y + halfH),
                center.z);
        }

        public Vector3 ClampToInterior(Vector3 position)
        {
            var center = centerPoint.position;
            var halfW = arenaWidth / 2f - edgeMargin;
            var halfH = arenaHeight / 2f - edgeMargin;
            position.x = Mathf.Clamp(position.x, center.x - halfW, center.x + halfW);
            position.y = Mathf.Clamp(position.y, center.y - halfH, center.y + halfH);
            position.z = center.z;
            return position;
        }

        public Vector3 GetRandomEdgePosition()
        {
            var center = centerPoint.position;
            var halfW = arenaWidth / 2f + edgeMargin;
            var halfH = arenaHeight / 2f + edgeMargin;

            var side = Random.Range(0, 4);

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

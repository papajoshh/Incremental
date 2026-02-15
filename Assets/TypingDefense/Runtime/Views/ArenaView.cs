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

        public Vector3 GetRandomEdgePosition()
        {
            var center = centerPoint.position;
            var halfW = arenaWidth / 2f + edgeMargin;
            var halfH = arenaHeight / 2f + edgeMargin;

            var side = Random.Range(0, 4);

            return side switch
            {
                0 => new Vector3(Random.Range(center.x - halfW, center.x + halfW), center.y + halfH, center.z), // arriba
                1 => new Vector3(Random.Range(center.x - halfW, center.x + halfW), center.y - halfH, center.z), // abajo
                2 => new Vector3(center.x - halfW, Random.Range(center.y - halfH, center.y + halfH), center.z), // izquierda
                _ => new Vector3(center.x + halfW, Random.Range(center.y - halfH, center.y + halfH), center.z), // derecha
            };
        }
    }
}

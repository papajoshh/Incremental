using Runtime.Application;
using UnityEngine;

namespace Runtime.Infraestructure
{
    public class DoorMonoBehaviour : MonoBehaviour, Door
    {
        [SerializeField] private Transform destination;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Collider2D triggerCollider;
        [SerializeField] private RoomMonoBehaviour destinationRoom;

        [SerializeField] private bool startClosed = true;
        
        private bool IsOpened => triggerCollider.enabled;
        private void Awake()
        {
            if (startClosed)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            spriteRenderer.sprite = openSprite;
            triggerCollider.enabled = true;
        }

        private void Close()
        {
            spriteRenderer.sprite = closedSprite;
            triggerCollider.enabled = false;
        }

        public void CrossTo(Transform objectToMove)
        {
            if (!IsOpened) return;
            objectToMove.position = destination.position;
            if (destinationRoom) destinationRoom.TryDiscover();
        }
    }
}

using Runtime.Application;
using Runtime.Domain;
using UnityEngine;

namespace Runtime.Infraestructure
{
    public class DoorMonoBehaviour : MonoBehaviour, Door, ISaveable
    {
        [SerializeField] private Transform entrancePosition;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Collider2D triggerCollider;
        [SerializeField] private DoorMonoBehaviour destinationDoor;
        [SerializeField] private RoomMonoBehaviour destinationRoom;

        [SerializeField] private bool startClosed = true;
        [SerializeField] private string saveId;
        public string SaveId => saveId;
        
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

        public void Close()
        {
            spriteRenderer.sprite = closedSprite;
            triggerCollider.enabled = false;
        }

        public void CrossTo(Transform objectToMove)
        {
            if (!IsOpened) return;
            objectToMove.position = destinationDoor.GetEntrancePosition();
            if (destinationRoom) destinationRoom.TryDiscover();
        }

        public Vector3 GetEntrancePosition() => entrancePosition.position;

        public DoorSaveData CaptureState()
        {
            return new DoorSaveData
            {
                id = saveId,
                isOpen = IsOpened
            };
        }

        public void RestoreState(DoorSaveData data)
        {
            if (data.isOpen) Open();
            else Close();
        }

        public string CaptureStateJson() => JsonUtility.ToJson(CaptureState());
        public void RestoreStateJson(string json) => RestoreState(JsonUtility.FromJson<DoorSaveData>(json));
    }
}

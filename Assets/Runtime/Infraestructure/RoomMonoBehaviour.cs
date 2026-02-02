using Runtime.Application;
using Runtime.Domain;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class RoomMonoBehaviour : MonoBehaviour, ISaveable
    {
        [SerializeField] private Vector3 cameraPosition;
        [SerializeField] private float cameraSize;
        [SerializeField] private string saveId;
        public string SaveId => saveId;

        [Inject] private readonly ScreenFader _screenFader;

        private Room _room;

        private void Awake()
        {
            _room = new Room();
            _room.OnDiscovered += OnDiscovered;
        }

        private void OnDestroy()
        {
            _room.OnDiscovered -= OnDiscovered;
        }

        public void TryDiscover()
        {
            _room.Discover();
        }

        public RoomSaveData CaptureState()
        {
            return new RoomSaveData
            {
                id = saveId,
                discovered = _room.Discovered
            };
        }

        public void RestoreState(RoomSaveData data)
        {
            if (data.discovered) _room.RestoreDiscovered();
        }

        public string CaptureStateJson() => JsonUtility.ToJson(CaptureState());
        public void RestoreStateJson(string json) => RestoreState(JsonUtility.FromJson<RoomSaveData>(json));

        private async void OnDiscovered()
        {
            var camera = Camera.main;
            await _screenFader.FadeInOut(() =>
            {
                camera.transform.localPosition = cameraPosition;
                camera.orthographicSize = cameraSize;
            });
        }
    }
}

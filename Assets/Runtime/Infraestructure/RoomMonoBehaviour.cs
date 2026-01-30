using Runtime.Domain;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class RoomMonoBehaviour : MonoBehaviour
    {
        [SerializeField] private Vector3 cameraPosition;
        [SerializeField] private float cameraSize;

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

using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class StickmanWorkbench: MonoBehaviour
    {
        [Inject] private readonly FirstStickman _firstStickman;
        [SerializeField] private Camera _camera;
        [SerializeField] private Collider2D _rightLegCollider;
        [SerializeField] private Collider2D _leftLegCollider;
        [SerializeField] private Collider2D _headCollider;

        private void Awake()
        {
            _camera.orthographicSize = 5.04f;
            _firstStickman.OnBodyReady += ZoomOutToArms;
            _firstStickman.OnArmsReady += ZoomOutToFullBench;
            _firstStickman.OnFullBodyReady += ZoomOutToRoom;
        }

        private void ZoomOutToFullBench()
        {
            _camera.DOOrthoSize(19.75f, 2f).SetEase(Ease.OutCubic);
            _rightLegCollider.enabled = true;
            _leftLegCollider.enabled = true;
            _headCollider.enabled = true;
        }

        private void OnDestroy()
        {
            _firstStickman.OnBodyReady -= ZoomOutToArms;
            _firstStickman.OnArmsReady -= ZoomOutToFullBench;
            _firstStickman.OnFullBodyReady -= ZoomOutToRoom;
        }

        private void ZoomOutToArms() => _camera.DOOrthoSize(10.68f, 2f).SetEase(Ease.OutCubic);
        private void ZoomOutToRoom() => _camera.DOOrthoSize(45f, 2f).SetEase(Ease.OutCubic);
    }
}
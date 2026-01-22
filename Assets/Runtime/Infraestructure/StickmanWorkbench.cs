using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class StickmanWorkbench: MonoBehaviour
    {
        [Inject] private readonly FirstStickman _firstStickman;
        [SerializeField] private Camera _camera;
        [SerializeField] private ClickRightLegButton _rightLegCollider;
        [SerializeField] private ClickLeftLegButton _leftLegCollider;
        [SerializeField] private ClickRightArmButton _rightArmCollider;
        [SerializeField] private ClickLeftArmButton _leftArmCollider;
        [SerializeField] private ClickHeadButton _headCollider;

        private void Awake()
        {
            _camera.orthographicSize = 1f;
            _firstStickman.OnBodyReady += ZoomOutToArms;
            _firstStickman.OnArmsReady += ZoomOutToFullBench;
            _firstStickman.OnFullBodyReady += ZoomOutToRoom;
        }

        private void ZoomOutToFullBench()
        {
            _camera.DOOrthoSize(2.23f, 2f).SetEase(Ease.OutCubic);
            _rightLegCollider.StartToFill();
            _leftLegCollider.StartToFill();
            _headCollider.StartToFill();
        }

        private void OnDestroy()
        {
            _firstStickman.OnBodyReady -= ZoomOutToArms;
            _firstStickman.OnArmsReady -= ZoomOutToFullBench;
            _firstStickman.OnFullBodyReady -= ZoomOutToRoom;
        }

        private void ZoomOutToArms()
        {
            _leftArmCollider.StartToFill();
            _rightArmCollider.StartToFill();
        }

        private void ZoomOutToRoom()
        {
            _camera.DOOrthoSize(5.21f, 2f).SetEase(Ease.OutCubic);
            _camera.transform.DOLocalMove(new Vector3(0.77f, -0.41f, 0f), 2f).SetEase(Ease.OutCubic);
        }
    }
}
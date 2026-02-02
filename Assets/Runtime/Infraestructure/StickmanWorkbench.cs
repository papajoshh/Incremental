using DG.Tweening;
using Runtime.Application;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class StickmanWorkbench: MonoBehaviour, ISkippable
    {
        [Inject] private readonly FirstStickman _firstStickman;
        [SerializeField] private Camera _camera;
        [Inject] private readonly ContainerShaker _containerShaker;
        [SerializeField] private ClickRightLegButton _rightLegCollider;
        [SerializeField] private ClickLeftLegButton _leftLegCollider;
        [SerializeField] private ClickRightArmButton _rightArmCollider;
        [SerializeField] private ClickLeftArmButton _leftArmCollider;
        [SerializeField] private ClickHeadButton _headCollider;
        [SerializeField] private MoñecoMonoBehaviour moñecoMonoBehaviour;

        [Inject] private readonly BagOfMoñecos _bagOfMoñecos;
        private void Awake()
        {
            moñecoMonoBehaviour.gameObject.SetActive(false);
            _camera.orthographicSize = 3.57f;
            _camera.transform.localPosition = new Vector3(-5.3f, -2.49f, 0);
            _firstStickman.OnBodyReady += ZoomOutToArms;
            _firstStickman.OnArmsReady += ZoomOutToFullBench;
            _firstStickman.OnFullBodyReady += ZoomOutToRoom;
        }

        private void ZoomOutToFullBench()
        {
            _camera.DOOrthoSize(9f, 0.4f).SetEase(Ease.OutExpo).OnComplete(() => _containerShaker.Shake(0.3f));
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
            var currentSize = _camera.orthographicSize;
            _containerShaker.SlowMotion(0.8f);
            DOTween.Sequence()
                .Append(_camera.DOOrthoSize(currentSize * 0.8f, 0.8f).SetEase(Ease.InQuad))
                .Append(_camera.DOOrthoSize(20f, 0.35f).SetEase(Ease.OutExpo))
                .Join(_camera.transform.DOLocalMove(new Vector3(14.9f, 3.2f, 0f), 0.35f).SetEase(Ease.OutExpo))
                .OnComplete(() =>
                {
                    _containerShaker.Shake(0.5f);
                    CreateMoñeco();
                })
                .SetUpdate(true);
        }
        
        public void Skip()
        {
            _camera.orthographicSize = 20f;
            _camera.transform.localPosition = new Vector3(14.9f, 3.2f, 0f);
            CreateMoñeco();
        }

        private void CreateMoñeco()
        {
            moñecoMonoBehaviour.gameObject.SetActive(true);
            moñecoMonoBehaviour.Birth();
            gameObject.SetActive(false);
            _bagOfMoñecos.Add();
        }
    }
}
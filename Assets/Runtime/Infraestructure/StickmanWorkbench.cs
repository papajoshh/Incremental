using DG.Tweening;
using Runtime.Application;
using UnityEngine;
using UnityEngine.Serialization;
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
        [SerializeField] private MoñecoMonoBehaviour moñecoMonoBehaviour;
        [SerializeField] private GameObject moñecoCreator;

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
            _camera.DOOrthoSize(9f, 2f).SetEase(Ease.OutCubic);
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
            _camera.DOOrthoSize(20f, 2f).SetEase(Ease.OutCubic);
            _camera.transform.DOLocalMove(new Vector3(14.9f, 3.2f, 0f), 2f).SetEase(Ease.OutCubic).OnComplete(CreateMoñeco);
        }
        
        private void CreateMoñeco()
        {
            moñecoMonoBehaviour.gameObject.SetActive(true);
            moñecoMonoBehaviour.Birth();
            gameObject.SetActive(false);
            moñecoCreator.GetComponent<MoñecoMachine>().TurnOn();
            _bagOfMoñecos.Add();
        }
    }
}
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
        [SerializeField] private Moñeco moñeco;

        private void Awake()
        {
            moñeco.gameObject.SetActive(false);
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
            _camera.DOOrthoSize(27.41f, 2f).SetEase(Ease.OutCubic);
            _camera.transform.DOLocalMove(new Vector3(32f, 13.39f, 0f), 2f).SetEase(Ease.OutCubic).OnComplete(CreateMoñeco);
        }
        
        private void CreateMoñeco()
        {
            moñeco.gameObject.SetActive(true);
            moñeco.Birth();
            gameObject.SetActive(false);
        }
    }
}
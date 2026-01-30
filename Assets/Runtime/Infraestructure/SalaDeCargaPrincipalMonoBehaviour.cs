using DG.Tweening;
using Runtime.Infrastructure;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class SalaDeCargaPrincipalMonoBehaviour: MonoBehaviour
    {
        [SerializeField] private SingleMoñecoCreatingMachineGameObject[] _moñecoCreatinGameObjectsMachines;
        
        [Inject] private readonly BagOfMoñecos _bagOfMoñecos;
        [Inject] private readonly ContainerShaker _containerShaker;
        [Inject] private StickmanWorkbench _workbench;

        private int machineOccupiedCount;
        public bool Milestone2MoñecosTriggered { get; private set; }
        public int MachineOccupiedCount => machineOccupiedCount;
        private void Awake()
        {
            _bagOfMoñecos.OnMoñecosChange += OnGet2Moñecos;
            foreach (var _moñecoCreatingMachines in _moñecoCreatinGameObjectsMachines)
            {
                _moñecoCreatingMachines.CurrentMachine.OnOccupied += OnMachineOccupied;
            }
        }

        private void OnDestroy()
        {
            _bagOfMoñecos.OnMoñecosChange -= OnGet2Moñecos;
            foreach (var moñecoCreatingMachines in _moñecoCreatinGameObjectsMachines)
            {
                moñecoCreatingMachines.CurrentMachine.OnOccupied -= OnMachineOccupied;
            }
        }

        private void OnGet2Moñecos(int _currentMoñecos)
        {
            if (_currentMoñecos != 2) return;
            Milestone2MoñecosTriggered = true;
            _bagOfMoñecos.OnMoñecosChange -= OnGet2Moñecos;
            var mainCamera = Camera.main;
            var currentSize = mainCamera.orthographicSize;
            _containerShaker.SlowMotion(1f);
            DOTween.Sequence()
                .Append(mainCamera.DOOrthoSize(currentSize * 0.75f, 1f).SetEase(Ease.InQuad))
                .Append(mainCamera.DOOrthoSize(48f, 0.25f).SetEase(Ease.OutExpo))
                .Join(mainCamera.transform.DOLocalMove(new Vector3(60.4f, 21.4f, 0f), 0.25f).SetEase(Ease.OutExpo))
                .OnComplete(() => _containerShaker.Shake(0.8f))
                .SetUpdate(true);
        }
        
        private void OnMachineOccupied()
        {
            machineOccupiedCount++;
            if (machineOccupiedCount < _moñecoCreatinGameObjectsMachines.Length) return;
            ZoomOutToExit();
        }
        
        public void RestoreMilestones(bool milestone2Triggered, int occupiedCount)
        {
            machineOccupiedCount = occupiedCount;
            if (milestone2Triggered)
            {
                Milestone2MoñecosTriggered = true;
                _bagOfMoñecos.OnMoñecosChange -= OnGet2Moñecos;
            }
            if (occupiedCount >= _moñecoCreatinGameObjectsMachines.Length)
            {
                foreach (var machine in _moñecoCreatinGameObjectsMachines)
                    machine.CurrentMachine.OnOccupied -= OnMachineOccupied;
            }
        }

        public void SkipToStart()
        {
            _workbench.Skip();
        }

        public void SkipTo2Moñecos()
        {
            SkipToStart();
            _bagOfMoñecos.Add();
        }

        public void SkipToAllOccupied()
        {
            _workbench.Skip();

            foreach (var machine in _moñecoCreatinGameObjectsMachines)
            {
                machine.SpawnWorker();
                _bagOfMoñecos.Add();
            }

            RestoreMilestones(true, _moñecoCreatinGameObjectsMachines.Length);

            var cam = Camera.main;
            cam.orthographicSize = 52f;
            cam.transform.localPosition = new Vector3(71.1f, 21.4f, 0f);
        }

        private void ZoomOutToExit()
        {
            var mainCamera = Camera.main;
            var currentSize = mainCamera.orthographicSize;
            _containerShaker.SlowMotion(1f);
            DOTween.Sequence()
                .Append(mainCamera.DOOrthoSize(currentSize * 0.75f, 1f).SetEase(Ease.InQuad))
                .Append(mainCamera.DOOrthoSize(52f, 0.25f).SetEase(Ease.OutExpo))
                .Join(mainCamera.transform.DOLocalMove(new Vector3(71.1f, 21.4f, 0f), 0.25f).SetEase(Ease.OutExpo))
                .OnComplete(() => _containerShaker.Shake(1f))
                .SetUpdate(true);
        }
    }
}
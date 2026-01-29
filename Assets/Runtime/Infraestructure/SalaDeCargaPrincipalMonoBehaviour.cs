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

        private int machineOccupiedCount;
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
            _bagOfMoñecos.OnMoñecosChange -= OnGet2Moñecos;
            var mainCamera = Camera.main;
            mainCamera.DOOrthoSize(48f, 2f).SetEase(Ease.OutCubic);
            mainCamera.transform.DOLocalMove(new Vector3(60.4f, 21.4f, 0f), 2f).SetEase(Ease.OutCubic);
        }
        
        private void OnMachineOccupied()
        {
            machineOccupiedCount++;
            if (machineOccupiedCount < _moñecoCreatinGameObjectsMachines.Length) return;
            
        }
    }
}
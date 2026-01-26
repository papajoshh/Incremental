using System.Threading.Tasks;
using Runtime.Application;
using Runtime.Infraestructure;
using UnityEngine;
using Zenject;

namespace Runtime.Infrastructure
{
    public class SingleMoñecoCreatingMachineGameObject: MonoBehaviour, Interactable, MoñecoMachine
    {
        [SerializeField] private int ticksToSpawn = 300;
        [SerializeField] private GameObject _moñecoPrefab;
        [SerializeField] private Transform positionToSpawn;
        [SerializeField] private Transform positionToInteract;
        
        private Interactor _currentUser;
        private bool _canBeInteracted = true;
        private MoñecoCreatingMachine _currentMachine;

        [Inject]
        private void Construct(BagOfMoñecos bag)
        {
            _currentMachine = new MoñecoCreatingMachine(bag, ticksToSpawn, this);
        }

        public bool CanInteract(Interactor interactor) => _currentUser == null && _canBeInteracted;
        
        public void StartInteraction(Interactor interactor)
        {
            _currentUser = interactor;
            interactor.SetPositionToInteract(positionToInteract);
        }

        public async Task OnInteractionTick(Interactor interactor)
        {
            if (!_canBeInteracted) return;
            await _currentMachine.ImpulseMoñecoCreation();
        }

        public void EndInteraction(Interactor interactor)
        {
            if (_currentUser != interactor) return;
            _currentUser = null;
        }

        public async Task GiveBirth()
        {
            var moñeco = Instantiate(_moñecoPrefab,positionToSpawn.position, Quaternion.identity);
            _canBeInteracted = false;
            await moñeco.GetComponent<Moñeco>().Birth();
            _canBeInteracted = true;
        }
    }
}
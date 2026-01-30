using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Runtime.Application;

namespace Runtime
{
    public class MoñecoCreatingMachine
    {
        private readonly BagOfMoñecos _bagOfMoñecos;
        private PressWitCap _pressWithCap;
        private MoñecoMachine _machine;
        public Action OnMoñecoCreated;
        public Action OnOccupied;
        
        public float Progress => _pressWithCap.Percentage;
        public int CurrentPresses => _pressWithCap.CurrentPresses;
        private List<Interactor> _workersHere;

        public MoñecoCreatingMachine(BagOfMoñecos _bagOfMoñecos, int pressesToCreate, MoñecoMachine machine, List<Interactor> workersHere)
        {
            this._bagOfMoñecos = _bagOfMoñecos;
            _pressWithCap = PressWitCap.StartWith(0,1,pressesToCreate);
            _machine = machine;
            _workersHere = workersHere;
        }

        private void CreateMoñeco()
        {
            _bagOfMoñecos.Add();
            OnMoñecoCreated?.Invoke();
        }

        public void AddWorkerMoñeco(Interactor moñeco)
        {
            _workersHere.Add(moñeco);
            OnOccupied?.Invoke();
        }

        public void RestorePresses(int presses)
        {
            _pressWithCap.SetPresses(presses);
        }

        public void RestoreWorker(Interactor worker)
        {
            _workersHere.Add(worker);
        }

        public async Task ImpulseMoñecoCreation()
        {
            _pressWithCap.Press();
            if (!_pressWithCap.Completed) return;
            await _machine.GiveBirth();
            CreateMoñeco();
            _pressWithCap.Reset();
        }
        
    }
}
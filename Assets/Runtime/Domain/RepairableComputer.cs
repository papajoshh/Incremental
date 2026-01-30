using System;
using System.Collections.Generic;
using Runtime.Application;

namespace Runtime.Domain
{
    public class RepairableComputer
    {
        private readonly PressWitCap _pressWithCap;
        private readonly int _totalSlots;
        private readonly List<Interactor> _workers = new();

        public bool Repaired { get; private set; }
        public float Progress => _pressWithCap.Percentage;
        public int CurrentPresses => _pressWithCap.CurrentPresses;
        public bool HasFreeSlot => _workers.Count < _totalSlots;
        public Action OnRepaired;

        public RepairableComputer(int pressesToRepair, int totalSlots)
        {
            _pressWithCap = PressWitCap.StartWith(0, 1, pressesToRepair);
            _totalSlots = totalSlots;
        }

        public bool AddWorker(Interactor worker)
        {
            if (!HasFreeSlot || Repaired) return false;
            _workers.Add(worker);
            return true;
        }

        public void Impulse()
        {
            if (Repaired) return;
            _pressWithCap.Press();
            if (!_pressWithCap.Completed) return;
            Repaired = true;
            OnRepaired?.Invoke();
        }

        public void Restore(int presses, bool repaired)
        {
            _pressWithCap.SetPresses(presses);
            Repaired = repaired;
        }

        public void RestoreWorker(Interactor worker)
        {
            _workers.Add(worker);
        }

        public List<Interactor> GetWorkers() => _workers;
    }
}

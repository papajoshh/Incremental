using System;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class EnergyTracker : ITickable
    {
        private readonly RunConfig _runConfig;
        private readonly PlayerStats _playerStats;
        private readonly RunManager _runManager;
        private readonly GameFlowController _gameFlow;

        private float _drainTimer;

        public float CurrentEnergy { get; private set; }

        public event Action<float> OnEnergyChanged;

        public EnergyTracker(
            RunConfig runConfig,
            PlayerStats playerStats,
            RunManager runManager,
            GameFlowController gameFlow)
        {
            _runConfig = runConfig;
            _playerStats = playerStats;
            _runManager = runManager;
            _gameFlow = gameFlow;
        }

        public void Tick()
        {
            if (_gameFlow.State != GameState.Playing) return;

            _drainTimer -= Time.deltaTime;
            var drainInterval = CalculateDrainInterval();

            if (_drainTimer > 0f) return;

            CurrentEnergy -= 1;
            _drainTimer = drainInterval;
            OnEnergyChanged?.Invoke(CurrentEnergy);

            if (CurrentEnergy <= 0) _runManager.TriggerGameOver();
        }

        public void AddEnergy(float amount)
        {
            if (amount <= 0) return;

            CurrentEnergy = Mathf.Min(CurrentEnergy + amount, _playerStats.MaxEnergy);
            OnEnergyChanged?.Invoke(CurrentEnergy);
        }

        public void StartRun()
        {
            CurrentEnergy = _playerStats.MaxEnergy;
            _drainTimer = CalculateDrainInterval();
            OnEnergyChanged?.Invoke(CurrentEnergy);
        }

        private float CalculateDrainInterval()
        {
            var baseInterval = _runConfig.baseDrainInterval;
            var levelReduction = (_runManager.CurrentLevel - 1) * _runConfig.drainScalePerLevel;
            var interval = baseInterval * (1f - levelReduction) * _playerStats.DrainMultiplier;
            return Mathf.Max(interval, 0.5f);
        }
    }
}

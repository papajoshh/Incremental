using System;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class EnergyTracker : ITickable
    {
        readonly RunConfig _runConfig;
        readonly PlayerStats _playerStats;
        readonly RunManager _runManager;
        readonly GameFlowController _gameFlow;

        public float CurrentEnergy { get; private set; }

        public event Action<float> OnEnergyChanged;
        public event Action OnEnergyDepleted;

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

            var drainPerSecond = 1f / CalculateDrainInterval();
            CurrentEnergy -= drainPerSecond * Time.deltaTime;
            CurrentEnergy = Mathf.Max(CurrentEnergy, 0f);
            OnEnergyChanged?.Invoke(CurrentEnergy);

            if (CurrentEnergy <= 0f)
            {
                OnEnergyDepleted?.Invoke();
                _gameFlow.StartCollectionPhase();
            }
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
            OnEnergyChanged?.Invoke(CurrentEnergy);
        }

        float CalculateDrainInterval()
        {
            var baseInterval = _runConfig.baseDrainInterval;
            var levelReduction = (_runManager.CurrentLevel - 1) * _runConfig.drainScalePerLevel;
            var interval = baseInterval * (1f - levelReduction) * _playerStats.DrainMultiplier;
            return Mathf.Max(interval, 0.5f);
        }
    }
}

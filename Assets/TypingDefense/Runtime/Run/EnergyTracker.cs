using System;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class EnergyTracker : ITickable
    {
        readonly LevelProgressionConfig _levelConfig;
        readonly PlayerStats _playerStats;
        readonly RunManager _runManager;
        readonly GameFlowController _gameFlow;

        public float CurrentEnergy { get; private set; }

        public event Action<float> OnEnergyChanged;
        public event Action OnEnergyDepleted;

        public EnergyTracker(
            LevelProgressionConfig levelConfig,
            PlayerStats playerStats,
            RunManager runManager,
            GameFlowController gameFlow)
        {
            _levelConfig = levelConfig;
            _playerStats = playerStats;
            _runManager = runManager;
            _gameFlow = gameFlow;
        }

        public void Tick()
        {
            var state = _gameFlow.State;
            if (state != GameState.Playing) return;

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
            var config = _levelConfig.GetLevel(_runManager.CurrentLevel);
            var interval = config.drainInterval * _playerStats.DrainMultiplier;
            return Mathf.Max(interval, 0.5f);
        }
    }
}

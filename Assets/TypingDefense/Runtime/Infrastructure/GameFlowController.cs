using System;
using Zenject;

namespace TypingDefense
{
    public class GameFlowController
    {
        readonly LazyInject<RunManager> _runManager;
        readonly LazyInject<WordManager> _wordManager;
        readonly LazyInject<EnergyTracker> _energyTracker;
        readonly LazyInject<UpgradeTracker> _upgradeTracker;
        readonly LazyInject<DefenseSaveManager> _saveManager;
        readonly PlayerStats _playerStats;

        public GameState State { get; private set; }

        public event Action<GameState> OnStateChanged;

        public GameFlowController(
            LazyInject<RunManager> runManager,
            LazyInject<WordManager> wordManager,
            LazyInject<EnergyTracker> energyTracker,
            LazyInject<UpgradeTracker> upgradeTracker,
            LazyInject<DefenseSaveManager> saveManager,
            PlayerStats playerStats)
        {
            _runManager = runManager;
            _wordManager = wordManager;
            _energyTracker = energyTracker;
            _upgradeTracker = upgradeTracker;
            _saveManager = saveManager;
            _playerStats = playerStats;
        }

        public void SetState(GameState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(State);
        }

        public void StartRun()
        {
            _playerStats.ResetToBase();
            _upgradeTracker.Value.ApplyAllUpgrades();
            _runManager.Value.StartRun();
            _energyTracker.Value.StartRun();
            _wordManager.Value.StartRun();
            SetState(GameState.Playing);
        }

        public void HandleRunEnded()
        {
            _saveManager.Value.MarkFirstRunCompleted();

            if (_runManager.Value.CurrentLevel >= 10)
                _saveManager.Value.MarkLevel10Reached();

            SetState(GameState.Converting);
        }

        public void HandleConvertingComplete()
        {
            SetState(GameState.Menu);
        }

        public void ReturnToMenu()
        {
            SetState(GameState.Menu);
        }
    }
}

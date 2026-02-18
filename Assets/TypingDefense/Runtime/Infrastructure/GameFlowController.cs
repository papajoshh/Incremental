using System;
using Zenject;

namespace TypingDefense
{
    public class GameFlowController : IInitializable
    {
        readonly LazyInject<RunManager> _runManager;
        readonly LazyInject<WordManager> _wordManager;
        readonly LazyInject<EnergyTracker> _energyTracker;
        readonly LazyInject<UpgradeTracker> _upgradeTracker;
        readonly LazyInject<DefenseSaveManager> _saveManager;
        readonly LazyInject<CollectionPhaseController> _collectionPhase;
        readonly LazyInject<BlackHoleController> _blackHole;
        readonly PlayerStats _playerStats;

        public GameState State { get; private set; }

        public event Action<GameState> OnStateChanged;
        public event Action OnReturnedFromRun;

        public GameFlowController(
            LazyInject<RunManager> runManager,
            LazyInject<WordManager> wordManager,
            LazyInject<EnergyTracker> energyTracker,
            LazyInject<UpgradeTracker> upgradeTracker,
            LazyInject<DefenseSaveManager> saveManager,
            LazyInject<CollectionPhaseController> collectionPhase,
            LazyInject<BlackHoleController> blackHole,
            PlayerStats playerStats)
        {
            _runManager = runManager;
            _wordManager = wordManager;
            _energyTracker = energyTracker;
            _upgradeTracker = upgradeTracker;
            _saveManager = saveManager;
            _collectionPhase = collectionPhase;
            _blackHole = blackHole;
            _playerStats = playerStats;
        }

        public void Initialize()
        {
            if (_saveManager.Value.HasCompletedFirstRun) return;

            StartRun();
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

        public void StartCollectionPhase()
        {
            if (State != GameState.Playing) return;

            _collectionPhase.Value.StartCollection(_blackHole.Value.Position);
            SetState(GameState.Collecting);
        }

        public void HandleCollectionEnded()
        {
            if (State != GameState.Collecting) return;

            _saveManager.Value.MarkFirstRunCompleted();

            if (_runManager.Value.CurrentLevel >= 10)
                _saveManager.Value.MarkLevel10Reached();

            SetState(GameState.Menu);
            OnReturnedFromRun?.Invoke();
        }

        public void HandleRunEnded()
        {
            if (State != GameState.Playing && State != GameState.Collecting) return;

            _collectionPhase.Value.ForceEnd();
            _saveManager.Value.MarkFirstRunCompleted();

            if (_runManager.Value.CurrentLevel >= 10)
                _saveManager.Value.MarkLevel10Reached();

            SetState(GameState.Menu);
            OnReturnedFromRun?.Invoke();
        }

        public void ReturnToMenu()
        {
            _collectionPhase.Value.ForceEnd();
            SetState(GameState.Menu);
        }
    }
}

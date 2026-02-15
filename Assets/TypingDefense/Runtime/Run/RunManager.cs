using System;

namespace TypingDefense
{
    public class RunManager
    {
        readonly PlayerStats _playerStats;
        readonly GameFlowController _gameFlow;

        public int CurrentHp { get; private set; }
        public int CurrentLevel { get; private set; } = 1;
        public bool ShieldActive { get; private set; }
        public int PrestigeCurrency { get; private set; }

        public event Action<int> OnHpChanged;
        public event Action<int> OnLevelChanged;
        public event Action OnRunEnded;
        public event Action<int> OnPrestigeCurrencyChanged;

        public RunManager(PlayerStats playerStats, GameFlowController gameFlow)
        {
            _playerStats = playerStats;
            _gameFlow = gameFlow;
        }

        public void StartRun()
        {
            CurrentHp = _playerStats.MaxHp;
            CurrentLevel = 1;
            ShieldActive = _playerStats.ShieldProtocol;
            OnHpChanged?.Invoke(CurrentHp);
            OnLevelChanged?.Invoke(CurrentLevel);
        }

        public void TakeDamage(int amount)
        {
            if (ShieldActive)
            {
                ShieldActive = false;
                return;
            }

            CurrentHp -= amount;
            OnHpChanged?.Invoke(CurrentHp);

            if (CurrentHp <= 0) TriggerGameOver();
        }

        public void TriggerGameOver()
        {
            OnRunEnded?.Invoke();
            _gameFlow.HandleRunEnded();
        }

        public void Retreat()
        {
            OnRunEnded?.Invoke();
            _gameFlow.HandleRunEnded();
        }

        public void AdvanceLevel()
        {
            CurrentLevel++;
            if (_playerStats.ShieldProtocol) ShieldActive = true;
            OnLevelChanged?.Invoke(CurrentLevel);
        }

        public void AddPrestigeCurrency(int amount)
        {
            PrestigeCurrency += amount;
            OnPrestigeCurrencyChanged?.Invoke(PrestigeCurrency);
        }

        public void RestorePrestigeCurrency(int amount)
        {
            PrestigeCurrency = amount;
        }
    }
}

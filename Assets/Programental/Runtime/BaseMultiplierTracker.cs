using System;

namespace Programental
{
    public class BaseMultiplierTracker
    {
        private readonly float _costBase;
        private readonly float _levelIncrement;
        private readonly BonusMultipliers _bonusMultipliers;
        private int _currentLevel;
        private int _availableLinesToInvest;

        public int CurrentLevel => _currentLevel;
        public int AvailableLinesToInvest => _availableLinesToInvest;
        public float CurrentMultiplier => 1f + _currentLevel * _levelIncrement;

        public event Action OnMultiplierChanged;

        public BaseMultiplierTracker(float costBase, float levelIncrement, BonusMultipliers bonusMultipliers)
        {
            _costBase = costBase;
            _levelIncrement = levelIncrement;
            _bonusMultipliers = bonusMultipliers;
        }

        public void AddDeletedLines(int count)
        {
            _availableLinesToInvest += count;
            AutoInvestLines();
        }

        private void AutoInvestLines()
        {
            while (true)
            {
                var cost = GetCostForLevel(_currentLevel + 1);
                if (_availableLinesToInvest < cost) break;

                _availableLinesToInvest -= cost;
                _currentLevel++;
                UpdateMultiplier();
                OnMultiplierChanged?.Invoke();
            }
        }

        private void UpdateMultiplier()
        {
            _bonusMultipliers.BaseMultiplier = CurrentMultiplier;
        }

        public int GetCostForLevel(int level)
        {
            return (int)Math.Pow(_costBase, level);
        }

        public BaseMultiplierData CaptureState()
        {
            return new BaseMultiplierData
            {
                currentLevel = _currentLevel,
                availableLinesToInvest = _availableLinesToInvest
            };
        }

        public void RestoreState(BaseMultiplierData data)
        {
            _currentLevel = data.currentLevel;
            _availableLinesToInvest = data.availableLinesToInvest;
            UpdateMultiplier();
            if (_currentLevel > 0)
                OnMultiplierChanged?.Invoke();
        }
    }
}
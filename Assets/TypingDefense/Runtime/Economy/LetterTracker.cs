using System;

namespace TypingDefense
{
    public class LetterTracker
    {
        private readonly LetterConfig _letterConfig;
        private readonly PlayerStats _playerStats;

        private int[] _letterInventory = new int[5];
        private int _coins;

        public event Action OnLettersChanged;
        public event Action OnCoinsChanged;

        public LetterTracker(LetterConfig letterConfig, PlayerStats playerStats)
        {
            _letterConfig = letterConfig;
            _playerStats = playerStats;
        }

        public void EarnLetters(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var type = RollLetterType();
                _letterInventory[(int)type]++;
            }

            OnLettersChanged?.Invoke();
        }

        public void ConvertAllToCoins()
        {
            var total = 0;

            for (var i = 0; i < 5; i++)
            {
                total += _letterInventory[i] * _letterConfig.GetConversionValue((LetterType)i);
                _letterInventory[i] = 0;
            }

            _coins += total;
            OnLettersChanged?.Invoke();
            OnCoinsChanged?.Invoke();
        }

        public bool TrySpendCoins(int amount)
        {
            if (_coins < amount) return false;

            _coins -= amount;
            OnCoinsChanged?.Invoke();
            return true;
        }

        public void DirectAddCoins(int amount)
        {
            _coins += amount;
            OnCoinsChanged?.Invoke();
        }

        public void RemoveLetter(LetterType type)
        {
            _letterInventory[(int)type]--;
            OnLettersChanged?.Invoke();
        }

        public int GetLetterCount(LetterType type) => _letterInventory[(int)type];

        public int GetCoins() => _coins;

        public DefenseSaveData CaptureState()
        {
            return new DefenseSaveData
            {
                Letters = (int[])_letterInventory.Clone(),
                Coins = _coins
            };
        }

        public void RestoreState(DefenseSaveData data)
        {
            _letterInventory = (int[])data.Letters.Clone();
            _coins = data.Coins;
            OnLettersChanged?.Invoke();
            OnCoinsChanged?.Invoke();
        }

        private LetterType RollLetterType()
        {
            var chances = _playerStats.LetterDropChances;

            for (var i = 4; i >= 0; i--)
            {
                if (chances[i] > 0f && UnityEngine.Random.value < chances[i])
                    return (LetterType)i;
            }

            return LetterType.A;
        }
    }
}

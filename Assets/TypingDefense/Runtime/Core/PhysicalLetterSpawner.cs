using System;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class PhysicalLetterSpawner : IInitializable, IDisposable
    {
        readonly WordManager _wordManager;
        readonly PlayerStats _playerStats;
        readonly PhysicalLetter.Factory _letterFactory;
        readonly WordViewBridge _wordViewBridge;

        public PhysicalLetterSpawner(
            WordManager wordManager,
            PlayerStats playerStats,
            PhysicalLetter.Factory letterFactory,
            WordViewBridge wordViewBridge)
        {
            _wordManager = wordManager;
            _playerStats = playerStats;
            _letterFactory = letterFactory;
            _wordViewBridge = wordViewBridge;
        }

        public void Initialize()
        {
            _wordManager.OnWordCompleted += OnWordKilled;
            _wordManager.OnWordCriticalKill += OnWordKilled;
        }

        public void Dispose()
        {
            _wordManager.OnWordCompleted -= OnWordKilled;
            _wordManager.OnWordCriticalKill -= OnWordKilled;
        }

        void OnWordKilled(DefenseWord word)
        {
            var position = _wordViewBridge.GetWordPosition(word);
            SpawnLetters(position);
        }

        void SpawnLetters(Vector3 position)
        {
            var count = _playerStats.LettersPerKill;

            for (var i = 0; i < count; i++)
            {
                var type = RollLetterType();
                var letter = _letterFactory.Create();
                letter.Setup(type, position);
            }
        }

        LetterType RollLetterType()
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

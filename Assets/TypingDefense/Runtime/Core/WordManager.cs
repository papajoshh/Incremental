using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class WordManager : ITickable
    {
        readonly WordPool _wordPool;
        readonly LevelProgressionConfig _levelConfig;
        readonly BossConfig _bossConfig;
        readonly PlayerStats _playerStats;
        readonly EnergyTracker _energyTracker;
        readonly RunManager _runManager;
        readonly GameFlowController _gameFlow;

        readonly List<DefenseWord> _activeWords = new();
        readonly List<DefenseWord> _pendingRemoval = new();

        float _spawnTimer;
        float _autoTypeTimer;
        int _killCount;
        DefenseWord _bossWord;
        bool _spawnPaused;

        public event Action<DefenseWord> OnWordSpawned;
        public event Action<DefenseWord> OnWordCompleted;
        public event Action<DefenseWord> OnWordCriticalKill;
        public event Action<DefenseWord> OnWordReachedCenter;
        public event Action OnInputError;
        public event Action<DefenseWord> OnBossSpawned;
        public event Action<DefenseWord> OnBossHit;
        public event Action<DefenseWord> OnBossDefeated;
        public event Action<DefenseWord, string> OnWordTextChanged;
        public event Action OnKillCountChanged;
        public event Action<IReadOnlyList<DefenseWord>> OnAllWordsDissipated;
        public event Action<DefenseWord, Vector3, float> OnExternalWordSpawned;

        LevelConfig CurrentLevelConfig => _levelConfig.GetLevel(_runManager.CurrentLevel);

        public int KillCount => _killCount;

        public WordManager(
            WordPool wordPool,
            LevelProgressionConfig levelConfig,
            BossConfig bossConfig,
            PlayerStats playerStats,
            EnergyTracker energyTracker,
            RunManager runManager,
            GameFlowController gameFlow)
        {
            _wordPool = wordPool;
            _levelConfig = levelConfig;
            _bossConfig = bossConfig;
            _playerStats = playerStats;
            _energyTracker = energyTracker;
            _runManager = runManager;
            _gameFlow = gameFlow;
        }

        public void Tick()
        {
            if (_gameFlow.State != GameState.Playing) return;

            if (!_spawnPaused)
                UpdateSpawnTimer(Time.deltaTime);

            UpdateAutoType(Time.deltaTime);
            ProcessInput();
        }

        public void HandleWordReachedCenter(DefenseWord word)
        {
            if (word.IsBoss) return;

            _activeWords.Remove(word);
            _runManager.TakeDamage(1);
            OnWordReachedCenter?.Invoke(word);
        }

        public void HandleWordReachedBlackHole(DefenseWord word)
        {
            _activeWords.Remove(word);
            OnWordReachedCenter?.Invoke(word);
        }

        public void RemoveWord(DefenseWord word)
        {
            _activeWords.Remove(word);
        }

        public void StartRun()
        {
            _activeWords.Clear();
            _killCount = 0;
            _bossWord = null;
            _spawnPaused = false;
            _spawnTimer = CurrentLevelConfig.spawnInterval;
            _autoTypeTimer = _playerStats.AutoTypeInterval;
        }

        public IReadOnlyList<DefenseWord> GetActiveWords() => _activeWords;

        public void AddExternalWord(DefenseWord word, Vector3 spawnPosition, float speed)
        {
            _activeWords.Add(word);
            OnExternalWordSpawned?.Invoke(word, spawnPosition, speed);
        }

        public void PauseSpawning()
        {
            _spawnPaused = true;
        }

        public void DissipateAllWords()
        {
            var snapshot = new List<DefenseWord>(_activeWords);
            _activeWords.Clear();
            _bossWord = null;
            OnAllWordsDissipated?.Invoke(snapshot);
        }

        void UpdateSpawnTimer(float dt)
        {
            _spawnTimer -= dt;
            if (_spawnTimer > 0f) return;

            SpawnWord();
            _spawnTimer = Mathf.Max(CurrentLevelConfig.spawnInterval, 0.5f);
        }

        void SpawnWord()
        {
            var config = CurrentLevelConfig;
            var text = _wordPool.GetRandomWord(config.minWordLength, config.maxWordLength);
            var hp = UnityEngine.Random.Range(config.minWordHp, config.maxWordHp + 1);
            var word = new DefenseWord(text, hp);
            _activeWords.Add(word);
            OnWordSpawned?.Invoke(word);
        }

        void ProcessInput()
        {
            var input = Input.inputString;
            foreach (var c in input)
                ProcessChar(c);
        }

        void ProcessChar(char c)
        {
            var matched = false;
            _pendingRemoval.Clear();

            for (var i = 0; i < _activeWords.Count; i++)
            {
                var word = _activeWords[i];
                if (!word.TryMatchChar(c)) continue;

                matched = true;

                if (!word.IsCompleted && !word.IsBoss
                    && _playerStats.CritChance > 0f
                    && UnityEngine.Random.value < _playerStats.CritChance)
                {
                    _pendingRemoval.Add(word);
                    continue;
                }

                if (word.IsCompleted)
                {
                    if (ApplyDamageToWord(word))
                        _pendingRemoval.Add(word);
                }
            }

            foreach (var word in _pendingRemoval)
            {
                var wasCrit = word.CurrentHp > 0;
                _activeWords.Remove(word);
                CompleteWord(word, wasCrit);
            }

            if (!matched) OnInputError?.Invoke();
        }

        bool ApplyDamageToWord(DefenseWord word)
        {
            var damage = _playerStats.BaseDamage;
            if (word.IsBoss)
            {
                damage += _playerStats.BossBonusDamage;
                _energyTracker.AddEnergy(_playerStats.EnergyPerBossHit);
                OnBossHit?.Invoke(word);
            }

            var killed = word.TakeDamage(damage);

            if (!killed)
            {
                var config = CurrentLevelConfig;
                var newText = _wordPool.GetRandomWord(config.minWordLength, config.maxWordLength);
                word.ChangeText(newText);
                OnWordTextChanged?.Invoke(word, newText);
            }

            return killed;
        }

        void CompleteWord(DefenseWord word, bool wasCrit = false)
        {
            _energyTracker.AddEnergy(_playerStats.EnergyPerKill);

            if (word.IsBoss)
            {
                _bossWord = null;
                var config = CurrentLevelConfig;
                _runManager.AddPrestigeCurrency(config.bossPrestigeReward);
                _runManager.MarkBossDefeated();
                OnBossDefeated?.Invoke(word);
                return;
            }

            _killCount++;
            OnKillCountChanged?.Invoke();

            if (wasCrit)
                OnWordCriticalKill?.Invoke(word);
            else
                OnWordCompleted?.Invoke(word);

            if (_killCount >= CurrentLevelConfig.killsForBoss && _bossWord == null)
                SpawnBoss();
        }

        void SpawnBoss()
        {
            var config = CurrentLevelConfig;
            var text = _wordPool.GetRandomWord(config.minWordLength, config.maxWordLength);
            _bossWord = new DefenseWord(text, config.bossHp, isBoss: true);
            _activeWords.Add(_bossWord);
            OnBossSpawned?.Invoke(_bossWord);
        }

        void UpdateAutoType(float dt)
        {
            if (_playerStats.AutoTypeInterval <= 0f) return;

            _autoTypeTimer -= dt;
            if (_autoTypeTimer > 0f) return;

            _autoTypeTimer = _playerStats.AutoTypeInterval;

            var nextChars = new HashSet<char>();
            foreach (var word in _activeWords)
            {
                if (!word.IsCompleted)
                    nextChars.Add(word.NextChar);
            }

            if (nextChars.Count == 0) return;

            var charList = new List<char>(nextChars);
            var count = Mathf.Min(_playerStats.AutoTypeCount, charList.Count);
            var autoRemoval = new List<DefenseWord>();

            for (var i = 0; i < count; i++)
            {
                var idx = UnityEngine.Random.Range(0, charList.Count);
                var chosen = charList[idx];
                charList.RemoveAt(idx);

                foreach (var word in _activeWords)
                {
                    if (word.IsCompleted) continue;
                    if (!word.TryMatchChar(chosen)) continue;

                    if (word.IsCompleted)
                    {
                        if (ApplyDamageToWord(word))
                            autoRemoval.Add(word);
                    }
                }
            }

            foreach (var word in autoRemoval)
            {
                _activeWords.Remove(word);
                CompleteWord(word);
            }
        }
    }
}

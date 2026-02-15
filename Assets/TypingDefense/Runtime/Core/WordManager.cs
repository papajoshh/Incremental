using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class WordManager : ITickable
    {
        readonly WordPool _wordPool;
        readonly RunConfig _runConfig;
        readonly WordSpawnConfig _spawnConfig;
        readonly BossConfig _bossConfig;
        readonly PlayerStats _playerStats;
        readonly LetterTracker _letterTracker;
        readonly EnergyTracker _energyTracker;
        readonly RunManager _runManager;
        readonly GameFlowController _gameFlow;

        readonly List<DefenseWord> _activeWords = new();
        readonly List<DefenseWord> _pendingRemoval = new();

        float _spawnTimer;
        float _autoTypeTimer;
        int _killCount;
        bool _warpAvailable;
        const string WarpText = "warp";
        DefenseWord _warpWord;
        DefenseWord _bossWord;

        public event Action<DefenseWord> OnWordSpawned;
        public event Action<DefenseWord> OnWordCompleted;
        public event Action<DefenseWord> OnWordCriticalKill;
        public event Action<DefenseWord> OnWordReachedCenter;
        public event Action<DefenseWord> OnWarpAvailable;
        public event Action OnWarpCompleted;
        public event Action OnInputError;
        public event Action<DefenseWord> OnBossSpawned;
        public event Action<DefenseWord> OnBossHit;
        public event Action<DefenseWord> OnBossDefeated;
        public event Action<DefenseWord, string> OnWordTextChanged;

        public WordManager(
            WordPool wordPool,
            RunConfig runConfig,
            WordSpawnConfig spawnConfig,
            BossConfig bossConfig,
            PlayerStats playerStats,
            LetterTracker letterTracker,
            EnergyTracker energyTracker,
            RunManager runManager,
            GameFlowController gameFlow)
        {
            _wordPool = wordPool;
            _runConfig = runConfig;
            _spawnConfig = spawnConfig;
            _bossConfig = bossConfig;
            _playerStats = playerStats;
            _letterTracker = letterTracker;
            _energyTracker = energyTracker;
            _runManager = runManager;
            _gameFlow = gameFlow;
        }

        public void Tick()
        {
            if (_gameFlow.State != GameState.Playing) return;

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

        public void StartRun()
        {
            _activeWords.Clear();
            _killCount = 0;
            _warpAvailable = false;
            _warpWord = null;
            _bossWord = null;
            _spawnTimer = _spawnConfig.baseSpawnInterval;
            _autoTypeTimer = _playerStats.AutoTypeInterval;
        }

        public IReadOnlyList<DefenseWord> GetActiveWords() => _activeWords;

        void UpdateSpawnTimer(float dt)
        {
            _spawnTimer -= dt;
            if (_spawnTimer > 0f) return;

            SpawnWord();
            var interval = _spawnConfig.baseSpawnInterval
                           - (_runManager.CurrentLevel * _spawnConfig.spawnIntervalScalePerLevel);
            _spawnTimer = Mathf.Max(interval, 0.5f);
        }

        void SpawnWord()
        {
            GetWordLengthRange(_runManager.CurrentLevel, out var minLen, out var maxLen);
            var text = _wordPool.GetRandomWord(minLen, maxLen);
            var hp = CalculateWordHp(_runManager.CurrentLevel);
            var word = new DefenseWord(text, hp);
            _activeWords.Add(word);
            OnWordSpawned?.Invoke(word);
        }

        int CalculateWordHp(int level)
        {
            return Mathf.Max(1, (level + 1) / 2);
        }

        void ProcessInput()
        {
            var input = Input.inputString;
            foreach (var c in input)
                ProcessChar(c);
        }

        void ProcessChar(char c)
        {
            if (_warpWord != null)
            {
                if (_warpWord.TryMatchChar(c) && _warpWord.IsCompleted)
                {
                    DoWarp();
                    return;
                }
            }

            var matched = false;
            _pendingRemoval.Clear();

            for (var i = 0; i < _activeWords.Count; i++)
            {
                var word = _activeWords[i];
                if (!word.TryMatchChar(c)) continue;

                matched = true;

                // Crit kills instantly regardless of HP (not boss)
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
                GetWordLengthRange(_runManager.CurrentLevel, out var minLen, out var maxLen);
                var newText = _wordPool.GetRandomWord(minLen, maxLen);
                word.ChangeText(newText);
                OnWordTextChanged?.Invoke(word, newText);
            }

            return killed;
        }

        void CompleteWord(DefenseWord word, bool wasCrit = false)
        {
            _letterTracker.EarnLetters(_playerStats.LettersPerKill);
            _energyTracker.AddEnergy(_playerStats.EnergyPerKill);
            _killCount++;

            if (word.IsBoss)
            {
                _bossWord = null;
                _runManager.AddPrestigeCurrency(_bossConfig.prestigeReward);
                OnBossDefeated?.Invoke(word);
                return;
            }

            if (wasCrit)
                OnWordCriticalKill?.Invoke(word);
            else
                OnWordCompleted?.Invoke(word);

            if (_killCount >= _runConfig.killsToWarp && !_warpAvailable && _bossWord == null)
                MakeWarpAvailable();
        }

        void MakeWarpAvailable()
        {
            _warpAvailable = true;
            _warpWord = new DefenseWord(WarpText);
            OnWarpAvailable?.Invoke(_warpWord);
        }

        void DoWarp()
        {
            _warpWord = null;
            _warpAvailable = false;
            _killCount = 0;
            OnWarpCompleted?.Invoke();
            _runManager.AdvanceLevel();

            if (_runManager.CurrentLevel == _bossConfig.bossLevel && _bossWord == null)
                SpawnBoss();
        }

        void SpawnBoss()
        {
            GetWordLengthRange(_runManager.CurrentLevel, out var minLen, out var maxLen);
            var text = _wordPool.GetRandomWord(minLen, maxLen);
            _bossWord = new DefenseWord(text, _bossConfig.bossHp, isBoss: true);
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

        static void GetWordLengthRange(int level, out int min, out int max)
        {
            switch (level)
            {
                case 1: min = 3; max = 5; break;
                case 2: min = 3; max = 6; break;
                case 3: min = 4; max = 6; break;
                case 4: min = 4; max = 7; break;
                case 5: min = 5; max = 7; break;
                case 6: min = 5; max = 8; break;
                case 7: min = 6; max = 8; break;
                case 8: min = 6; max = 9; break;
                case 9: min = 7; max = 9; break;
                default: min = 7; max = 10; break;
            }
        }
    }
}

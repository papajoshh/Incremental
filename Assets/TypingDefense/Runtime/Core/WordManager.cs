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

        readonly LazyInject<WordViewBridge> _wordViewBridge;
        readonly LazyInject<BlackHoleController> _blackHole;
        readonly LazyInject<WallManager> _wallManager;
        readonly LazyInject<WallViewBridge> _wallViewBridge;

        readonly List<DefenseWord> _activeWords = new();
        readonly List<DefenseWord> _pendingRemoval = new();
        readonly List<DefenseWord> _autoTargets = new();
        readonly List<DefenseWord> _autoRemoval = new();
        readonly List<WallSegmentId> _autoWallTargets = new();
        readonly List<WallSegmentId> _autoWallRemoval = new();

        float _spawnTimer;
        float _autoTargetTimer;
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
        public event Action<DefenseWord> OnAutoTargetAcquired;
        public event Action<DefenseWord> OnAutoTargetLost;

        LevelConfig CurrentLevelConfig => _levelConfig.GetLevel(_runManager.CurrentLevel);

        public int KillCount => _killCount;

        public WordManager(
            WordPool wordPool,
            LevelProgressionConfig levelConfig,
            BossConfig bossConfig,
            PlayerStats playerStats,
            EnergyTracker energyTracker,
            RunManager runManager,
            GameFlowController gameFlow,
            LazyInject<WordViewBridge> wordViewBridge,
            LazyInject<BlackHoleController> blackHole,
            LazyInject<WallManager> wallManager,
            LazyInject<WallViewBridge> wallViewBridge)
        {
            _wordPool = wordPool;
            _levelConfig = levelConfig;
            _bossConfig = bossConfig;
            _playerStats = playerStats;
            _energyTracker = energyTracker;
            _runManager = runManager;
            _gameFlow = gameFlow;
            _wordViewBridge = wordViewBridge;
            _blackHole = blackHole;
            _wallManager = wallManager;
            _wallViewBridge = wallViewBridge;
        }

        public void Tick()
        {
            if (_gameFlow.State != GameState.Playing) return;

            if (!_spawnPaused)
                UpdateSpawnTimer(Time.deltaTime);

            UpdateAutoTarget(Time.deltaTime);
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
            _autoTargetTimer = _playerStats.AutoTargetInterval;
            _autoTargets.Clear();
            _autoWallTargets.Clear();
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

        void UpdateAutoTarget(float dt)
        {
            if (!_playerStats.AutoTargetUnlocked) return;
            if (_playerStats.AutoTargetInterval <= 0f) return;

            _autoTargetTimer -= dt;
            if (_autoTargetTimer > 0f) return;
            _autoTargetTimer = _playerStats.AutoTargetInterval;

            RefreshAutoTargets();
            if (_autoTargets.Count == 0 && _autoWallTargets.Count == 0) return;

            // Process word/boss targets
            _autoRemoval.Clear();
            foreach (var target in _autoTargets)
            {
                if (target.IsCompleted) continue;
                target.TryMatchChar(target.NextChar);

                if (!target.IsCompleted) continue;
                if (ApplyDamageToWord(target))
                    _autoRemoval.Add(target);
            }

            foreach (var word in _autoRemoval)
            {
                _activeWords.Remove(word);
                _autoTargets.Remove(word);
                OnAutoTargetLost?.Invoke(word);
                CompleteWord(word);
            }

            // Process wall targets
            _autoWallRemoval.Clear();
            foreach (var segmentId in _autoWallTargets)
            {
                var completed = _wallManager.Value.TryAutoTypeSegment(segmentId);
                if (!completed) continue;

                _autoWallRemoval.Add(segmentId);
            }

            foreach (var segmentId in _autoWallRemoval)
            {
                _autoWallTargets.Remove(segmentId);
                _wallViewBridge.Value.SetSegmentTargeted(segmentId, false);

                _energyTracker.AddEnergy(_playerStats.EnergyPerKill);
                _killCount++;
                OnKillCountChanged?.Invoke();

                if (_killCount >= CurrentLevelConfig.killsForBoss && _bossWord == null)
                    SpawnBoss();
            }
        }

        void RefreshAutoTargets()
        {
            // Purge stale word targets
            for (var i = _autoTargets.Count - 1; i >= 0; i--)
            {
                var w = _autoTargets[i];
                if (!w.IsCompleted && _activeWords.Contains(w)) continue;
                _autoTargets.RemoveAt(i);
                OnAutoTargetLost?.Invoke(w);
            }

            // Purge stale wall targets
            for (var i = _autoWallTargets.Count - 1; i >= 0; i--)
            {
                var id = _autoWallTargets[i];
                if (_wallManager.Value.IsSegmentAutoTargetable(id)) continue;
                _autoWallTargets.RemoveAt(i);
                _wallViewBridge.Value.SetSegmentTargeted(id, false);
            }

            var maxTargets = Mathf.Max(1, _playerStats.AutoTargetCount);
            var currentCount = _autoTargets.Count + _autoWallTargets.Count;
            if (currentCount >= maxTargets) return;

            var bhPos = _blackHole.Value.Position;
            var slotsToFill = maxTargets - currentCount;

            for (var s = 0; s < slotsToFill; s++)
            {
                var closestDist = float.MaxValue;
                DefenseWord closestWord = null;
                WallSegmentId? closestWall = null;

                // Check all words (normal + boss)
                foreach (var word in _activeWords)
                {
                    if (word.IsCompleted) continue;
                    if (_autoTargets.Contains(word)) continue;

                    var pos = _wordViewBridge.Value.GetWordPosition(word);
                    var dist = (pos - bhPos).sqrMagnitude;
                    if (dist >= closestDist) continue;

                    closestDist = dist;
                    closestWord = word;
                    closestWall = null;
                }

                // Check wall segments
                foreach (var id in _wallManager.Value.GetAutoTargetableSegments())
                {
                    if (_autoWallTargets.Contains(id)) continue;
                    if (!_wallViewBridge.Value.HasView(id)) continue;

                    var pos = _wallViewBridge.Value.GetSegmentPosition(id);
                    var dist = (pos - bhPos).sqrMagnitude;
                    if (dist >= closestDist) continue;

                    closestDist = dist;
                    closestWall = id;
                    closestWord = null;
                }

                if (closestWord != null)
                {
                    _autoTargets.Add(closestWord);
                    OnAutoTargetAcquired?.Invoke(closestWord);
                }
                else if (closestWall.HasValue)
                {
                    _autoWallTargets.Add(closestWall.Value);
                    _wallViewBridge.Value.SetSegmentTargeted(closestWall.Value, true);
                }
                else
                {
                    break;
                }
            }
        }
    }
}

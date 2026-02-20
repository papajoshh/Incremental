using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class WallManager : ITickable, IInitializable
    {
        readonly WallConfig _config;
        readonly WallTracker _tracker;
        readonly WordPool _wordPool;
        readonly WordManager _wordManager;
        readonly PlayerStats _playerStats;
        readonly GameFlowController _gameFlow;
        readonly ArenaView _arenaView;

        readonly Dictionary<WallSegmentId, DefenseWord> _wallWords = new();

        float _blueWordTimer;

        public event Action<WallSegmentId> OnSegmentWordCompleted;
        public event Action<WallSegmentId, int> OnSegmentCharMatched;

        public WallManager(
            WallConfig config,
            WallTracker tracker,
            WordPool wordPool,
            WordManager wordManager,
            PlayerStats playerStats,
            GameFlowController gameFlow,
            ArenaView arenaView)
        {
            _config = config;
            _tracker = tracker;
            _wordPool = wordPool;
            _wordManager = wordManager;
            _playerStats = playerStats;
            _gameFlow = gameFlow;
            _arenaView = arenaView;
        }

        public void Initialize()
        {
            AssignWordsToAllSegments();
            _blueWordTimer = _config.blueWordSpawnInterval;
        }

        public DefenseWord GetWallWord(WallSegmentId id)
        {
            _wallWords.TryGetValue(id, out var word);
            return word;
        }

        public void Tick()
        {
            if (_gameFlow.State != GameState.Playing) return;

            ProcessWallInput();
            UpdateBlueWordSpawner(Time.deltaTime);
        }

        void ProcessWallInput()
        {
            var input = Input.inputString;
            foreach (var c in input)
                ProcessWallChar(c);
        }

        void ProcessWallChar(char c)
        {
            WallSegmentId? completedId = null;

            foreach (var kvp in _wallWords)
            {
                var id = kvp.Key;
                var word = kvp.Value;

                if (_tracker.IsBroken(id)) continue;
                if (!CanTypeRing(id.Ring)) continue;
                if (!word.TryMatchChar(c)) continue;

                OnSegmentCharMatched?.Invoke(id, word.MatchedCount);

                if (!word.IsCompleted) continue;

                completedId = id;
                break;
            }

            if (!completedId.HasValue) return;

            _tracker.BreakSegment(completedId.Value);
            OnSegmentWordCompleted?.Invoke(completedId.Value);
        }

        bool CanTypeRing(int ring) => _playerStats.WallRevealLevel > ring;

        public bool TryAutoTypeSegment(WallSegmentId id)
        {
            var word = _wallWords[id];
            word.TryMatchChar(word.NextChar);
            OnSegmentCharMatched?.Invoke(id, word.MatchedCount);

            if (!word.IsCompleted) return false;

            _tracker.BreakSegment(id);
            OnSegmentWordCompleted?.Invoke(id);
            return true;
        }

        public bool IsSegmentAutoTargetable(WallSegmentId id)
        {
            if (_tracker.IsBroken(id)) return false;
            if (!CanTypeRing(id.Ring)) return false;
            return _wallWords.ContainsKey(id);
        }

        public IEnumerable<WallSegmentId> GetAutoTargetableSegments()
        {
            foreach (var (id, word) in _wallWords)
            {
                if (_tracker.IsBroken(id)) continue;
                if (!CanTypeRing(id.Ring)) continue;
                yield return id;
            }
        }

        void UpdateBlueWordSpawner(float dt)
        {
            var brokenSides = _tracker.GetSidesWithBrokenSegments();
            if (brokenSides.Count == 0) return;

            _blueWordTimer -= dt;
            if (_blueWordTimer > 0f) return;

            _blueWordTimer = _config.blueWordSpawnInterval;
            SpawnBlueWord(brokenSides);
        }

        void SpawnBlueWord(List<(int ring, int side)> brokenSides)
        {
            var (ring, side) = brokenSides[UnityEngine.Random.Range(0, brokenSides.Count)];
            var text = _wordPool.GetRandomWord(_config.blueWordMinLength, _config.blueWordMaxLength);
            var word = new DefenseWord(text, type: WordType.Blue);

            var spawnPos = _arenaView.GetEdgePositionOnSide(side, ring);
            _wordManager.AddExternalWord(word, spawnPos, _config.blueWordSpeed);
        }

        void AssignWordsToAllSegments()
        {
            _wallWords.Clear();

            foreach (var id in _tracker.EnumerateAllSegments())
            {
                if (_tracker.IsBroken(id)) continue;

                var rc = _config.rings[id.Ring];
                var text = _wordPool.GetRandomWord(rc.wallWordMinLength, rc.wallWordMaxLength);
                _wallWords[id] = new DefenseWord(text);
            }
        }
    }
}

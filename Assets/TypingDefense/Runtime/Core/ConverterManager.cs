using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class ConverterManager : ITickable
    {
        readonly ConverterConfig _config;
        readonly PlayerStats _playerStats;
        readonly LetterTracker _letterTracker;
        readonly LetterConfig _letterConfig;
        readonly GameFlowController _gameFlow;
        readonly ArenaView _arenaView;

        readonly List<ConverterLetter> _activeLetters = new();
        readonly List<BlackHole> _blackHoles = new();

        public event Action<ConverterLetter> OnLetterSpawned;
        public event Action<ConverterLetter> OnLetterCollected;
        public event Action<int> OnCoinsEarned;
        public event Action OnConvertingStarted;

        public IReadOnlyList<BlackHole> BlackHoles => _blackHoles;
        public IReadOnlyList<ConverterLetter> ActiveLetters => _activeLetters;

        public ConverterManager(
            ConverterConfig config,
            PlayerStats playerStats,
            LetterTracker letterTracker,
            LetterConfig letterConfig,
            GameFlowController gameFlow,
            ArenaView arenaView)
        {
            _config = config;
            _playerStats = playerStats;
            _letterTracker = letterTracker;
            _letterConfig = letterConfig;
            _gameFlow = gameFlow;
            _arenaView = arenaView;
        }

        public void Tick()
        {
            if (_gameFlow.State != GameState.Converting) return;

            UpdateBlackHoles(Time.deltaTime);
            UpdateLetters(Time.deltaTime);
        }

        public void StartConverting()
        {
            _activeLetters.Clear();
            _blackHoles.Clear();

            SpawnLettersFromInventory();
            SpawnBlackHoles();

            OnConvertingStarted?.Invoke();
        }

        public void FinishConverting()
        {
            _activeLetters.Clear();
            _blackHoles.Clear();
            _gameFlow.HandleConvertingComplete();
        }

        float GetSpeed() => _config.speedLevels[Mathf.Min(_playerStats.ConverterSpeedLevel, _config.speedLevels.Length - 1)];
        float GetSize() => _config.sizeLevels[Mathf.Min(_playerStats.ConverterSizeLevel, _config.sizeLevels.Length - 1)];
        int GetTotalHoles() => _config.extraHolesLevels[Mathf.Min(_playerStats.ConverterExtraHolesLevel, _config.extraHolesLevels.Length - 1)];
        bool HasAutoMove() => _playerStats.ConverterAutoMoveLevel > 0;
        float GetAutoMoveRatio() => _playerStats.ConverterAutoMoveLevel > 0
            ? _config.autoMoveSpeedRatios[Mathf.Min(_playerStats.ConverterAutoMoveLevel - 1, _config.autoMoveSpeedRatios.Length - 1)]
            : 0f;

        void SpawnLettersFromInventory()
        {
            var center = _arenaView.CenterPosition;

            for (var type = 0; type < 5; type++)
            {
                var count = _letterTracker.GetLetterCount((LetterType)type);

                for (var i = 0; i < count; i++)
                {
                    var randomPos = center + new Vector3(
                        UnityEngine.Random.Range(-_config.letterSpawnSpread, _config.letterSpawnSpread),
                        UnityEngine.Random.Range(-_config.letterSpawnSpread, _config.letterSpawnSpread),
                        0f);

                    var letter = new ConverterLetter((LetterType)type, randomPos);
                    _activeLetters.Add(letter);
                    OnLetterSpawned?.Invoke(letter);
                }

                // Clear inventory for this type (letters are now in the converter)
                while (_letterTracker.GetLetterCount((LetterType)type) > 0)
                    _letterTracker.RemoveLetter((LetterType)type);
            }
        }

        void SpawnBlackHoles()
        {
            var center = _arenaView.CenterPosition;
            var totalHoles = GetTotalHoles();

            _blackHoles.Add(new BlackHole(center, isPlayerControlled: true));

            for (var i = 1; i < totalHoles; i++)
            {
                var angle = (360f / (totalHoles - 1)) * (i - 1) * Mathf.Deg2Rad;
                var offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 3f;
                _blackHoles.Add(new BlackHole(center + offset, isPlayerControlled: false));
            }
        }

        void UpdateBlackHoles(float dt)
        {
            foreach (var hole in _blackHoles)
            {
                if (hole.IsPlayerControlled)
                {
                    UpdatePlayerHole(hole, dt);

                    if (HasAutoMove())
                        ApplyAutoMove(hole, dt, GetAutoMoveRatio());
                }
                else
                {
                    ApplyAutoMove(hole, dt, 1f);
                }

                CollectNearbyLetters(hole);
            }
        }

        void UpdatePlayerHole(BlackHole hole, float dt)
        {
            var input = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.y -= 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input.x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x += 1f;

            if (Input.GetMouseButton(0))
            {
                var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0f;
                var dir = (mouseWorld - hole.Position).normalized;
                input = dir;
            }

            if (input.sqrMagnitude > 0.01f)
            {
                input.Normalize();
                hole.Position += input * GetSpeed() * dt;
            }
        }

        void ApplyAutoMove(BlackHole hole, float dt, float speedRatio)
        {
            if (_activeLetters.Count == 0) return;

            var closest = FindClosestLetter(hole.Position);
            var dir = (closest.Position - hole.Position).normalized;
            hole.Position += dir * GetSpeed() * speedRatio * dt;
        }

        ConverterLetter FindClosestLetter(Vector3 position)
        {
            var closest = _activeLetters[0];
            var closestDist = Vector3.SqrMagnitude(closest.Position - position);

            for (var i = 1; i < _activeLetters.Count; i++)
            {
                var dist = Vector3.SqrMagnitude(_activeLetters[i].Position - position);
                if (dist >= closestDist) continue;

                closest = _activeLetters[i];
                closestDist = dist;
            }

            return closest;
        }

        void UpdateLetters(float dt)
        {
            foreach (var letter in _activeLetters)
            {
                if (!letter.IsBeingSucked) continue;

                var dir = (letter.SuckTarget - letter.Position).normalized;
                letter.Position += dir * _config.suctionForce * dt;
            }
        }

        void CollectNearbyLetters(BlackHole hole)
        {
            var sizeRatio = GetSize() / _config.sizeLevels[0];
            var suctionRadius = _config.suctionRadius * sizeRatio;
            var collectRadius = _config.collectRadius * sizeRatio;

            for (var i = _activeLetters.Count - 1; i >= 0; i--)
            {
                var letter = _activeLetters[i];
                var dist = Vector3.Distance(letter.Position, hole.Position);

                if (dist <= collectRadius)
                {
                    var coins = _letterConfig.GetConversionValue(letter.Type);
                    _letterTracker.DirectAddCoins(coins);
                    OnLetterCollected?.Invoke(letter);
                    OnCoinsEarned?.Invoke(coins);
                    _activeLetters.RemoveAt(i);
                    continue;
                }

                if (dist <= suctionRadius)
                {
                    letter.IsBeingSucked = true;
                    letter.SuckTarget = hole.Position;
                }
            }
        }
    }

    public class ConverterLetter
    {
        public LetterType Type { get; }
        public Vector3 Position { get; set; }
        public bool IsBeingSucked { get; set; }
        public Vector3 SuckTarget { get; set; }

        public ConverterLetter(LetterType type, Vector3 position)
        {
            Type = type;
            Position = position;
        }
    }

    public class BlackHole
    {
        public Vector3 Position { get; set; }
        public bool IsPlayerControlled { get; }

        public BlackHole(Vector3 position, bool isPlayerControlled)
        {
            Position = position;
            IsPlayerControlled = isPlayerControlled;
        }
    }
}

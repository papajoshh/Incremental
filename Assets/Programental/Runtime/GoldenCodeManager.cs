using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class GoldenCodeManager : MonoBehaviour
    {
        [Inject] private CodeTyperMonoBehaviour codeTyperMono;
        [Inject] private GoldenCodeConfig config;
        [Inject] private BonusMultipliers bonusMultipliers;
        [Inject] private DiContainer container;
        [Inject] private IBonusFeedback bonusFeedback;
        [Inject] private List<IGoldenCodeBonus> bonuses;
        [Inject] private GoldenCodeMilestoneTracker milestoneTracker;

        [SerializeField] private GoldenCodeWord wordPrefab;
        [SerializeField] private RectTransform spawnArea;

        private int _poolSize;
        private int _totalPurchased;
        private float _spawnTimer;
        private bool _firstSpawn = true;
        private bool _enabled;
        private readonly List<GoldenCodeWord> _activeWords = new();
        private readonly Dictionary<string, float> _activeBonusTimers = new();
        private string[] _wordList;

        public int PoolSize => _poolSize;
        public int TotalPurchased => _totalPurchased;
        public int WordsCompleted { get; private set; }

        public event Action OnStatsChanged;

        private void Start()
        {
            _wordList = LoadWordList();
        }

        private void OnEnable()
        {
            codeTyperMono.OnKeyPressed += OnKeyPressed;
        }

        private void OnDisable()
        {
            codeTyperMono.OnKeyPressed -= OnKeyPressed;
        }

        private void Update()
        {
            UpdateBonusTimers();

            if (!_enabled || _poolSize <= 0) return;

            _spawnTimer -= Time.deltaTime;
            if (!(_spawnTimer <= 0f)) return;
            SpawnWord();
            _spawnTimer = config.spawnInterval;
        }

        private void UpdateBonusTimers()
        {
            if (_activeBonusTimers.Count == 0) return;

            var expired = new List<string>();
            var keys = new List<string>(_activeBonusTimers.Keys);
            foreach (var key in keys)
            {
                _activeBonusTimers[key] -= Time.deltaTime;
                if (_activeBonusTimers[key] > 0f) continue;
                expired.Add(key);
            }

            foreach (var id in expired)
            {
                _activeBonusTimers.Remove(id);
                var bonus = bonuses.Find(b => b.BonusId == id);
                bonus.Revert();
            }
        }

        public void Enable()
        {
            _enabled = true;
            _spawnTimer = _firstSpawn ? config.firstSpawnDelay : config.spawnInterval;
        }

        public void PurchaseWithLines(int deletedLines)
        {
            var budget = deletedLines;
            while (true)
            {
                var cost = GetCostForNext();
                if (budget < cost) break;
                budget -= cost;
                _totalPurchased++;
                _poolSize++;
            }
            OnStatsChanged?.Invoke();
        }

        private int GetCostForNext()
        {
            return Mathf.RoundToInt(Mathf.Pow(config.costBase, _totalPurchased));
        }


        private void SpawnWord()
        {
            if (_poolSize <= 0) return;
            _poolSize--;
            OnStatsChanged?.Invoke();

            InstantiateWord();

            _firstSpawn = false;
        }
        
        [ContextMenu("Instantiate Word")]
        private void InstantiateWord()
        {
            var word = container.InstantiatePrefabForComponent<GoldenCodeWord>(
                wordPrefab, spawnArea);
            var lifetime = config.wordLifetime + bonusMultipliers.GoldenCodeTimeBonus;
            word.Init(GetRandomWord(), lifetime, spawnArea);
            word.OnCompleted += HandleWordCompleted;
            word.OnExpired += HandleWordExpired;
            _activeWords.Add(word);
        }

        private void OnKeyPressed(char c)
        {
            for (var i = _activeWords.Count - 1; i >= 0; i--)
                _activeWords[i].CheckChar(c);
        }

        private void HandleWordCompleted(GoldenCodeWord word)
        {
            WordsCompleted++;
            _activeWords.Remove(word);
            Destroy(word.gameObject);
            OnStatsChanged?.Invoke();
            milestoneTracker.CheckMilestones(WordsCompleted);
            ApplyRandomBonus();
        }

        private void HandleWordExpired(GoldenCodeWord word)
        {
            _activeWords.Remove(word);
            Destroy(word.gameObject);
        }

        private void ApplyRandomBonus()
        {
            var bonus = WordsCompleted == 1
                ? bonuses.Find(b => b.BonusId == config.firstBonusId)
                : bonuses[UnityEngine.Random.Range(0, bonuses.Count)];

            var info = bonus.Apply();

            if (_activeBonusTimers.ContainsKey(bonus.BonusId))
            {
                _activeBonusTimers[bonus.BonusId] += info.Duration;
                bonusFeedback.ExtendBonus(bonus.BonusId, info.Duration);
            }
            else
            {
                _activeBonusTimers[bonus.BonusId] = info.Duration;
                bonusFeedback.ShowBonus(info);
            }
        }

        private string GetRandomWord()
        {
            return _wordList[UnityEngine.Random.Range(0, _wordList.Length)];
        }

        private string[] LoadWordList()
        {
            var textAsset = Resources.Load<TextAsset>("GoldenCodeWords");
            if (textAsset == null) return new[] { "debug", "class", "void" };
            var lines = textAsset.text.Split('\n');
            var result = new List<string>();
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Length > 0) result.Add(trimmed);
            }
            return result.ToArray();
        }
    }
}

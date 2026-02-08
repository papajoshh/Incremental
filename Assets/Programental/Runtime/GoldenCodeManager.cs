using System.Collections.Generic;
using DG.Tweening;
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

        [SerializeField] private GoldenCodeWord wordPrefab;
        [SerializeField] private RectTransform spawnArea;

        private int _poolSize;
        private int _totalPurchased;
        private float _spawnTimer;
        private bool _firstSpawn = true;
        private bool _enabled;
        private readonly List<GoldenCodeWord> _activeWords = new();
        private string[] _wordList;

        public int PoolSize => _poolSize;
        public int TotalPurchased => _totalPurchased;
        public int WordsCompleted { get; private set; }

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
            if (!_enabled || _poolSize <= 0) return;

            _spawnTimer -= Time.deltaTime;
            if (!(_spawnTimer <= 0f)) return;
            SpawnWord();
            _spawnTimer = config.spawnInterval;
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
        }

        private int GetCostForNext()
        {
            return Mathf.RoundToInt(Mathf.Pow(config.costBase, _totalPurchased));
        }


        private void SpawnWord()
        {
            if (_poolSize <= 0) return;
            _poolSize--;

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
            ApplyRandomBonus();
        }

        private void HandleWordExpired(GoldenCodeWord word)
        {
            _activeWords.Remove(word);
            Destroy(word.gameObject);
        }

        private void ApplyRandomBonus()
        {
            var roll = Random.Range(0, 3);
            switch (roll)
            {
                case 0:
                    bonusMultipliers.LineMultiplier = config.lineMultiplierValue;
                    DOVirtual.DelayedCall(config.bonusDuration, () =>
                        bonusMultipliers.LineMultiplier = 1);
                    break;
                case 1:
                    bonusMultipliers.CharsPerKeypress = config.charsPerKeypressValue;
                    DOVirtual.DelayedCall(config.bonusDuration, () =>
                        bonusMultipliers.CharsPerKeypress = 1);
                    break;
                case 2:
                    bonusMultipliers.GoldenCodeTimeBonus = config.goldenCodeTimeBonus;
                    DOVirtual.DelayedCall(config.bonusDuration, () =>
                        bonusMultipliers.GoldenCodeTimeBonus = 0f);
                    break;
            }
        }

        private string GetRandomWord()
        {
            return _wordList[Random.Range(0, _wordList.Length)];
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

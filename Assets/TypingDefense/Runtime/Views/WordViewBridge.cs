using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class WordViewBridge : MonoBehaviour
    {
        WordManager wordManager;
        DefenseWordView.Factory wordFactory;
        DiContainer container;
        ArenaView arenaView;
        LevelProgressionConfig levelConfig;
        RunManager runManager;
        GameFlowController gameFlow;

        readonly Dictionary<DefenseWord, DefenseWordView> activeViews = new();
        readonly Dictionary<DefenseWord, Vector3> lastKnownPositions = new();
        BossWordView activeBossView;

        [Inject]
        public void Construct(
            WordManager wordManager,
            DefenseWordView.Factory wordFactory,
            DiContainer container,
            ArenaView arenaView,
            LevelProgressionConfig levelConfig,
            RunManager runManager,
            GameFlowController gameFlow)
        {
            this.wordManager = wordManager;
            this.wordFactory = wordFactory;
            this.container = container;
            this.arenaView = arenaView;
            this.levelConfig = levelConfig;
            this.runManager = runManager;
            this.gameFlow = gameFlow;

            gameFlow.OnStateChanged += OnStateChanged;
            wordManager.OnWordSpawned += OnWordSpawned;
            wordManager.OnWordCompleted += OnWordCompleted;
            wordManager.OnWordCriticalKill += OnWordCriticalKill;
            wordManager.OnWordReachedCenter += OnWordReachedCenter;
            wordManager.OnBossSpawned += OnBossSpawned;
            wordManager.OnBossHit += OnBossHit;
            wordManager.OnBossDefeated += OnBossDefeated;
            wordManager.OnWordTextChanged += OnWordTextChanged;
            wordManager.OnAllWordsDissipated += OnAllWordsDissipated;
            wordManager.OnExternalWordSpawned += OnExternalWordSpawned;
            wordManager.OnAutoTargetAcquired += OnAutoTargetAcquired;
            wordManager.OnAutoTargetLost += OnAutoTargetLost;
        }

        void OnDestroy()
        {
            gameFlow.OnStateChanged -= OnStateChanged;
            wordManager.OnWordSpawned -= OnWordSpawned;
            wordManager.OnWordCompleted -= OnWordCompleted;
            wordManager.OnWordCriticalKill -= OnWordCriticalKill;
            wordManager.OnWordReachedCenter -= OnWordReachedCenter;
            wordManager.OnBossSpawned -= OnBossSpawned;
            wordManager.OnBossHit -= OnBossHit;
            wordManager.OnBossDefeated -= OnBossDefeated;
            wordManager.OnWordTextChanged -= OnWordTextChanged;
            wordManager.OnAllWordsDissipated -= OnAllWordsDissipated;
            wordManager.OnExternalWordSpawned -= OnExternalWordSpawned;
            wordManager.OnAutoTargetAcquired -= OnAutoTargetAcquired;
            wordManager.OnAutoTargetLost -= OnAutoTargetLost;
        }

        public Vector3 GetWordPosition(DefenseWord word)
        {
            if (activeViews.TryGetValue(word, out var view))
                return view.LastPosition;

            if (lastKnownPositions.TryGetValue(word, out var cached))
            {
                lastKnownPositions.Remove(word);
                return cached;
            }

            return arenaView.CenterPosition;
        }

        void OnStateChanged(GameState state)
        {
            if (state == GameState.Playing || state == GameState.Collecting) return;

            DestroyAllViews();
        }

        void DestroyAllViews()
        {
            foreach (var kvp in activeViews)
                Destroy(kvp.Value.gameObject);
            activeViews.Clear();

            if (activeBossView != null)
            {
                Destroy(activeBossView.gameObject);
                activeBossView = null;
            }
        }

        void OnWordSpawned(DefenseWord word)
        {
            var view = wordFactory.Create();
            var startPos = arenaView.GetRandomEdgePosition();
            var config = levelConfig.GetLevel(runManager.CurrentLevel);
            var speed = config.wordSpeed;
            var variance = Random.Range(-0.3f, 0.3f);
            speed *= (1f + variance);
            view.Setup(word, startPos, arenaView.CenterPosition, speed);
            activeViews[word] = view;
        }

        void OnWordCompleted(DefenseWord word)
        {
            if (!activeViews.TryGetValue(word, out var view)) return;
            lastKnownPositions[word] = view.LastPosition;
            view.OnCompleted();
            activeViews.Remove(word);
        }

        void OnWordCriticalKill(DefenseWord word)
        {
            if (!activeViews.TryGetValue(word, out var view)) return;
            lastKnownPositions[word] = view.LastPosition;
            view.OnCriticalKill();
            activeViews.Remove(word);
        }

        void OnWordReachedCenter(DefenseWord word)
        {
            activeViews.Remove(word);
        }

        void OnBossSpawned(DefenseWord word)
        {
            var config = levelConfig.GetLevel(runManager.CurrentLevel);
            activeBossView = container.InstantiatePrefabForComponent<BossWordView>(config.bossPrefab);
            activeBossView.Setup(word, arenaView.CenterPosition);
        }

        void OnBossHit(DefenseWord word)
        {
            activeBossView.OnHit();
        }

        void OnBossDefeated(DefenseWord word)
        {
            activeBossView.OnDefeated();
            activeBossView = null;
        }

        void OnWordTextChanged(DefenseWord word, string newText)
        {
            if (activeViews.TryGetValue(word, out var view))
            {
                view.OnTextChanged();
                return;
            }

            if (activeBossView != null)
                activeBossView.OnTextChanged();
        }

        void OnExternalWordSpawned(DefenseWord word, Vector3 spawnPos, float speed)
        {
            var view = wordFactory.Create();
            view.Setup(word, spawnPos, arenaView.CenterPosition, speed);
            activeViews[word] = view;

            if (word.Type == WordType.Blue)
            {
                view.transform.localScale = new Vector3(2f, 0.2f, 1f);
                view.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }
        }

        void OnAllWordsDissipated(IReadOnlyList<DefenseWord> words)
        {
            foreach (var word in words)
            {
                if (!activeViews.TryGetValue(word, out var view)) continue;
                view.OnDissipated();
                activeViews.Remove(word);
            }
        }

        void OnAutoTargetAcquired(DefenseWord word)
        {
            if (activeViews.TryGetValue(word, out var view))
            {
                view.SetTargeted(true);
                return;
            }

            if (activeBossView != null && word.IsBoss)
                activeBossView.SetTargeted(true);
        }

        void OnAutoTargetLost(DefenseWord word)
        {
            if (activeViews.TryGetValue(word, out var view))
            {
                view.SetTargeted(false);
                return;
            }

            if (activeBossView != null && word.IsBoss)
                activeBossView.SetTargeted(false);
        }
    }
}

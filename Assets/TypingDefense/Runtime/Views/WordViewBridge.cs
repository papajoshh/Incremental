using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class WordViewBridge : MonoBehaviour
    {
        WordManager wordManager;
        DefenseWordView.Factory wordFactory;
        BossWordView.Factory bossFactory;
        ArenaView arenaView;
        WordSpawnConfig spawnConfig;
        RunManager runManager;

        readonly Dictionary<DefenseWord, DefenseWordView> activeViews = new();
        DefenseWordView warpView;
        BossWordView activeBossView;

        [Inject]
        public void Construct(
            WordManager wordManager,
            DefenseWordView.Factory wordFactory,
            BossWordView.Factory bossFactory,
            ArenaView arenaView,
            WordSpawnConfig spawnConfig,
            RunManager runManager)
        {
            this.wordManager = wordManager;
            this.wordFactory = wordFactory;
            this.bossFactory = bossFactory;
            this.arenaView = arenaView;
            this.spawnConfig = spawnConfig;
            this.runManager = runManager;
        }

        void OnEnable()
        {
            wordManager.OnWordSpawned += OnWordSpawned;
            wordManager.OnWordCompleted += OnWordCompleted;
            wordManager.OnWordCriticalKill += OnWordCriticalKill;
            wordManager.OnWordReachedCenter += OnWordReachedCenter;
            wordManager.OnWarpAvailable += OnWarpAvailable;
            wordManager.OnWarpCompleted += OnWarpCompleted;
            wordManager.OnBossSpawned += OnBossSpawned;
            wordManager.OnBossHit += OnBossHit;
            wordManager.OnBossDefeated += OnBossDefeated;
            wordManager.OnWordTextChanged += OnWordTextChanged;
        }

        void OnDisable()
        {
            wordManager.OnWordSpawned -= OnWordSpawned;
            wordManager.OnWordCompleted -= OnWordCompleted;
            wordManager.OnWordCriticalKill -= OnWordCriticalKill;
            wordManager.OnWordReachedCenter -= OnWordReachedCenter;
            wordManager.OnWarpAvailable -= OnWarpAvailable;
            wordManager.OnWarpCompleted -= OnWarpCompleted;
            wordManager.OnBossSpawned -= OnBossSpawned;
            wordManager.OnBossHit -= OnBossHit;
            wordManager.OnBossDefeated -= OnBossDefeated;
            wordManager.OnWordTextChanged -= OnWordTextChanged;
        }

        void OnWordSpawned(DefenseWord word)
        {
            var view = wordFactory.Create();
            var startPos = arenaView.GetRandomEdgePosition();
            var speed = spawnConfig.baseWordSpeed
                        + (runManager.CurrentLevel - 1) * spawnConfig.wordSpeedScalePerLevel;
            var variance = Random.Range(-spawnConfig.speedVariance, spawnConfig.speedVariance);
            speed *= (1f + variance);
            view.Setup(word, startPos, arenaView.CenterPosition, speed);
            activeViews[word] = view;
        }

        void OnWordCompleted(DefenseWord word)
        {
            if (!activeViews.TryGetValue(word, out var view)) return;
            view.OnCompleted();
            activeViews.Remove(word);
        }

        void OnWordCriticalKill(DefenseWord word)
        {
            if (!activeViews.TryGetValue(word, out var view)) return;
            view.OnCriticalKill();
            activeViews.Remove(word);
        }

        void OnWordReachedCenter(DefenseWord word)
        {
            activeViews.Remove(word);
        }

        void OnWarpAvailable(DefenseWord word)
        {
            var view = wordFactory.Create();
            var warpPos = arenaView.CenterPosition + Vector3.up * 3f;
            view.Setup(word, warpPos, warpPos, 0f);
            warpView = view;
        }

        void OnWarpCompleted()
        {
            if (warpView == null) return;
            warpView.OnCompleted();
            warpView = null;
        }

        void OnBossSpawned(DefenseWord word)
        {
            activeBossView = bossFactory.Create();
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
    }
}

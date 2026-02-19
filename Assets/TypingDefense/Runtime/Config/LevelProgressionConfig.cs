using System;
using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "LevelProgressionConfig", menuName = "TypingDefense/Level Progression Config")]
    public class LevelProgressionConfig : ScriptableObject
    {
        public LevelConfig[] levels;

        public LevelConfig GetLevel(int levelNumber)
        {
            var index = Mathf.Clamp(levelNumber - 1, 0, levels.Length - 1);
            return levels[index];
        }

        public int TotalLevels => levels.Length;
    }

    [Serializable]
    public class LevelConfig
    {
        [Header("Display")]
        public string displayName;

        [Header("Word Spawning")]
        public float spawnInterval = 3f;
        public float wordSpeed = 0.8f;
        public int minWordLength = 4;
        public int maxWordLength = 7;
        public int minWordHp = 1;
        public int maxWordHp = 1;

        [Header("Progression")]
        public int killsForBoss = 20;

        [Header("Boss")]
        public BossWordView bossPrefab;
        public int bossHp = 40;
        public int bossPrestigeReward = 10;

        [Header("Energy")]
        public float drainInterval = 5f;
    }
}

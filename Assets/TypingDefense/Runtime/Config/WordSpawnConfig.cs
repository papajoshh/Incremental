using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "WordSpawnConfig", menuName = "TypingDefense/Word Spawn Config")]
    public class WordSpawnConfig : ScriptableObject
    {
        public float baseSpawnInterval = 3f;
        public float baseWordSpeed = 0.8f;
        public float speedVariance = 0.3f;
        public float spawnIntervalScalePerLevel = 0.2f;
        public float wordSpeedScalePerLevel = 0.1f;
    }
}

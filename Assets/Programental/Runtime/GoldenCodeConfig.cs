using UnityEngine;

namespace Programental
{
    [CreateAssetMenu(fileName = "GoldenCodeConfig", menuName = "Programental/GoldenCodeConfig")]
    public class GoldenCodeConfig : ScriptableObject
    {
        [Header("Spawn")]
        public float firstSpawnDelay = 10f;
        public float spawnInterval = 300f;

        [Header("Word")]
        public float wordLifetime = 15f;

        [Header("Cost Curve")]
        public float costBase = 2f;

        [Header("Bonuses")]
        public string firstBonusId = "LineMultiplier";

        [Header("Line Multiplier")]
        public int lineMultiplierValue = 2;
        public float lineMultiplierDuration = 20f;

        [Header("Speed")]
        public int charsPerKeypressValue = 2;
        public float speedBonusDuration = 20f;

        [Header("Duration")]
        public float durationMultiplierFactor = 2f;
        public float durationBonusDuration = 120f;
    }
}

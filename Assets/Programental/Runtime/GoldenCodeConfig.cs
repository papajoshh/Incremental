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
        public float bonusDuration = 30f;
        public int lineMultiplierValue = 2;
        public int charsPerKeypressValue = 2;
        public float goldenCodeTimeBonus = 10f;
    }
}

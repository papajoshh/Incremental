using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "RunConfig", menuName = "TypingDefense/Run Config")]
    public class RunConfig : ScriptableObject
    {
        public float baseDrainInterval = 5f;
        public float drainScalePerLevel = 0.15f;
        public int killsToWarp = 20;
        public int baseMaxHp = 1;
        public float baseMaxEnergy = 5f;
    }
}

using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "BossConfig", menuName = "TypingDefense/Boss Config")]
    public class BossConfig : ScriptableObject
    {
        public int bossLevel = 5;
        public int bossHp = 40;
        public float orbitalSpeed = 45f;
        public float orbitalRadius = 7f;
        public int prestigeReward = 10;
    }
}

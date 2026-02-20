using System;
using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "WallConfig", menuName = "TypingDefense/Wall Config")]
    public class WallConfig : ScriptableObject
    {
        public WallRingConfig[] rings;

        [Header("Blue Words")]
        public float blueWordSpawnInterval = 8f;
        public float blueWordSpeed = 1.2f;
        public int blueWordMinLength = 3;
        public int blueWordMaxLength = 5;
        public int blueWordCoinReward = 25;

        [Header("Segment Visuals")]
        public float segmentLineWidth = 0.08f;
        public Color wallLineColor = new(0.2f, 0.6f, 1f);
        public Color wallLineBreakColor = new(0.4f, 0.9f, 1f);
        public float wordRevealAlpha = 0.9f;
    }

    [Serializable]
    public class WallRingConfig
    {
        public float width = 16f;
        public float height = 9f;
        public int segmentsPerSide = 4;
        public int wallWordMinLength = 3;
        public int wallWordMaxLength = 6;
    }
}

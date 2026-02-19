using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "CollectionPhaseConfig", menuName = "TypingDefense/Collection Phase Config")]
    public class CollectionPhaseConfig : ScriptableObject
    {
        public float slowMotionScale = 0.3f;
        public float collectRadius = 0.8f;
        public float wordHomingSpeed = 1.0f;
        public float letterDriftSpeed = 0.15f;
        public float transitionOutDuration = 0.6f;

        [Header("Charge Sequence")]
        public float chargeDuration = 3f;
        public float zoomAmount = 0.3f;
        public float chargeShakeIntensity = 0.15f;
        public float releaseShakeIntensity = 0.5f;
        public float releaseShakeDuration = 0.4f;
    }
}

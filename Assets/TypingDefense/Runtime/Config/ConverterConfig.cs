using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "ConverterConfig", menuName = "TypingDefense/Converter Config")]
    public class ConverterConfig : ScriptableObject
    {
        public float[] speedLevels = { 3f, 4.5f, 6.5f, 9f, 12f };
        public float[] sizeLevels = { 0.8f, 1.2f, 1.6f, 2.2f, 3.0f };
        public float[] autoMoveSpeedRatios = { 0.7f, 0.85f, 1.0f };
        public int[] extraHolesLevels = { 1, 2, 3 };

        public float suctionRadius = 1.2f;
        public float suctionForce = 15f;
        public float collectRadius = 0.3f;
        public float letterSpawnSpread = 8f;
    }
}

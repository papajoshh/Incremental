using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "ConverterConfig", menuName = "TypingDefense/Converter Config")]
    public class ConverterConfig : ScriptableObject
    {
        public float suctionRadius = 1.2f;
        public float suctionForce = 15f;
        public float collectRadius = 0.3f;
    }
}

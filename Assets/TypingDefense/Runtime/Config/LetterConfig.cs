using System;
using UnityEngine;

namespace TypingDefense
{
    [CreateAssetMenu(fileName = "LetterConfig", menuName = "TypingDefense/Letter Config")]
    public class LetterConfig : ScriptableObject
    {
        public LetterValue[] letterValues =
        {
            new() { type = LetterType.A, conversionValue = 1 },
            new() { type = LetterType.B, conversionValue = 3 },
            new() { type = LetterType.C, conversionValue = 9 },
            new() { type = LetterType.D, conversionValue = 27 },
            new() { type = LetterType.E, conversionValue = 81 }
        };

        public int GetConversionValue(LetterType type)
        {
            foreach (var entry in letterValues)
            {
                if (entry.type == type) return entry.conversionValue;
            }

            return 1;
        }
    }

    [Serializable]
    public struct LetterValue
    {
        public LetterType type;
        public int conversionValue;
    }
}

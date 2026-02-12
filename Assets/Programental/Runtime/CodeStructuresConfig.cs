using System;
using UnityEngine;

namespace Programental
{
    [CreateAssetMenu(fileName = "CodeStructuresConfig", menuName = "Programental/Code Structures Config")]
    public class CodeStructuresConfig : ScriptableObject
    {
        public bool abilityScalesWithAvailable = true;
        public int autoKeystrokesPerSecPerLevel = 1;
        public StructureDefinition[] structures;
    }

    [Serializable]
    public class StructureDefinition
    {
        public string id;
        public string localizationKey;
        public float baseCost = 30f;
        public float growthRate = 1.15f;
        public string abilityId;
    }
}

using System;
using UnityEngine;

namespace Programental
{
    [CreateAssetMenu(fileName = "CodeStructuresConfig", menuName = "Programental/Code Structures Config")]
    public class CodeStructuresConfig : ScriptableObject
    {
        public bool abilityScalesWithAvailable = true;
        public float autoTypeBaseInterval = 5f;
        public StructureDefinition[] structures;
    }

    [Serializable]
    public class StructureDefinition
    {
        public string id;
        public string localizationKey;
        public float costBase = 2f;
        public string abilityId;
    }
}

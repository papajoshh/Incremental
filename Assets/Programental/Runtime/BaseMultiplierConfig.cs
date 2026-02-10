using UnityEngine;

namespace Programental
{
    [CreateAssetMenu(fileName = "BaseMultiplierConfig", menuName = "Programental/BaseMultiplierConfig")]
    public class BaseMultiplierConfig : ScriptableObject
    {
        [Header("Progression")]
        public float costBase = 2f;
        public float levelIncrement = 0.05f;
    }
}

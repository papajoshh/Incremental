using System;
using UnityEngine;

namespace Programental
{
    [Serializable]
    public struct Milestone
    {
        public int linesRequired;
        public string rewardId;
    }

    [CreateAssetMenu(fileName = "MilestonesConfig", menuName = "Programental/MilestonesConfig")]
    public class MilestonesConfig : ScriptableObject
    {
        public Milestone[] milestones;
    }
}

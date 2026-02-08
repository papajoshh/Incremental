using System;
using UnityEngine;

namespace Programental
{
    [Serializable]
    public struct GoldenCodeMilestone
    {
        public int goldenCodesRequired;
        public string rewardId;
    }

    [CreateAssetMenu(fileName = "GoldenCodeMilestonesConfig", menuName = "Programental/GoldenCodeMilestonesConfig")]
    public class GoldenCodeMilestonesConfig : ScriptableObject
    {
        public GoldenCodeMilestone[] milestones;
    }
}

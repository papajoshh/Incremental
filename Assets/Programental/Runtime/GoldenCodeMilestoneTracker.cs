using System.Collections.Generic;

namespace Programental
{
    public class GoldenCodeMilestoneTracker
    {
        private readonly GoldenCodeMilestone[] _milestones;
        private readonly Dictionary<string, GoldenCodeMilestoneReward> _rewards = new();
        private int _nextMilestoneIndex;

        public GoldenCodeMilestoneTracker(GoldenCodeMilestonesConfig config)
        {
            _milestones = config.milestones;
        }

        public void Register(string rewardId, GoldenCodeMilestoneReward reward)
        {
            _rewards[rewardId] = reward;
        }

        public void CheckMilestones(int totalGoldenCodes)
        {
            while (_nextMilestoneIndex < _milestones.Length &&
                   _milestones[_nextMilestoneIndex].goldenCodesRequired <= totalGoldenCodes)
            {
                var id = _milestones[_nextMilestoneIndex].rewardId;
                if (_rewards.TryGetValue(id, out var reward))
                    reward.Unlock();
                _nextMilestoneIndex++;
            }
        }
    }
}

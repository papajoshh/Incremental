using System.Collections.Generic;

namespace Programental
{
    public class MilestoneTracker
    {
        private readonly Milestone[] _milestones;
        private readonly Dictionary<string, MilestoneReward> _rewards = new();
        private int _nextMilestoneIndex;

        public MilestoneTracker(MilestonesConfig config)
        {
            _milestones = config.milestones;
        }

        public void Register(string rewardId, MilestoneReward reward)
        {
            _rewards[rewardId] = reward;
        }

        public void CheckMilestones(int totalLines)
        {
            while (_nextMilestoneIndex < _milestones.Length &&
                   _milestones[_nextMilestoneIndex].linesRequired <= totalLines)
            {
                var id = _milestones[_nextMilestoneIndex].rewardId;
                if (_rewards.TryGetValue(id, out var reward))
                    reward.Unlock();
                _nextMilestoneIndex++;
            }
        }
    }
}

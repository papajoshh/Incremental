using System;

namespace Programental
{
    public class LinesTracker
    {
        private readonly MilestoneTracker _milestoneTracker;
        private readonly BonusMultipliers _bonusMultipliers;

        public int TotalLinesEver { get; private set; }
        public int TotalLinesDeleted { get; private set; }
        public int AvailableLines => TotalLinesEver - TotalLinesDeleted;

        public event Action<int> OnAvailableLinesChanged;

        public LinesTracker(MilestoneTracker milestoneTracker, BonusMultipliers bonusMultipliers)
        {
            _milestoneTracker = milestoneTracker;
            _bonusMultipliers = bonusMultipliers;
        }

        public void AddCompletedLine()
        {
            TotalLinesEver += _bonusMultipliers.LineMultiplier;
            OnAvailableLinesChanged?.Invoke(AvailableLines);
            _milestoneTracker.CheckMilestones(AvailableLines);
        }

        public int DeleteAllLines()
        {
            int deleted = AvailableLines;
            TotalLinesDeleted += deleted;
            OnAvailableLinesChanged?.Invoke(AvailableLines);
            return deleted;
        }
    }
}

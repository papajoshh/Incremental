using System;

namespace Programental
{
    public class LinesTracker
    {
        private readonly MilestoneTracker _milestoneTracker;
        private readonly BonusMultipliers _bonusMultipliers;
        private float _fractionalAccumulator;

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
            _fractionalAccumulator += _bonusMultipliers.TotalLineMultiplier;
            var linesEarned = (int)_fractionalAccumulator;
            _fractionalAccumulator -= linesEarned;
            TotalLinesEver += linesEarned;
            OnAvailableLinesChanged?.Invoke(AvailableLines);
            _milestoneTracker.CheckMilestones(AvailableLines);
        }

        public bool TrySpendLines(int cost)
        {
            if (AvailableLines < cost) return false;
            TotalLinesDeleted += cost;
            OnAvailableLinesChanged?.Invoke(AvailableLines);
            return true;
        }

        public int DeleteAllLines()
        {
            var deleted = AvailableLines;
            TotalLinesDeleted += deleted;
            OnAvailableLinesChanged?.Invoke(AvailableLines);
            return deleted;
        }

        public LinesData CaptureState()
        {
            return new LinesData
            {
                totalLinesEver = TotalLinesEver,
                totalLinesDeleted = TotalLinesDeleted,
                fractionalAccumulator = _fractionalAccumulator
            };
        }

        public void RestoreState(LinesData data)
        {
            TotalLinesEver = data.totalLinesEver;
            TotalLinesDeleted = data.totalLinesDeleted;
            _fractionalAccumulator = data.fractionalAccumulator;
            OnAvailableLinesChanged?.Invoke(AvailableLines);
        }
    }
}

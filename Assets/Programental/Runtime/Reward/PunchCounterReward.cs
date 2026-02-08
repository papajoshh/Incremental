using Zenject;

namespace Programental
{
    public class PunchCounterReward : MilestoneReward
    {
        public override string RewardId => "PunchCounter";

        [Inject] private LineCounterView counterView;

        public override void OnUnlock() => counterView.SetPunchOnIncrement(true);

        public override void Restore()
        {
            base.Restore();
            counterView.SetPunchOnIncrement(true);
        }
    }
}

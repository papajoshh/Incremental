using Zenject;

namespace Programental
{
    public class DescriptiveCounterReward : MilestoneReward
    {
        public override string RewardId => "DescriptiveCounter";

        [Inject] private LineCounterView counterView;

        public override void OnUnlock() => counterView.ShowLabel(true);

        public override void Restore()
        {
            base.Restore();
            counterView.ShowLabel(true);
        }
    }
}

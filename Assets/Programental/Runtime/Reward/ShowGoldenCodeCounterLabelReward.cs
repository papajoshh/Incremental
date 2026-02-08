using Zenject;

namespace Programental
{
    public class ShowGoldenCodeCounterLabelReward : GoldenCodeMilestoneReward
    {
        public override string RewardId => "ShowGoldenCodeCounterLabel";

        [Inject] private GoldenCodeCounterView counterView;

        public override void OnUnlock() => counterView.ShowLabels();

        public override void Restore()
        {
            base.Restore();
            counterView.ShowLabels(false);
        }
    }
}

using Zenject;

namespace Programental
{
    public class ShowGoldenCodeCounterReward : GoldenCodeMilestoneReward
    {
        public override string RewardId => "ShowGoldenCodeCounter";

        [Inject] private GoldenCodeCounterView counterView;

        public override void OnUnlock() => counterView.Show();

        public override void Restore()
        {
            base.Restore();
            counterView.Show(false);
        }
    }
}

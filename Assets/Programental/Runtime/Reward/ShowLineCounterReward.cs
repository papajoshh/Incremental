using Zenject;

namespace Programental
{
    public class ShowLineCounterReward : MilestoneReward
    {
        public override string RewardId => "ShowLineCounter";

        [Inject] private LineCounterView counterView;

        public override void OnUnlock() => counterView.Show();

        public override void Restore()
        {
            base.Restore();
            counterView.Show(false);
        }
    }
}

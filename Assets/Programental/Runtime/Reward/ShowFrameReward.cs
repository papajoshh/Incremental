using Zenject;

namespace Programental
{
    public class ShowFrameReward : MilestoneReward
    {
        public override string RewardId => "ShowFrame";

        [Inject] private LineCounterView counterView;

        public override void OnUnlock() => counterView.ShowFrame(true);

        public override void Restore()
        {
            base.Restore();
            counterView.ShowFrame(true);
        }
    }
}

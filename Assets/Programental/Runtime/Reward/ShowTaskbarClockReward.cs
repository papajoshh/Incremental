using Zenject;

namespace Programental
{
    public class ShowTaskbarClockReward : MilestoneReward
    {
        public override string RewardId => "ShowTaskbarClock";

        [Inject] private TaskbarView taskbarView;

        public override void OnUnlock() => taskbarView.ShowClock();

        public override void Restore()
        {
            base.Restore();
            taskbarView.ShowClock(false);
        }
    }
}

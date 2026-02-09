using Zenject;

namespace Programental
{
    public class ShowUpgradesTabReward : MilestoneReward
    {
        public override string RewardId => "ShowUpgradesTab";

        [Inject] private TaskbarView taskbarView;

        public override void OnUnlock() => taskbarView.ShowUpgradesTab();

        public override void Restore()
        {
            base.Restore();
            taskbarView.ShowUpgradesTab(false);
        }
    }
}

using Zenject;

namespace Programental
{
    public class ShowCodeTabReward : MilestoneReward
    {
        public override string RewardId => "ShowCodeTab";

        [Inject] private TaskbarView taskbarView;

        public override void OnUnlock() => taskbarView.ShowCodeTab();

        public override void Restore()
        {
            base.Restore();
            taskbarView.ShowCodeTab(false);
        }
    }
}

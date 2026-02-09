using Zenject;

namespace Programental
{
    public class ShowTaskbarReward : MilestoneReward
    {
        public override string RewardId => "ShowTaskbar";

        [Inject] private TaskbarView taskbarView;

        public override void OnUnlock() => taskbarView.ShowBar();

        public override void Restore()
        {
            base.Restore();
            taskbarView.ShowBar(false);
        }
    }
}

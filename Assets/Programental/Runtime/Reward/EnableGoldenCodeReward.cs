using Zenject;

namespace Programental
{
    public class EnableGoldenCodeReward : MilestoneReward
    {
        public override string RewardId => "EnableGoldenCode";

        [Inject] private GoldenCodeManager goldenCodeManager;

        public override void OnUnlock() => goldenCodeManager.Enable();

        public override void Restore()
        {
            base.Restore();
            goldenCodeManager.Enable();
        }
    }
}

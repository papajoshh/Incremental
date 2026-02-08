using Zenject;

namespace Programental
{
    public class ShowDeleteButtonReward : MilestoneReward
    {
        public override string RewardId => "ShowDeleteButton";

        [Inject] private DeleteCodeButtonView buttonView;

        public override void OnUnlock() => buttonView.Show();

        public override void Restore()
        {
            base.Restore();
            buttonView.Show(false);
        }
    }
}

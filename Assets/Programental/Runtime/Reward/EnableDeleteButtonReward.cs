using Zenject;

namespace Programental
{
    public class EnableDeleteButtonReward : MilestoneReward
    {
        public override string RewardId => "EnableDeleteButton";

        [Inject] private DeleteCodeButtonView buttonView;

        public override void OnUnlock() => buttonView.EnableFunctionality();

        public override void Restore()
        {
            base.Restore();
            buttonView.EnableFunctionality(false);
        }
    }
}

using UnityEngine;

namespace Programental
{
    public class ShowCodeStructuresScreenReward : MilestoneReward
    {
        public override string RewardId => "ShowCodeStructuresScreen";
        [SerializeField] private CodeStructuresScreenView screenView;

        public override void OnUnlock() => screenView.Show();
        public override void Restore() { base.Restore(); screenView.Show(false); }
    }
}

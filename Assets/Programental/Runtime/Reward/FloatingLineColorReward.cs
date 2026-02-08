using TMPro;
using UnityEngine;

namespace Programental
{
    public class FloatingLineColorReward : MilestoneReward
    {
        public override string RewardId => "FloatingLineColor";

        [SerializeField] private FloatingLineReward floatingLineReward;
        [SerializeField] private Color lineColor = new Color(0.2f, 0.8f, 0.2f);

        private void OnEnable()
        {
            if (floatingLineReward != null) floatingLineReward.OnFloatingLineSpawned += OnSpawned;
        }

        private void OnDisable()
        {
            if (floatingLineReward != null) floatingLineReward.OnFloatingLineSpawned -= OnSpawned;
        }

        public override void OnUnlock() { }

        public override void Restore()
        {
            base.Restore();
        }

        private void OnSpawned(TextMeshProUGUI tmp)
        {
            if (!Unlocked) return;
            tmp.color = lineColor;
        }
    }
}

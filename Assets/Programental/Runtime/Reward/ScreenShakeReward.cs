using UnityEngine;
using Zenject;

namespace Programental
{
    public class ScreenShakeReward : MilestoneReward
    {
        public override string RewardId => "ScreenShake";

        [Inject] private CodeTyper codeTyper;
        [Inject] private ScreenShaker screenShaker;

        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float cameraShakeStrength = 0.3f;
        [SerializeField] private float uiShakeStrength = 15f;

        private void OnEnable()
        {
            codeTyper.OnLineCompleted += OnLine;
        }

        private void OnDisable()
        {
            codeTyper.OnLineCompleted -= OnLine;
        }

        public override void OnUnlock() { }

        public override void Restore()
        {
            base.Restore();
        }

        private void OnLine(string _, int __)
        {
            if (!Unlocked) return;
            screenShaker.Shake(shakeDuration, cameraShakeStrength, uiShakeStrength);
        }
    }
}

using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class HitstopReward : MilestoneReward
    {
        public override string RewardId => "Hitstop";

        [Inject] private CodeTyper codeTyper;
        [SerializeField] private float hitstopDuration = 0.06f;

        private void OnEnable()
        {
            if (codeTyper != null) codeTyper.OnLineCompleted += OnLine;
        }

        private void OnDisable()
        {
            if (codeTyper != null) codeTyper.OnLineCompleted -= OnLine;
        }

        public override void OnUnlock() { }

        public override void Restore()
        {
            base.Restore();
        }

        private void OnLine(string _, int __)
        {
            if (!Unlocked) return;
            Time.timeScale = 0f;
            DOVirtual.DelayedCall(hitstopDuration, () => Time.timeScale = 1f).SetUpdate(true);
        }
    }
}

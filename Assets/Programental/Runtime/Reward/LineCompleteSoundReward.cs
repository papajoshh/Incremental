using Zenject;

namespace Programental
{
    public class LineCompleteSoundReward : MilestoneReward
    {
        public override string RewardId => "LineCompleteSound";

        [Inject] private CodeTyper codeTyper;
        [Inject] private AudioPlayer audioPlayer;

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

        private void OnLine(string completedLine, int totalLines)
        {
            if (!Unlocked) return;
            audioPlayer.PlaySfx("linecomplete");
        }
    }
}

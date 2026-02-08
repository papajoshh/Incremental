using Zenject;

namespace Programental
{
    public class KeypressSoundReward : MilestoneReward
    {
        public override string RewardId => "KeypressSound";

        [Inject] private CodeTyper codeTyper;
        [Inject] private AudioPlayer audioPlayer;

        private void OnEnable()
        {
            codeTyper.OnCharTyped += OnChar;
        }

        private void OnDisable()
        {
            codeTyper.OnCharTyped -= OnChar;
        }

        public override void OnUnlock() { }

        public override void Restore()
        {
            base.Restore();
        }

        private void OnChar(char c, string visibleText)
        {
            if (!Unlocked || c == '\0') return;
            audioPlayer.PlaySfx("keypress");
        }
    }
}

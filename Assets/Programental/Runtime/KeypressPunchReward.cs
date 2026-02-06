using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class KeypressPunchReward : MilestoneReward
    {
        [Inject] private CodeTyper codeTyper;
        [SerializeField] private TextMeshProUGUI codeText;
        [SerializeField] private Vector3 punchScale = new Vector3(0.03f, 0.03f, 0);
        [SerializeField] private float punchDuration = 0.12f;

        private void OnEnable()
        {
            if (codeTyper != null) codeTyper.OnCharTyped += OnChar;
        }

        private void OnDisable()
        {
            if (codeTyper != null) codeTyper.OnCharTyped -= OnChar;
        }

        public override void OnUnlock() { }

        public override void Restore()
        {
            base.Restore();
        }

        private void OnChar(char c, string _)
        {
            if (!Unlocked || c == '\0') return;
            codeText.transform.DOComplete();
            codeText.transform.DOPunchScale(punchScale, punchDuration, 6, 0);
        }
    }
}

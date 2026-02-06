using TMPro;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class KeypressParticlesReward : MilestoneReward
    {
        [Inject] private CodeTyper codeTyper;
        [SerializeField] private TextMeshProUGUI codeText;
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private int particlesPerKey = 3;

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

        private RectTransform _particlesRect;

        private void Awake()
        {
            _particlesRect = particles.GetComponent<RectTransform>();
        }

        private void OnChar(char c, string visibleText)
        {
            if (!Unlocked || c == '\0') return;

            codeText.text = visibleText;
            codeText.ForceMeshUpdate();

            int charIndex = visibleText.Length - 1;
            var textInfo = codeText.textInfo;

            if (charIndex < 0 || charIndex >= textInfo.characterCount)
                return;

            var charInfo = textInfo.characterInfo[charIndex];
            var charCenter = (Vector2)(charInfo.bottomLeft + charInfo.topRight) / 2f;

            // charCenter is in TMP local space, offset by TMP's anchoredPosition to get parent space
            _particlesRect.anchoredPosition = codeText.rectTransform.anchoredPosition + charCenter;
            particles.Emit(particlesPerKey);
        }
    }
}

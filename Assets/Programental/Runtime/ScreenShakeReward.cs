using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class ScreenShakeReward : MilestoneReward
    {
        [Inject] private CodeTyper codeTyper;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float cameraShakeStrength = 0.3f;
        [SerializeField] private float uiShakeStrength = 15f;

        private Tween _cameraTween;
        private Tween _uiTween;

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

            _cameraTween?.Complete();
            _cameraTween = cameraTransform.DOShakePosition(shakeDuration, cameraShakeStrength, 20);

            _uiTween?.Complete();
            _uiTween = canvasRect.DOShakeAnchorPos(shakeDuration, uiShakeStrength, 20);
        }
    }
}

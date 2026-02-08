using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class FloatingLineReward : MilestoneReward
    {
        public override string RewardId => "FloatingLine";

        [Inject] private CodeTyper codeTyper;
        [SerializeField] private TextMeshProUGUI codeText;
        [SerializeField] private float floatUpDistance = 150f;
        [SerializeField] private float floatUpDuration = 0.8f;

        public event Action<TextMeshProUGUI> OnFloatingLineSpawned;

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

        private void OnLine(string completedLine, int totalLines)
        {
            if (!Unlocked) return;

            var go = Instantiate(codeText.gameObject, codeText.transform.parent);
            var rt = go.GetComponent<RectTransform>();
            var tmp = go.GetComponent<TextMeshProUGUI>();

            rt.anchoredPosition = codeText.rectTransform.anchoredPosition;
            rt.localScale = Vector3.one;
            tmp.text = completedLine;
            tmp.richText = false;

            OnFloatingLineSpawned?.Invoke(tmp);

            var targetY = rt.anchoredPosition.y + floatUpDistance;
            rt.DOAnchorPosY(targetY, floatUpDuration).SetEase(Ease.OutQuad);
            tmp.DOFade(0f, floatUpDuration).SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(go));
        }
    }
}

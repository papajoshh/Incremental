using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class ShowLineCounterReward : MilestoneReward
    {
        [Inject] private CodeTyper codeTyper;
        [SerializeField] private TextMeshProUGUI counterText;

        private void Awake()
        {
            counterText.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (codeTyper != null)
                codeTyper.OnLineCompleted += OnLine;
        }

        private void OnDisable()
        {
            if (codeTyper != null)
                codeTyper.OnLineCompleted -= OnLine;
        }

        public override void OnUnlock()
        {
            counterText.gameObject.SetActive(true);
            counterText.transform.localScale = Vector3.zero;
            counterText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            UpdateCounter(codeTyper.LinesCompleted);
        }

        public override void Restore()
        {
            base.Restore();
            counterText.gameObject.SetActive(true);
            UpdateCounter(codeTyper.LinesCompleted);
        }

        private void OnLine(string _, int totalLines)
        {
            if (!Unlocked) return;
            UpdateCounter(totalLines);
            counterText.transform.DOComplete();
            counterText.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0);
        }

        private void UpdateCounter(int totalLines)
        {
            counterText.text = totalLines.ToString();
        }
    }
}

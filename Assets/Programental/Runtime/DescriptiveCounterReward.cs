using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class DescriptiveCounterReward : MilestoneReward
    {
        [Inject] private CodeTyper codeTyper;
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private string localizationKey = "UI/LinesOfCode";
        [SerializeField] private ShowLineCounterReward previousReward;

        public override void OnUnlock()
        {
            if (previousReward != null) previousReward.enabled = false;
            counterText.transform.DOComplete();
            counterText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0);
            UpdateCounter(codeTyper.LinesCompleted);
        }

        public override void Restore()
        {
            base.Restore();
            if (previousReward != null) previousReward.enabled = false;
            UpdateCounter(codeTyper.LinesCompleted);
        }

        private void OnEnable()
        {
            if (codeTyper != null) codeTyper.OnLineCompleted += OnLine;
        }

        private void OnDisable()
        {
            if (codeTyper != null) codeTyper.OnLineCompleted -= OnLine;
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
            var label = LocalizationManager.GetTranslation(localizationKey);
            counterText.text = $"{label} {totalLines}";
        }
    }
}

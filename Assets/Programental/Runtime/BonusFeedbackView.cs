using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Programental
{
    public class BonusFeedbackView : MonoBehaviour, IBonusFeedback
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private Image timerBar;
        [SerializeField] private CanvasGroup canvasGroup;

        private Tween _timerTween;
        private Tween _fadeTween;

        private void Awake()
        {
            canvasGroup.alpha = 0f;
        }

        public void ShowBonus(BonusInfo info)
        {
            _timerTween?.Kill();
            _fadeTween?.Kill();

            var translation = LocalizationManager.GetTranslation(info.LocalizationKey);
            labelText.text = string.Format(translation, info.Value);
            timerBar.fillAmount = 1f;
            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;

            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0);

            _timerTween = timerBar.DOFillAmount(0f, info.Duration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    _fadeTween = canvasGroup.DOFade(0f, 0.3f);
                });
        }
    }
}

using System;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Programental
{
    public class BonusFeedbackItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private Image timerBar;
        [SerializeField] private CanvasGroup canvasGroup;

        private float _totalDuration;
        private float _remainingTime;
        private bool _expired;

        public string BonusId { get; private set; }
        public event Action<BonusFeedbackItem> OnExpired;

        public void Play(BonusInfo info)
        {
            BonusId = info.BonusId;
            _totalDuration = info.Duration;
            _remainingTime = info.Duration;

            var translation = LocalizationManager.GetTranslation(info.LocalizationKey);
            labelText.text = string.Format(translation, info.Value);
            timerBar.fillAmount = 1f;
            canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;

            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0);
        }

        public void Extend(float addedDuration)
        {
            _remainingTime += addedDuration;
            _totalDuration += addedDuration;
            timerBar.fillAmount = _remainingTime / _totalDuration;

            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 6, 0);
        }

        private void Update()
        {
            if (_expired) return;

            _remainingTime -= Time.deltaTime;
            timerBar.fillAmount = Mathf.Clamp01(_remainingTime / _totalDuration);

            if (_remainingTime > 0f) return;
            _expired = true;
            OnExpired?.Invoke(this);
            canvasGroup.DOFade(0f, 0.3f)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}

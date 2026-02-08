using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class GoldenCodeCounterView : MonoBehaviour
    {
        [Inject] private GoldenCodeManager goldenCodeManager;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI availableText;
        [SerializeField] private TextMeshProUGUI completedText;
        [SerializeField] private string availableLocKey = "UI/GoldenCodeAvailable";
        [SerializeField] private string completedLocKey = "UI/GoldenCodeCompleted";

        private bool _visible;
        private bool _showLabels;

        private void Awake()
        {
            canvasGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            goldenCodeManager.OnStatsChanged += OnStatsChanged;
        }

        private void OnDisable()
        {
            goldenCodeManager.OnStatsChanged -= OnStatsChanged;
        }

        public void Show(bool animate = true)
        {
            if (_visible) return;
            _visible = true;
            UpdateTexts();

            if (!animate)
            {
                canvasGroup.alpha = 1f;
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.3f);
            availableText.transform.localScale = Vector3.zero;
            completedText.transform.localScale = Vector3.zero;
            availableText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            completedText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.1f);
        }

        public void ShowLabels(bool animate = true)
        {
            _showLabels = true;
            if (!_visible) return;
            UpdateTexts();

            if (!animate) return;
            availableText.transform.DOComplete();
            completedText.transform.DOComplete();
            availableText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0);
            completedText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0);
        }

        private void OnStatsChanged()
        {
            if (!_visible) return;
            UpdateTexts();
            availableText.transform.DOComplete();
            availableText.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0);
            completedText.transform.DOComplete();
            completedText.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0);
        }

        private void UpdateTexts()
        {
            var available = goldenCodeManager.PoolSize;
            var completed = goldenCodeManager.WordsCompleted;

            if (_showLabels)
            {
                availableText.text = $"{LocalizationManager.GetTranslation(availableLocKey)} {available}";
                completedText.text = $"{LocalizationManager.GetTranslation(completedLocKey)} {completed}";
            }
            else
            {
                availableText.text = available.ToString();
                completedText.text = completed.ToString();
            }
        }
    }
}

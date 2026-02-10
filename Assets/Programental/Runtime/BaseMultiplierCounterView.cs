using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class BaseMultiplierCounterView : MonoBehaviour
    {
        [Inject] private BaseMultiplierTracker baseMultiplierTracker;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI multiplierText;

        private bool _visible;

        private void Awake()
        {
            canvasGroup.alpha = 0f;
        }

        private void Start()
        {
            if (baseMultiplierTracker.CurrentLevel > 0)
                Show(false);
        }

        private void OnEnable()
        {
            baseMultiplierTracker.OnMultiplierChanged += OnMultiplierChanged;
        }

        private void OnDisable()
        {
            baseMultiplierTracker.OnMultiplierChanged -= OnMultiplierChanged;
        }

        public void Show(bool animate = true)
        {
            if (_visible) return;
            _visible = true;
            UpdateText();

            if (!animate)
            {
                canvasGroup.alpha = 1f;
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.3f);
            multiplierText.transform.localScale = Vector3.zero;
            multiplierText.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        private void OnMultiplierChanged()
        {
            if (!_visible)
            {
                Show();
                return;
            }

            UpdateText();
            multiplierText.transform.DOComplete();
            multiplierText.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0);
        }

        private void UpdateText()
        {
            var multiplier = baseMultiplierTracker.CurrentMultiplier;
            multiplierText.text = $"x{multiplier:F2}";
        }
    }
}

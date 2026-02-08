using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class LineCounterView : MonoBehaviour
    {
        [Inject] private LinesTracker linesTracker;
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private GameObject frame;
        [SerializeField] private string localizationKey = "UI/LinesOfCode";

        private bool _visible;
        private bool _showLabel;
        private bool _punchOnIncrement;

        private void Awake()
        {
            counterText.gameObject.SetActive(false);
            frame.SetActive(false);
        }

        private void OnEnable()
        {
            linesTracker.OnAvailableLinesChanged += OnLinesChanged;
        }

        private void OnDisable()
        {
            linesTracker.OnAvailableLinesChanged -= OnLinesChanged;
        }

        public void Show(bool animate = true)
        {
            if (_visible) return;
            _visible = true;
            counterText.gameObject.SetActive(true);
            UpdateText(linesTracker.AvailableLines);
            if (!animate) return;
            counterText.transform.localScale = Vector3.zero;
            counterText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }

        public void ShowLabel(bool enabled)
        {
            _showLabel = enabled;
            if (!_visible) return;
            UpdateText(linesTracker.AvailableLines);
            counterText.transform.DOComplete();
            counterText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0);
        }

        public void SetPunchOnIncrement(bool enabled)
        {
            _punchOnIncrement = enabled;
        }

        public void ShowFrame(bool enabled)
        {
            frame.SetActive(enabled);
        }

        private void OnLinesChanged(int availableLines)
        {
            if (!_visible) return;
            UpdateText(availableLines);
            if (!_punchOnIncrement) return;
            counterText.transform.DOComplete();
            counterText.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0);
        }

        private void UpdateText(int lines)
        {
            if (_showLabel)
            {
                var label = LocalizationManager.GetTranslation(localizationKey);
                counterText.text = $"{label} {lines}";
            }
            else
            {
                counterText.text = lines.ToString();
            }
        }
    }
}

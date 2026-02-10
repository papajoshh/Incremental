using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Programental
{
    public class GoldenCodeWord : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI wordText;
        [SerializeField] private Color defaultColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color correctColor = new Color(0.2f, 1f, 0.2f);
        [SerializeField] private Color wrongColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private float moveSpeed = 30f;

        private string _word;
        private int _progress;
        private float _timer;
        private RectTransform _bounds;
        private RectTransform _rt;
        private Vector2 _direction;

        public event Action<GoldenCodeWord> OnCompleted;
        public event Action<GoldenCodeWord> OnExpired;

        public void Init(string word, float lifetime, RectTransform bounds)
        {
            _word = word;
            _timer = lifetime;
            _bounds = bounds;
            _rt = GetComponent<RectTransform>();

            var rect = _bounds.rect;
            _rt.anchoredPosition = new Vector2(
                UnityEngine.Random.Range(rect.xMin + 50, rect.xMax - 50),
                UnityEngine.Random.Range(rect.yMin + 20, rect.yMax - 20));

            _direction = UnityEngine.Random.insideUnitCircle.normalized;

            UpdateDisplay();
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                OnExpired?.Invoke(this);
                return;
            }

            _rt.anchoredPosition += _direction * (moveSpeed * Time.deltaTime);

            var rect = _bounds.rect;
            var pos = _rt.anchoredPosition;
            if (pos.x < rect.xMin || pos.x > rect.xMax) _direction.x = -_direction.x;
            if (pos.y < rect.yMin || pos.y > rect.yMax) _direction.y = -_direction.y;
        }

        public void CheckChar(char c)
        {
            if (_progress >= _word.Length) return;

            if (char.ToLower(c) == char.ToLower(_word[_progress]))
            {
                _progress++;
                UpdateDisplay();
                wordText.transform.DOComplete();
                wordText.transform.DOPunchScale(Vector3.one * 0.1f, 0.15f, 5, 0);

                if (_progress >= _word.Length)
                    OnCompleted?.Invoke(this);
                return;
            }

            wordText.transform.DOComplete();
            wordText.DOColor(wrongColor, 0.1f)
                .OnComplete(() => UpdateDisplay());
            wordText.transform.DOShakePosition(0.2f, 5f, 20);
        }

        private void UpdateDisplay()
        {
            wordText.color = defaultColor;

            if (_progress <= 0)
            {
                wordText.richText = false;
                wordText.text = _word;
                return;
            }

            wordText.richText = true;
            var typed = _word[.._progress];
            var remaining = _word[_progress..];
            var hex = ColorUtility.ToHtmlStringRGB(correctColor);
            wordText.text = $"<color=#{hex}>{typed}</color>{remaining}";
        }
    }
}

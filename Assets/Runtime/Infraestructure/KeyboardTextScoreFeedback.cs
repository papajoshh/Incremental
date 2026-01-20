using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class KeyboardTextScoreFeedback: MonoBehaviour
    {
        [Inject] private readonly Keyboard _controller;
        [SerializeField] private TextMeshProUGUI _textFeedback;
        [SerializeField] private RectTransform _rectFeedback;
        [SerializeField] private float upMove;
        [SerializeField] private float horizonalRange;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Game Feel")]
        [SerializeField] private float rotationRange = 15f;
        [SerializeField] private float baseScale = 1f;
        [SerializeField] private float scalePerPower = 0.05f;

        private void Awake()
        {
            _textFeedback.text = $"+ {_controller.AddPerPress}";
            ApplyRandomRotation();
            ApplyScaleByValue();
        }

        private void Start()
        {
            _rectFeedback.anchoredPosition += new Vector2(Random.Range(-horizonalRange, horizonalRange), 0);
            _rectFeedback.DOAnchorPos(new Vector2(_rectFeedback.anchoredPosition.x, _rectFeedback.anchoredPosition.y + upMove), 0.75f).OnComplete(FadeOut);
            canvasGroup.DOFade(1, 0.3f);

            // Punch scale al aparecer
            _rectFeedback.DOPunchScale(Vector3.one * 0.3f, 0.2f, 5);
        }

        private void ApplyRandomRotation()
        {
            _rectFeedback.localRotation = Quaternion.Euler(0, 0, Random.Range(-rotationRange, rotationRange));
        }

        private void ApplyScaleByValue()
        {
            float scale = baseScale + (_controller.AddPerPress - 1) * scalePerPower;
            _rectFeedback.localScale = Vector3.one * scale;
        }

        private void FadeOut()
        {
            canvasGroup.DOFade(0, 1f).SetDelay(1).OnComplete(OnEnd);
        }

        private void OnEnd()
        {
            Destroy(gameObject);
        }

        public class KeyboardScoreFeedbackFactory : PlaceholderFactory<KeyboardTextScoreFeedback> { }
    }
}
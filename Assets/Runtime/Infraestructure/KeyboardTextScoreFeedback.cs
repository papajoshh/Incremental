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

        private void Awake()
        {
            _textFeedback.text = $"+ {_controller.AddPerPress}";
        }

        private void Start()
        {
            _rectFeedback.anchoredPosition += new Vector2(Random.Range(-horizonalRange, horizonalRange), 0);
            _rectFeedback.DOAnchorPos(new Vector2(_rectFeedback.anchoredPosition.x, _rectFeedback.anchoredPosition.y + upMove), 0.75f).OnComplete(FadeOut);
            canvasGroup.DOFade(1, 0.3f);
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
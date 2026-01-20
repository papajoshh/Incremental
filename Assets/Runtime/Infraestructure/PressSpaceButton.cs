using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Runtime.Infraestructure
{
    public class PressSpaceButton: MonoBehaviour
    {
        [Inject] private readonly Keyboard _controller;
        [Inject] private readonly KeyboardTextScoreFeedback.KeyboardScoreFeedbackFactory _factory;
        [Inject] private readonly ContainerShaker _shaker;

        [SerializeField] private GameObject _textFeedback;
        [SerializeField] private RectTransform referenceSpawnPosition;
        [SerializeField] private Button _button;
        [SerializeField] private Image _buttonImage;

        [Header("Game Feel")]
        [SerializeField] private float hitstopDuration = 0.05f;
        [SerializeField] private Vector3 squashPunch = new Vector3(0.1f, -0.2f, 0);
        [SerializeField] private float squashDuration = 0.2f;

        private Color _originalButtonColor;

        private void Awake()
        {
            _button.onClick.AddListener(Pressed);
            if (_buttonImage != null)
                _originalButtonColor = _buttonImage.color;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space)) Pressed();
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(Pressed);
        }

        private void Pressed()
        {
            _controller.Press();
            var feedback = _factory.Create();
            feedback.transform.SetParent(referenceSpawnPosition, false);
            _shaker.Shake();

            ApplyHitstop();
            ApplySquashStretch();
            ApplyFlash();
        }

        private void ApplyHitstop()
        {
            Time.timeScale = 0f;
            DOVirtual.DelayedCall(hitstopDuration, () => Time.timeScale = 1f).SetUpdate(true);
        }

        private void ApplySquashStretch()
        {
            _button.transform.DOComplete();
            _button.transform.DOPunchScale(squashPunch, squashDuration, 5);
        }

        private void ApplyFlash()
        {
            if (_buttonImage == null) return;

            _buttonImage.DOComplete();
            _buttonImage.DOColor(Color.white, 0.05f)
                .OnComplete(() => _buttonImage.DOColor(_originalButtonColor, 0.1f));
        }
    }
}
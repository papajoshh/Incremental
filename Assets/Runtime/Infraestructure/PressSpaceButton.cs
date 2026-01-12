using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Runtime.Infraestructure
{
    public class PressSpaceButton: MonoBehaviour
    {
        [Inject] private readonly Keyboard _controller;
        [Inject] private readonly KeyboardTextScoreFeedback.KeyboardScoreFeedbackFactory _factory;

        [SerializeField] private GameObject _textFeedback;
        [SerializeField] private RectTransform referenceSpawnPosition;
        [SerializeField] private Button _button;

        private void Awake()
        {
            _button.onClick.AddListener(Pressed);
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
        }
    }
}
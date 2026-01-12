using TMPro;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class CounterTextMeshPro: MonoBehaviour
    {
        [Inject] private readonly Keyboard _controller;
        [SerializeField] private TextMeshProUGUI _text;

        private void Awake()
        {
            _controller.OnSpacePress += ShowCounter;
            if (_controller.SpacePresses <= 0) _text.text = "";
        }

        private void OnDestroy()
        {
            _controller.OnSpacePress -= ShowCounter;
        }
        
		private void ShowCounter(int counter)
        {
            _text.text = counter.ToString();
        }
    }
}
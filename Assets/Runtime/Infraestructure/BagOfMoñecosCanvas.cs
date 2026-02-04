using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class BagOfMoñecosCanvas: MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI counterText;
        [SerializeField] private CanvasGroup canvasGroup;
        [Inject] private readonly BagOfMoñecos _bagOfMoñecos;

        private void Start()
        {
            _bagOfMoñecos.OnMoñecosInsideChange += UpdateCounterText;
            if (_bagOfMoñecos.MoñecosInside <= 0)
            {
                canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InQuad);
                return;
            }
            UpdateCounterText(_bagOfMoñecos.MoñecosInside);
        }

        private void OnDestroy()
        {
            _bagOfMoñecos.OnMoñecosInsideChange -= UpdateCounterText;
        }
        
        public void Enable()
        {
            Show();
        }

        private void UpdateCounterText(int moñecosInside)
        {
            counterText.text = moñecosInside.ToString();
        }
        
        private void Show()
        {
            if(canvasGroup.alpha >= 1f) return;
            canvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InQuad);
        }
    }
}
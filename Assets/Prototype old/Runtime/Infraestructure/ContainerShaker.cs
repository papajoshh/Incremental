using DG.Tweening;
using UnityEngine;

namespace Runtime.Infraestructure
{
    public class ContainerShaker: MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Transform cameraContainer;
        [SerializeField] private float shakeDuration = 0.15f;
        [SerializeField] private float uiShakeStrength = 15f;
        [SerializeField] private float cameraShakeStrength = 0.2f;

        private Tween shake;
        private Tween cameraShake;

        public void Shake()
        {
            Shake(cameraShakeStrength);
        }

        public void Shake(float strength)
        {
            shake?.Complete();
            shake = rectTransform.DOShakeAnchorPos(shakeDuration, uiShakeStrength, 20);

            cameraShake?.Complete();
            cameraShake = cameraContainer.DOShakePosition(shakeDuration, strength, 20);
        }

        public void SlowMotion(float duration, float timeScale = 0.1f)
        {
            Time.timeScale = timeScale;
            DOTween.Sequence()
                .AppendInterval(duration)
                .AppendCallback(() => Time.timeScale = 1f)
                .SetUpdate(true);
        }
    }
}
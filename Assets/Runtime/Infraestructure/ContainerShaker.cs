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
            shake?.Complete();
            shake = rectTransform.DOShakeAnchorPos(shakeDuration, uiShakeStrength, 20);

            cameraShake?.Complete();
            cameraShake = cameraContainer.DOShakePosition(shakeDuration, cameraShakeStrength, 20);
        }

    }
}
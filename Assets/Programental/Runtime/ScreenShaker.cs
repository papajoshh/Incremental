using DG.Tweening;
using UnityEngine;

namespace Programental
{
    public class ScreenShaker : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private RectTransform canvasRect;

        private Tween _cameraTween;
        private Tween _canvasTween;

        public void Shake(float duration, float cameraStrength, float uiStrength, int vibrato = 20)
        {
            _cameraTween?.Complete();
            _cameraTween = cameraTransform.DOShakePosition(duration, cameraStrength, vibrato);

            _canvasTween?.Complete();
            _canvasTween = canvasRect.DOShakeAnchorPos(duration, uiStrength, vibrato);
        }
    }
}

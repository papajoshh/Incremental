using DG.Tweening;
using UnityEngine;

namespace TypingDefense
{
    public class CameraShaker : MonoBehaviour
    {
        [SerializeField] Transform cameraTransform;
        [SerializeField] RectTransform uiContainer;

        Vector3 cameraOriginalPos;

        void Awake()
        {
            cameraOriginalPos = cameraTransform.localPosition;
        }

        public void Shake(float intensity, float duration, int vibrato = 14)
        {
            cameraTransform.DOComplete();
            cameraTransform.DOShakePosition(duration, intensity, vibrato, 90f, false, true, ShakeRandomnessMode.Harmonic)
                .OnComplete(() => cameraTransform.localPosition = cameraOriginalPos);

            if (uiContainer == null) return;

            uiContainer.DOComplete();
            uiContainer.DOShakeAnchorPos(duration, intensity * 50f, vibrato, 90f, false, true, ShakeRandomnessMode.Harmonic);
        }
    }
}

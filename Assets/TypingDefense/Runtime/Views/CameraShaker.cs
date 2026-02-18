using DG.Tweening;
using UnityEngine;

namespace TypingDefense
{
    public class CameraShaker : MonoBehaviour
    {
        [SerializeField] Transform cameraTransform;
        [SerializeField] RectTransform uiContainer;

        Vector3 _cameraOriginalPos;
        Vector3 _rigOriginalPos;
        Transform _rigTransform;
        Camera _camera;
        float _baseOrthographicSize;

        void Awake()
        {
            _cameraOriginalPos = cameraTransform.localPosition;
            _rigTransform = cameraTransform.parent;
            _rigOriginalPos = _rigTransform.position;
            _camera = cameraTransform.GetComponent<Camera>();
            _baseOrthographicSize = _camera.orthographicSize;
        }

        public void Shake(float intensity, float duration, int vibrato = 14)
        {
            cameraTransform.DOComplete();
            cameraTransform.DOShakePosition(duration, intensity, vibrato, 90f, false, true, ShakeRandomnessMode.Harmonic)
                .SetUpdate(true);

            if (uiContainer == null) return;

            uiContainer.DOComplete();
            uiContainer.DOShakeAnchorPos(duration, intensity * 50f, vibrato, 90f, false, true, ShakeRandomnessMode.Harmonic)
                .SetUpdate(true);
        }

        public Sequence ZoomCharge(float chargeDuration, float zoomFraction, float chargeShakeIntensity,
            float releaseShakeIntensity, float releaseShakeDuration, Vector3 targetWorldPosition)
        {
            DOTween.Kill(this);

            var targetSize = _baseOrthographicSize * (1f - zoomFraction);
            var buildupDuration = chargeDuration * 0.85f;
            var holdDuration = chargeDuration * 0.15f;
            var panTarget = new Vector3(targetWorldPosition.x, targetWorldPosition.y, _rigOriginalPos.z);

            var seq = DOTween.Sequence().SetUpdate(true).SetTarget(this);

            // Phase 1: zoom in + pan rig towards BH with escalating shake
            // Pan moves the PARENT (transform) so Shake on cameraTransform doesn't interfere
            seq.Append(
                DOTween.To(() => _camera.orthographicSize, v => _camera.orthographicSize = v,
                    targetSize, buildupDuration)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
            );
            seq.Join(
                _rigTransform.DOMove(panTarget, buildupDuration)
                    .SetEase(Ease.InOutQuad)
                    .SetUpdate(true)
            );

            var shakeSteps = 6;
            var stepDuration = buildupDuration / shakeSteps;
            for (var i = 0; i < shakeSteps; i++)
            {
                var t = (float)(i + 1) / shakeSteps;
                var intensity = Mathf.Lerp(chargeShakeIntensity * 0.2f, chargeShakeIntensity, t * t);
                var capturedIntensity = intensity;
                seq.InsertCallback(stepDuration * i, () =>
                    Shake(capturedIntensity, stepDuration * 1.2f, 8));
            }

            // Phase 2: dramatic hold at max zoom
            seq.AppendInterval(holdDuration);

            // Phase 3: RELEASE — snap back with overshoot + explosive shake
            seq.AppendCallback(() =>
                Shake(releaseShakeIntensity, releaseShakeDuration, 20));
            seq.Append(
                DOTween.To(() => _camera.orthographicSize, v => _camera.orthographicSize = v,
                    _baseOrthographicSize, 0.35f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
            );
            seq.Join(
                _rigTransform.DOMove(_rigOriginalPos, 0.35f)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
            );

            return seq;
        }

        public void ResetZoom()
        {
            DOTween.Kill(this);
            _camera.orthographicSize = _baseOrthographicSize;
            cameraTransform.localPosition = _cameraOriginalPos;
            _rigTransform.position = _rigOriginalPos;
        }
    }
}

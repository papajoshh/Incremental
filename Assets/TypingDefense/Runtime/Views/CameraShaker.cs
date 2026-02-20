using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class CameraShaker : MonoBehaviour
    {
        [SerializeField] Transform cameraTransform;
        [SerializeField] RectTransform uiContainer;
        [SerializeField] float followSmoothTime = 0.15f;

        Vector3 _cameraOriginalPos;
        Vector3 _rigOriginalPos;
        Transform _rigTransform;
        Camera _camera;
        float _baseOrthographicSize;

        ArenaView _arenaView;
        BlackHoleController _blackHole;
        GameFlowController _gameFlow;

        Vector3 _followVelocity;
        bool _isCharging;

        [Inject]
        public void Construct(ArenaView arenaView, BlackHoleController blackHole, GameFlowController gameFlow)
        {
            _arenaView = arenaView;
            _blackHole = blackHole;
            _gameFlow = gameFlow;
        }

        void Awake()
        {
            _cameraOriginalPos = cameraTransform.localPosition;
            _rigTransform = cameraTransform.parent;
            _rigOriginalPos = _rigTransform.position;
            _camera = cameraTransform.GetComponent<Camera>();
            _baseOrthographicSize = _camera.orthographicSize;
        }

        void LateUpdate()
        {
            if (_isCharging) return;

            var state = _gameFlow.State;
            if (state != GameState.Playing && state != GameState.Collecting) return;

            var targetPos = _blackHole.Position;
            targetPos.z = _rigOriginalPos.z;

            var cameraBounds = _arenaView.GetCameraBounds();
            targetPos.x = Mathf.Clamp(targetPos.x, cameraBounds.xMin, cameraBounds.xMax);
            targetPos.y = Mathf.Clamp(targetPos.y, cameraBounds.yMin, cameraBounds.yMax);

            _rigTransform.position = Vector3.SmoothDamp(
                _rigTransform.position, targetPos, ref _followVelocity,
                followSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
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
            _isCharging = true;

            var targetSize = _baseOrthographicSize * (1f - zoomFraction);
            var buildupDuration = chargeDuration * 0.85f;
            var holdDuration = chargeDuration * 0.15f;
            var panTarget = new Vector3(targetWorldPosition.x, targetWorldPosition.y, _rigOriginalPos.z);

            var seq = DOTween.Sequence().SetUpdate(true).SetTarget(this);

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

            seq.AppendInterval(holdDuration);

            seq.AppendCallback(() =>
            {
                _isCharging = false;
                Shake(releaseShakeIntensity, releaseShakeDuration, 20);
            });
            seq.Append(
                DOTween.To(() => _camera.orthographicSize, v => _camera.orthographicSize = v,
                    _baseOrthographicSize, 0.35f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
            );

            return seq;
        }

        public void ZoomPunch(float zoomAmount, float duration)
        {
            var targetSize = _baseOrthographicSize * (1f - zoomAmount);
            DOTween.To(() => _camera.orthographicSize, v => _camera.orthographicSize = v,
                targetSize, duration * 0.3f).SetEase(Ease.OutQuad).SetUpdate(true)
                .OnComplete(() =>
                    DOTween.To(() => _camera.orthographicSize, v => _camera.orthographicSize = v,
                        _baseOrthographicSize, duration * 0.7f).SetEase(Ease.OutBack).SetUpdate(true));
        }

        public void ResetZoom()
        {
            DOTween.Kill(this);
            _isCharging = false;
            _camera.orthographicSize = _baseOrthographicSize;
            cameraTransform.localPosition = _cameraOriginalPos;
            _followVelocity = Vector3.zero;
        }
    }
}

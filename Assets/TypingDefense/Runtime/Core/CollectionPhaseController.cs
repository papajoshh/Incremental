using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class CollectionPhaseController : ITickable
    {
        readonly CollectionPhaseConfig _config;
        readonly GameFlowController _gameFlow;
        readonly RunManager _runManager;
        readonly CameraShaker _cameraShaker;

        float _timer;
        bool _frozen;
        Tween _chargeTween;

        public float TimeRemaining => _timer;
        public float TimerRatio => _config.collectionDuration > 0f ? _timer / _config.collectionDuration : 0f;
        public bool IsActive => _gameFlow.State == GameState.Collecting;

        public event Action<float> OnTimerChanged;
        public event Action OnCollectionStarted;
        public event Action OnCollectionEnded;
        public event Action OnFreezeReleased;
        public event Action<float> OnChargeStarted;

        public CollectionPhaseController(
            CollectionPhaseConfig config,
            GameFlowController gameFlow,
            RunManager runManager,
            CameraShaker cameraShaker)
        {
            _config = config;
            _gameFlow = gameFlow;
            _runManager = runManager;
            _cameraShaker = cameraShaker;
        }

        public void Tick()
        {
            if (!IsActive) return;
            if (_frozen) return;

            _timer -= Time.unscaledDeltaTime;
            OnTimerChanged?.Invoke(_timer);

            if (_timer > 0f) return;

            EndCollection();
        }

        public void StartCollection()
        {
            _timer = _config.collectionDuration;
            _frozen = true;

            Time.timeScale = 0f;

            OnCollectionStarted?.Invoke();
            OnChargeStarted?.Invoke(_config.chargeDuration);

            _chargeTween = _cameraShaker.ZoomCharge(
                _config.chargeDuration,
                _config.zoomAmount,
                _config.chargeShakeIntensity,
                _config.releaseShakeIntensity,
                _config.releaseShakeDuration);

            _chargeTween.OnComplete(() =>
            {
                _frozen = false;
                Time.timeScale = _config.slowMotionScale;
                OnFreezeReleased?.Invoke();
            });
        }

        public void HandleWordHitBlackHole()
        {
            _runManager.TakeDamage(1);
            _cameraShaker.Shake(0.2f, 0.15f);
        }

        public void ForceEnd()
        {
            _chargeTween?.Kill();
            _chargeTween = null;
            _cameraShaker.ResetZoom();
            RestoreTimeScale();
        }

        void EndCollection()
        {
            RestoreTimeScale();
            OnCollectionEnded?.Invoke();
            _gameFlow.HandleCollectionEnded();
        }

        void RestoreTimeScale()
        {
            _frozen = false;
            Time.timeScale = 1f;
        }
    }
}

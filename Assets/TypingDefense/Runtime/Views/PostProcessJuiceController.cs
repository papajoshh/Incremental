using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Zenject;

namespace TypingDefense
{
    public class PostProcessJuiceController : MonoBehaviour
    {
        [SerializeField] PostProcessVolume volume;

        ChromaticAberration _chromatic;
        Vignette _vignette;
        ColorGrading _colorGrading;
        LensDistortion _lens;

        float _baseVignetteIntensity;
        float _baseSaturation;
        float _baseLensIntensity;
        float _baseTemperature;
        float _baseTint;
        bool _gameOverFlashing;

        GameFlowController _gameFlow;
        CollectionPhaseController _collectionPhase;
        RunManager _runManager;

        [Inject]
        public void Construct(
            GameFlowController gameFlow,
            CollectionPhaseController collectionPhase,
            RunManager runManager)
        {
            _gameFlow = gameFlow;
            _collectionPhase = collectionPhase;
            _runManager = runManager;

            gameFlow.OnStateChanged += OnStateChanged;
            collectionPhase.OnChargeStarted += OnChargeStarted;
            collectionPhase.OnFreezeReleased += OnFreezeReleased;
            runManager.OnRunEnded += OnGameOver;
        }

        void Start()
        {
            var profile = volume.profile;

            if (!profile.TryGetSettings(out _chromatic))
            {
                _chromatic = profile.AddSettings<ChromaticAberration>();
                _chromatic.enabled.value = true;
                _chromatic.intensity.overrideState = true;
            }
            _chromatic.intensity.value = 0f;

            profile.TryGetSettings(out _vignette);
            profile.TryGetSettings(out _colorGrading);
            profile.TryGetSettings(out _lens);

            _baseVignetteIntensity = _vignette.intensity.value;
            _baseSaturation = _colorGrading.saturation.value;
            _baseLensIntensity = _lens.intensity.value;
            _baseTemperature = _colorGrading.temperature.value;
            _baseTint = _colorGrading.tint.value;
        }

        void OnDestroy()
        {
            _gameFlow.OnStateChanged -= OnStateChanged;
            _collectionPhase.OnChargeStarted -= OnChargeStarted;
            _collectionPhase.OnFreezeReleased -= OnFreezeReleased;
            _runManager.OnRunEnded -= OnGameOver;

            DOTween.Kill(this);
            RestoreImmediate();
        }

        void OnStateChanged(GameState state)
        {
            if (_gameOverFlashing) return;

            if (state == GameState.Menu || state == GameState.Playing)
                RestoreDefaults();
        }

        void OnChargeStarted(float chargeDuration)
        {
            DOTween.Kill(this);

            _colorGrading.temperature.overrideState = true;
            _colorGrading.tint.overrideState = true;

            DOTween.To(
                () => _chromatic.intensity.value,
                v => _chromatic.intensity.value = v,
                0.5f, chargeDuration).SetUpdate(true).SetEase(Ease.InQuad).SetTarget(this);

            DOTween.To(
                () => _vignette.intensity.value,
                v => _vignette.intensity.value = v,
                0.65f, chargeDuration).SetUpdate(true).SetEase(Ease.InQuad).SetTarget(this);

            DOTween.To(
                () => _lens.intensity.value,
                v => _lens.intensity.value = v,
                25f, chargeDuration).SetUpdate(true).SetEase(Ease.InCubic).SetTarget(this);

            DOTween.To(
                () => _colorGrading.saturation.value,
                v => _colorGrading.saturation.value = v,
                -30f, chargeDuration).SetUpdate(true).SetEase(Ease.InQuad).SetTarget(this);

            DOTween.To(
                () => _colorGrading.temperature.value,
                v => _colorGrading.temperature.value = v,
                -15f, chargeDuration).SetUpdate(true).SetEase(Ease.InQuad).SetTarget(this);

            DOTween.To(
                () => _colorGrading.tint.value,
                v => _colorGrading.tint.value = v,
                10f, chargeDuration).SetUpdate(true).SetEase(Ease.InQuad).SetTarget(this);
        }

        void OnFreezeReleased()
        {
            DOTween.Kill(this);

            DOTween.To(
                () => _chromatic.intensity.value,
                v => _chromatic.intensity.value = v,
                1.0f, 0.3f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);

            DOTween.To(
                () => _vignette.intensity.value,
                v => _vignette.intensity.value = v,
                0.52f, 0.4f).SetUpdate(true).SetTarget(this);

            DOTween.To(
                () => _colorGrading.saturation.value,
                v => _colorGrading.saturation.value = v,
                -50f, 0.5f).SetUpdate(true).SetTarget(this);

            DOTween.To(
                () => _lens.intensity.value,
                v => _lens.intensity.value = v,
                60f, 0.4f).SetUpdate(true).SetEase(Ease.OutBack).SetTarget(this);

            DOTween.To(
                () => _colorGrading.temperature.value,
                v => _colorGrading.temperature.value = v,
                -30f, 0.4f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);

            DOTween.To(
                () => _colorGrading.tint.value,
                v => _colorGrading.tint.value = v,
                20f, 0.4f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);
        }

        void OnGameOver()
        {
            DOTween.Kill(this);
            _gameOverFlashing = true;

            _chromatic.intensity.value = 1f;
            DOTween.To(
                () => _chromatic.intensity.value,
                v => _chromatic.intensity.value = v,
                0f, 0.8f).SetUpdate(true).SetTarget(this);

            _vignette.intensity.value = 0.8f;
            DOTween.To(
                () => _vignette.intensity.value,
                v => _vignette.intensity.value = v,
                _baseVignetteIntensity, 0.6f).SetUpdate(true).SetTarget(this)
                .OnComplete(() =>
                {
                    _gameOverFlashing = false;
                    RestoreDefaults();
                });
        }

        void RestoreDefaults()
        {
            DOTween.Kill(this);

            DOTween.To(
                () => _chromatic.intensity.value,
                v => _chromatic.intensity.value = v,
                0f, 0.4f).SetUpdate(true).SetTarget(this);

            DOTween.To(
                () => _vignette.intensity.value,
                v => _vignette.intensity.value = v,
                _baseVignetteIntensity, 0.4f).SetUpdate(true).SetTarget(this);

            DOTween.To(
                () => _colorGrading.saturation.value,
                v => _colorGrading.saturation.value = v,
                _baseSaturation, 0.4f).SetUpdate(true).SetTarget(this);

            DOTween.To(
                () => _lens.intensity.value,
                v => _lens.intensity.value = v,
                _baseLensIntensity, 0.3f).SetUpdate(true).SetTarget(this);

            DOTween.To(
                () => _colorGrading.temperature.value,
                v => _colorGrading.temperature.value = v,
                _baseTemperature, 0.4f).SetUpdate(true).SetTarget(this);

            DOTween.To(
                () => _colorGrading.tint.value,
                v => _colorGrading.tint.value = v,
                _baseTint, 0.4f).SetUpdate(true).SetTarget(this);
        }

        void RestoreImmediate()
        {
            _chromatic.intensity.value = 0f;
            _vignette.intensity.value = _baseVignetteIntensity;
            _colorGrading.saturation.value = _baseSaturation;
            _lens.intensity.value = _baseLensIntensity;
            _colorGrading.temperature.value = _baseTemperature;
            _colorGrading.tint.value = _baseTint;
        }
    }
}

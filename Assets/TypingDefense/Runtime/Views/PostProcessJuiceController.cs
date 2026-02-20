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
        Bloom _bloom;

        float _baseVignetteIntensity;
        float _baseSaturation;
        float _baseLensIntensity;
        float _baseTemperature;
        float _baseTint;
        float _baseBloomIntensity;
        bool _gameOverFlashing;

        GameFlowController _gameFlow;
        CollectionPhaseController _collectionPhase;
        RunManager _runManager;
        WordManager _wordManager;
        WallTracker _wallTracker;

        [Inject]
        public void Construct(
            GameFlowController gameFlow,
            CollectionPhaseController collectionPhase,
            RunManager runManager,
            WordManager wordManager,
            WallTracker wallTracker)
        {
            _gameFlow = gameFlow;
            _collectionPhase = collectionPhase;
            _runManager = runManager;
            _wordManager = wordManager;
            _wallTracker = wallTracker;

            gameFlow.OnStateChanged += OnStateChanged;
            collectionPhase.OnChargeStarted += OnChargeStarted;
            collectionPhase.OnFreezeReleased += OnFreezeReleased;
            runManager.OnRunEnded += OnGameOver;
            wordManager.OnBossSpawned += OnBossSpawned;
            wordManager.OnBossHit += OnBossHit;
            wordManager.OnWordCompleted += OnWordKilled;
            wordManager.OnWordCriticalKill += OnCriticalKill;
            wallTracker.OnSegmentBroken += OnWallSegmentBroken;
            wallTracker.OnRingCompleted += OnWallRingCompleted;
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

            if (!profile.TryGetSettings(out _bloom))
            {
                _bloom = profile.AddSettings<Bloom>();
                _bloom.enabled.value = true;
                _bloom.intensity.overrideState = true;
            }

            _baseVignetteIntensity = _vignette.intensity.value;
            _baseSaturation = _colorGrading.saturation.value;
            _baseLensIntensity = _lens.intensity.value;
            _baseTemperature = _colorGrading.temperature.value;
            _baseTint = _colorGrading.tint.value;
            _baseBloomIntensity = _bloom.intensity.value;
        }

        void OnDestroy()
        {
            _gameFlow.OnStateChanged -= OnStateChanged;
            _collectionPhase.OnChargeStarted -= OnChargeStarted;
            _collectionPhase.OnFreezeReleased -= OnFreezeReleased;
            _runManager.OnRunEnded -= OnGameOver;
            _wordManager.OnBossSpawned -= OnBossSpawned;
            _wordManager.OnBossHit -= OnBossHit;
            _wordManager.OnWordCompleted -= OnWordKilled;
            _wordManager.OnWordCriticalKill -= OnCriticalKill;
            _wallTracker.OnSegmentBroken -= OnWallSegmentBroken;
            _wallTracker.OnRingCompleted -= OnWallRingCompleted;

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

        void OnBossSpawned(DefenseWord word)
        {
            DOTween.Kill(this);

            _chromatic.intensity.value = 0.8f;
            DOTween.To(
                () => _chromatic.intensity.value,
                v => _chromatic.intensity.value = v,
                0f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);

            _bloom.intensity.value = _baseBloomIntensity + 4f;
            DOTween.To(
                () => _bloom.intensity.value,
                v => _bloom.intensity.value = v,
                _baseBloomIntensity, 0.6f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);
        }

        void OnWordKilled(DefenseWord word)
        {
            _bloom.intensity.value = _baseBloomIntensity + 1f;
            DOTween.To(
                () => _bloom.intensity.value,
                v => _bloom.intensity.value = v,
                _baseBloomIntensity, 0.2f).SetEase(Ease.OutQuad).SetTarget(this);
        }

        void OnCriticalKill(DefenseWord word)
        {
            _bloom.intensity.value = _baseBloomIntensity + 2.5f;
            DOTween.To(
                () => _bloom.intensity.value,
                v => _bloom.intensity.value = v,
                _baseBloomIntensity, 0.3f).SetEase(Ease.OutQuad).SetTarget(this);

            _chromatic.intensity.value = 0.35f;
            DOTween.To(
                () => _chromatic.intensity.value,
                v => _chromatic.intensity.value = v,
                0f, 0.2f).SetEase(Ease.OutQuad).SetTarget(this);
        }

        void OnBossHit(DefenseWord word)
        {
            _vignette.intensity.value = 0.55f;
            DOTween.To(
                () => _vignette.intensity.value,
                v => _vignette.intensity.value = v,
                _baseVignetteIntensity, 0.3f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);
        }

        void OnWallSegmentBroken(WallSegmentId id)
        {
            _bloom.intensity.value = _baseBloomIntensity + 1.5f;
            DOTween.To(
                () => _bloom.intensity.value,
                v => _bloom.intensity.value = v,
                _baseBloomIntensity, 0.25f).SetEase(Ease.OutQuad).SetTarget(this);

            _chromatic.intensity.value = 0.25f;
            DOTween.To(
                () => _chromatic.intensity.value,
                v => _chromatic.intensity.value = v,
                0f, 0.2f).SetEase(Ease.OutQuad).SetTarget(this);
        }

        void OnWallRingCompleted(int ring)
        {
            DOTween.Kill(this);

            _bloom.intensity.value = _baseBloomIntensity + 5f;
            DOTween.To(
                () => _bloom.intensity.value,
                v => _bloom.intensity.value = v,
                _baseBloomIntensity, 0.8f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);

            _chromatic.intensity.value = 0.7f;
            DOTween.To(
                () => _chromatic.intensity.value,
                v => _chromatic.intensity.value = v,
                0f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);

            _colorGrading.saturation.value = 30f;
            DOTween.To(
                () => _colorGrading.saturation.value,
                v => _colorGrading.saturation.value = v,
                _baseSaturation, 1f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);
        }

        public void PlayBossDeathBurst()
        {
            DOTween.Kill(this);

            _bloom.intensity.value = 8f;
            DOTween.To(
                () => _bloom.intensity.value,
                v => _bloom.intensity.value = v,
                _baseBloomIntensity + 2f, 1.2f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);

            _chromatic.intensity.value = 1f;
            DOTween.To(
                () => _chromatic.intensity.value,
                v => _chromatic.intensity.value = v,
                0.2f, 1.5f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);

            _vignette.intensity.value = 0.7f;
            DOTween.To(
                () => _vignette.intensity.value,
                v => _vignette.intensity.value = v,
                _baseVignetteIntensity, 1.5f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);

            _colorGrading.saturation.value = 40f;
            DOTween.To(
                () => _colorGrading.saturation.value,
                v => _colorGrading.saturation.value = v,
                _baseSaturation, 1.5f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);
        }

        public void PlayAbsorptionPulse()
        {
            _bloom.intensity.value = 5f;
            DOTween.To(
                () => _bloom.intensity.value,
                v => _bloom.intensity.value = v,
                _baseBloomIntensity, 0.8f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);

            _chromatic.intensity.value = 0.8f;
            DOTween.To(
                () => _chromatic.intensity.value,
                v => _chromatic.intensity.value = v,
                0f, 0.6f).SetUpdate(true).SetEase(Ease.OutQuad).SetTarget(this);

            _lens.intensity.value = 40f;
            DOTween.To(
                () => _lens.intensity.value,
                v => _lens.intensity.value = v,
                _baseLensIntensity, 0.5f).SetUpdate(true).SetEase(Ease.OutBack).SetTarget(this);
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

            DOTween.To(
                () => _bloom.intensity.value,
                v => _bloom.intensity.value = v,
                _baseBloomIntensity, 0.4f).SetUpdate(true).SetTarget(this);
        }

        void RestoreImmediate()
        {
            _chromatic.intensity.value = 0f;
            _vignette.intensity.value = _baseVignetteIntensity;
            _colorGrading.saturation.value = _baseSaturation;
            _lens.intensity.value = _baseLensIntensity;
            _colorGrading.temperature.value = _baseTemperature;
            _colorGrading.tint.value = _baseTint;
            _bloom.intensity.value = _baseBloomIntensity;
        }
    }
}

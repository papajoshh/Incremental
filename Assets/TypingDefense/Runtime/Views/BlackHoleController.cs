using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class BlackHoleController : MonoBehaviour
    {
        [SerializeField] TrailRenderer trail;
        [SerializeField] ParticleSystem ambientParticles;

        CollectionPhaseConfig _config;
        GameFlowController _gameFlow;
        ArenaView _arenaView;
        LetterTracker _letterTracker;
        LetterConfig _letterConfig;
        RunManager _runManager;
        CameraShaker _cameraShaker;
        CollectionPhaseController _collectionPhase;
        PlayerStats _playerStats;

        int _collectStreak;
        bool _imploding;
        bool _charging;

        public static Vector3 AttractionTarget { get; private set; }
        public static float AttractionSpeed { get; private set; }

        public Vector3 Position => transform.position;

        public event Action<PhysicalLetter> OnLetterCollected;

        [Inject]
        public void Construct(
            CollectionPhaseConfig config,
            GameFlowController gameFlow,
            ArenaView arenaView,
            LetterTracker letterTracker,
            LetterConfig letterConfig,
            RunManager runManager,
            CameraShaker cameraShaker,
            CollectionPhaseController collectionPhase,
            PlayerStats playerStats)
        {
            _config = config;
            _gameFlow = gameFlow;
            _arenaView = arenaView;
            _letterTracker = letterTracker;
            _letterConfig = letterConfig;
            _runManager = runManager;
            _cameraShaker = cameraShaker;
            _collectionPhase = collectionPhase;
            _playerStats = playerStats;

            gameFlow.OnStateChanged += OnStateChanged;
            runManager.OnRunEnded += OnGameOver;
            collectionPhase.OnChargeStarted += OnChargeStarted;
            collectionPhase.OnFreezeReleased += OnChargeReleased;

            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            _gameFlow.OnStateChanged -= OnStateChanged;
            _runManager.OnRunEnded -= OnGameOver;
            _collectionPhase.OnChargeStarted -= OnChargeStarted;
            _collectionPhase.OnFreezeReleased -= OnChargeReleased;
            transform.DOKill();
        }

        void OnStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    transform.position = _arenaView.CenterPosition;
                    gameObject.SetActive(true);
                    transform.localScale = Vector3.zero;
                    transform.DOScale(1f, 2f).SetEase(Ease.OutBack);
                    trail.enabled = false;
                    ambientParticles.Play();
                    _collectStreak = 0;
                    _imploding = false;
                    _charging = false;
                    UpdateAttractionStatics();
                    break;

                case GameState.Collecting:
                    trail.Clear();
                    trail.enabled = true;
                    break;

                case GameState.Menu:
                    PhysicalLetter.ExpireAll();
                    trail.enabled = false;
                    ambientParticles.Stop();
                    transform.DOKill();
                    _imploding = false;
                    AttractionSpeed = 0f;
                    gameObject.SetActive(false);
                    break;
            }
        }

        void OnChargeStarted(float chargeDuration)
        {
            _charging = true;

            // Pulsing scale during charge — intensity builds over time
            transform.DOKill();
            var seq = DOTween.Sequence().SetUpdate(true);
            var steps = 8;
            var stepDur = chargeDuration / steps;
            for (var i = 0; i < steps; i++)
            {
                var t = (float)(i + 1) / steps;
                var pulseSize = Mathf.Lerp(1.05f, 1.25f, t * t);
                seq.Append(transform.DOScale(pulseSize, stepDur * 0.4f).SetEase(Ease.OutQuad).SetUpdate(true));
                seq.Append(transform.DOScale(1f, stepDur * 0.6f).SetEase(Ease.InQuad).SetUpdate(true));
            }
        }

        void OnChargeReleased()
        {
            _charging = false;

            // Snap to normal scale after charge
            transform.DOKill();
            transform.localScale = Vector3.one * 1.3f;
            transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        void TransitionOut()
        {
            transform.DOKill();
            transform.DOScale(0f, _config.transitionOutDuration)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() => gameObject.SetActive(false));
        }

        void OnGameOver()
        {
            if (_gameFlow.State != GameState.Collecting && _gameFlow.State != GameState.Playing) return;

            _imploding = true;

            transform.DOKill();
            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(transform.DOScale(1.6f, 0.15f).SetEase(Ease.OutQuad));
            seq.Append(transform.DOScale(0f, 0.4f).SetEase(Ease.InBack));
            seq.Join(transform.DORotate(new Vector3(0, 0, 720), 0.4f, RotateMode.FastBeyond360)
                .SetEase(Ease.InQuad));
            seq.OnComplete(() =>
            {
                _imploding = false;
                gameObject.SetActive(false);
            });

            _cameraShaker.Shake(0.6f, 0.5f);
        }

        void Update()
        {
            var state = _gameFlow.State;
            if (state != GameState.Playing && state != GameState.Collecting) return;
            if (_charging) return;

            AttractionTarget = transform.position;

            MoveWithArrowKeys();
            CheckLetterCollection();
        }

        void MoveWithArrowKeys()
        {
            var input = Vector3.zero;

            if (Input.GetKey(KeyCode.UpArrow)) input.y += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) input.y -= 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) input.x -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) input.x += 1f;

            if (input.sqrMagnitude < 0.01f) return;

            input.Normalize();
            var newPos = transform.position + input * _playerStats.CollectionSpeed * Time.unscaledDeltaTime;
            transform.position = _arenaView.ClampToInterior(newPos);
        }

        void CheckLetterCollection()
        {
            var pos = transform.position;
            var radiusSq = _config.collectRadius * _config.collectRadius;

            for (var i = PhysicalLetter.Active.Count - 1; i >= 0; i--)
            {
                var letter = PhysicalLetter.Active[i];
                if (letter.IsCollected) continue;

                var dist = (letter.transform.position - pos).sqrMagnitude;
                if (dist > radiusSq) continue;

                CollectLetter(letter);
            }
        }

        void CollectLetter(PhysicalLetter letter)
        {
            var coins = _letterConfig.GetConversionValue(letter.Type);
            _letterTracker.DirectAddCoins(coins);
            OnLetterCollected?.Invoke(letter);
            letter.Collect(transform.position);

            _collectStreak++;
            var punchIntensity = Mathf.Lerp(0.08f, 0.25f, Mathf.Clamp01(_collectStreak / 20f));
            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * punchIntensity, 0.15f, 10, 0f).SetUpdate(true);

            if (_collectStreak % 5 == 0)
                _cameraShaker.Shake(0.05f, 0.08f);
        }

        void UpdateAttractionStatics()
        {
            AttractionTarget = transform.position;
            AttractionSpeed = _config.letterDriftSpeed + _playerStats.LetterAttraction;
        }
    }
}

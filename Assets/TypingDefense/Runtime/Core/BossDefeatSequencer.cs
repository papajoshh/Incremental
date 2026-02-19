using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class BossDefeatSequencer : IInitializable, IDisposable
    {
        readonly WordManager _wordManager;
        readonly GameFlowController _gameFlow;
        readonly CameraShaker _cameraShaker;
        readonly BlackHoleController _blackHole;
        readonly PostProcessJuiceController _postProcess;

        const float FreezeFrameDuration = 0.15f;
        const float DissipateDelay = 0.3f;
        const float OrbitDuration = 1.5f;
        const float PostOrbitPause = 0.6f;
        const float ReturnToMenuDelay = 0.8f;

        public event Action OnSequenceCompleted;

        public BossDefeatSequencer(
            WordManager wordManager,
            GameFlowController gameFlow,
            CameraShaker cameraShaker,
            BlackHoleController blackHole,
            PostProcessJuiceController postProcess)
        {
            _wordManager = wordManager;
            _gameFlow = gameFlow;
            _cameraShaker = cameraShaker;
            _blackHole = blackHole;
            _postProcess = postProcess;
        }

        public void Initialize()
        {
            _wordManager.OnBossDefeated += OnBossDefeated;
        }

        public void Dispose()
        {
            _wordManager.OnBossDefeated -= OnBossDefeated;
        }

        void OnBossDefeated(DefenseWord word)
        {
            _wordManager.PauseSpawning();
            PlaySequence();
        }

        void PlaySequence()
        {
            var bhPos = _blackHole.Position;
            var seq = DOTween.Sequence().SetUpdate(true);

            // Phase 0: Freeze frame on boss kill
            seq.AppendCallback(() => Time.timeScale = 0f);
            seq.AppendInterval(FreezeFrameDuration);
            seq.AppendCallback(() => Time.timeScale = 1f);

            // Phase 1: Bloom burst + chromatic on boss death
            seq.AppendCallback(() => _postProcess.PlayBossDeathBurst());

            // Phase 2: Dissipate all active words (no reward)
            seq.AppendInterval(DissipateDelay);
            seq.AppendCallback(() => _wordManager.DissipateAllWords());

            // Phase 3: After a beat, orbit all physical letters into the black hole
            seq.AppendInterval(0.5f);
            seq.AppendCallback(() =>
            {
                var letters = PhysicalLetter.Active;
                for (var i = 0; i < letters.Count; i++)
                {
                    var letter = letters[i];
                    if (letter.IsCollected) continue;

                    var offset = letter.transform.position - bhPos;
                    var startAngle = Mathf.Atan2(offset.y, offset.x);
                    var capturedRadius = Mathf.Max(offset.magnitude, 0.5f);
                    var capturedAngle = startAngle;
                    var letterTransform = letter.transform;

                    DOTween.To(() => capturedAngle, angle =>
                    {
                        capturedAngle = angle;
                        var t = Mathf.InverseLerp(startAngle, startAngle + 12f, angle);
                        var r = capturedRadius * (1f - t);
                        r = Mathf.Max(r, 0.1f);
                        letterTransform.position = new Vector3(
                            bhPos.x + Mathf.Cos(angle) * r,
                            bhPos.y + Mathf.Sin(angle) * r,
                            letterTransform.position.z);
                    }, capturedAngle + Mathf.PI * 6f, OrbitDuration)
                        .SetEase(Ease.InQuad).SetUpdate(true);

                    letterTransform.DORotate(new Vector3(0, 0, 720f), OrbitDuration, RotateMode.FastBeyond360)
                        .SetEase(Ease.InQuad).SetUpdate(true);
                    letterTransform.DOScale(0f, OrbitDuration).SetEase(Ease.InCubic).SetUpdate(true);
                }
            });

            // Phase 3b: Escalating camera shake during orbit
            var orbitStart = FreezeFrameDuration + DissipateDelay + 0.5f;
            seq.InsertCallback(orbitStart, () => _cameraShaker.Shake(0.1f, 0.3f));
            seq.InsertCallback(orbitStart + 0.4f, () => _cameraShaker.Shake(0.2f, 0.3f));
            seq.InsertCallback(orbitStart + 0.8f, () => _cameraShaker.Shake(0.35f, 0.3f));
            seq.InsertCallback(orbitStart + 1.2f, () => _cameraShaker.Shake(0.5f, 0.4f));

            // Phase 4: Final absorption burst
            seq.AppendInterval(OrbitDuration);
            seq.AppendCallback(() =>
            {
                for (var i = PhysicalLetter.Active.Count - 1; i >= 0; i--)
                {
                    var letter = PhysicalLetter.Active[i];
                    if (!letter.IsCollected) letter.Collect(bhPos);
                }

                _cameraShaker.Shake(0.7f, 0.5f, 20);
                _postProcess.PlayAbsorptionPulse();
            });

            // Phase 5: Save progression + return to menu
            seq.AppendInterval(PostOrbitPause);
            seq.AppendCallback(() => _gameFlow.SaveBossProgression());

            seq.AppendInterval(ReturnToMenuDelay);
            seq.AppendCallback(() =>
            {
                _gameFlow.ReturnToMenu();
                OnSequenceCompleted?.Invoke();
            });
        }
    }
}

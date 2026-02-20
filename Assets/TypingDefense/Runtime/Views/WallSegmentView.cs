using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class WallSegmentView : MonoBehaviour
    {
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] TextMeshPro wordLabel;
        [SerializeField] ParticleSystem breakParticles;
        [SerializeField] SpriteRenderer flashRenderer;
        [SerializeField] SpriteRenderer targetIndicator;

        WallConfig _wallConfig;
        WallSegmentId _id;
        bool _revealed;
        bool _broken;
        float _baseLineWidth;

        public WallSegmentId Id => _id;
        public bool IsRevealed => _revealed;
        public Vector3 MidpointPosition { get; private set; }

        [Inject]
        public void Construct(WallConfig wallConfig)
        {
            _wallConfig = wallConfig;
        }

        public void Setup(WallSegmentId id, Vector3 startPoint, Vector3 endPoint, string wordText)
        {
            _id = id;
            _broken = false;
            _revealed = false;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
            lineRenderer.startWidth = _wallConfig.segmentLineWidth;
            lineRenderer.endWidth = _wallConfig.segmentLineWidth;
            lineRenderer.startColor = _wallConfig.wallLineColor;
            lineRenderer.endColor = _wallConfig.wallLineColor;
            _baseLineWidth = _wallConfig.segmentLineWidth;

            var midpoint = (startPoint + endPoint) / 2f;
            wordLabel.transform.position = midpoint;
            MidpointPosition = midpoint;
            wordLabel.text = wordText;
            wordLabel.alpha = 0f;

            if (flashRenderer != null)
            {
                flashRenderer.transform.position = midpoint;
                flashRenderer.color = new Color(1f, 1f, 1f, 0f);
            }

            transform.localScale = new Vector3(1f, 0f, 1f);
            transform.DOScaleY(1f, 0.4f).SetEase(Ease.OutBack);
        }

        public void SetRevealed(bool revealed)
        {
            if (_broken || _revealed == revealed) return;
            _revealed = revealed;

            var targetAlpha = revealed ? _wallConfig.wordRevealAlpha : 0f;
            wordLabel.DOComplete();
            wordLabel.DOFade(targetAlpha, 0.3f).SetEase(Ease.OutQuad);
        }

        public void SetRevealedDelayed(bool revealed, float delay)
        {
            if (_broken || _revealed == revealed) return;
            _revealed = revealed;

            var targetAlpha = revealed ? _wallConfig.wordRevealAlpha : 0f;

            var seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.AppendCallback(() =>
            {
                lineRenderer.startColor = Color.white;
                lineRenderer.endColor = Color.white;
            });
            seq.Append(DOTween.To(
                () => lineRenderer.startColor,
                c => { lineRenderer.startColor = c; lineRenderer.endColor = c; },
                _wallConfig.wallLineColor, 0.3f));
            seq.Join(wordLabel.DOFade(targetAlpha, 0.4f).SetEase(Ease.OutQuad));
            seq.Join(wordLabel.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0.5f));
        }

        public void SetTargeted(bool targeted)
        {
            targetIndicator.enabled = targeted;
            targetIndicator.transform.DOKill();
            if (!targeted) return;
            targetIndicator.transform.rotation = Quaternion.identity;
            targetIndicator.transform.DORotate(new Vector3(0, 0, -360), 3f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        public void UpdateMatchProgress(int matchedCount, string fullText)
        {
            if (!_revealed || _broken) return;

            var matched = fullText.Substring(0, matchedCount);
            var remaining = fullText.Substring(matchedCount);
            wordLabel.text = $"<color=#00E5FF>{matched}</color><color=#B0BEC5>{remaining}</color>";

            var progress = (float)matchedCount / fullText.Length;

            var punchIntensity = Mathf.Lerp(0.12f, 0.25f, progress);
            wordLabel.transform.DOComplete();
            wordLabel.transform.DOPunchScale(Vector3.one * punchIntensity, 0.15f, 8, 0.5f);

            transform.DOComplete();
            transform.DOShakePosition(0.1f, 0.05f, 10);

            var targetColor = Color.Lerp(_wallConfig.wallLineColor, _wallConfig.wallLineBreakColor, progress);
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
            DOTween.To(
                () => lineRenderer.startColor,
                c => { lineRenderer.startColor = c; lineRenderer.endColor = c; },
                targetColor, 0.15f).SetEase(Ease.OutQuad);
        }

        public void PlayBreakAnimation()
        {
            _broken = true;
            SetTargeted(false);

            Time.timeScale = 0.1f;
            DOVirtual.DelayedCall(0.05f, () => Time.timeScale = 1f).SetUpdate(true);

            if (flashRenderer != null)
            {
                flashRenderer.color = new Color(1f, 1f, 1f, 0.8f);
                flashRenderer.DOFade(0f, 0.4f).SetEase(Ease.OutQuad);
                flashRenderer.transform.DOScale(1.5f, 0.4f).SetEase(Ease.OutQuad);
            }

            if (breakParticles != null)
            {
                var main = breakParticles.main;
                main.startColor = _wallConfig.wallLineBreakColor;
                breakParticles.Play();
            }

            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(DOTween.To(
                () => lineRenderer.startWidth,
                w => { lineRenderer.startWidth = w; lineRenderer.endWidth = w; },
                0f, 0.35f
            ).SetEase(Ease.InQuad));

            wordLabel.transform.DOPunchScale(Vector3.one * 0.3f, 0.15f, 12, 0.5f);
            wordLabel.DOFade(0f, 0.25f).SetEase(Ease.InQuad);

            transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 0.5f);

            seq.OnComplete(() => DOVirtual.DelayedCall(1f, () => Destroy(gameObject)));
        }

        public void PlayNeighborFlinch()
        {
            transform.DOComplete();
            transform.DOShakePosition(0.2f, 0.08f, 14);

            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
            DOTween.To(
                () => lineRenderer.startColor,
                c => { lineRenderer.startColor = c; lineRenderer.endColor = c; },
                _wallConfig.wallLineColor, 0.2f);
        }

        public void SetBrokenImmediate()
        {
            _broken = true;
            Destroy(gameObject);
        }

        void Update()
        {
            if (_broken || !lineRenderer.enabled) return;

            var breath = 1f + Mathf.Sin(Time.time * 2f + _id.Ring * 0.5f + _id.Index * 0.3f) * 0.02f;
            lineRenderer.startWidth = _baseLineWidth * breath;
            lineRenderer.endWidth = _baseLineWidth * breath;
        }

        void OnDestroy()
        {
            transform.DOKill();
            wordLabel.DOKill();
            if (flashRenderer != null) flashRenderer.DOKill();
            targetIndicator.transform.DOKill();
        }

        public class Factory : PlaceholderFactory<WallSegmentView> { }
    }
}
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class DefenseWordView : MonoBehaviour
    {
        [SerializeField] TextMeshPro label;
        [SerializeField] SpriteRenderer dissolveRenderer;
        [SerializeField] SpriteRenderer hpBarBg;
        [SerializeField] SpriteRenderer hpBarFill;
        [SerializeField] float arrivalThreshold = 0.3f;

        static readonly Color HealthyColor = new(0.3f, 0.85f, 0.2f);
        static readonly Color HurtColor = new(1f, 0.85f, 0f);
        static readonly Color CriticalColor = new(0.9f, 0.15f, 0.1f);

        static readonly int CutoffId = Shader.PropertyToID("_Cutoff");
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int EdgeColorId = Shader.PropertyToID("_EdgeColor");
        static readonly int EdgeWidthId = Shader.PropertyToID("_EdgeWidth");
        static readonly int NoiseOffsetId = Shader.PropertyToID("_NoiseOffset");

        DefenseWord _word;
        Vector3 _targetPosition;
        float _speed;
        WordManager _wordManager;
        GameFlowController _gameFlow;
        BlackHoleController _blackHole;
        CollectionPhaseConfig _collectionConfig;
        CollectionPhaseController _collectionPhase;
        bool _isDead;
        Material _dissolveMaterial;
        float _hpBarFullWidth;

        public DefenseWord Word => _word;
        public Vector3 LastPosition => transform.position;

        [Inject]
        public void Construct(
            WordManager wordManager,
            GameFlowController gameFlow,
            BlackHoleController blackHole,
            CollectionPhaseConfig collectionConfig,
            CollectionPhaseController collectionPhase)
        {
            _wordManager = wordManager;
            _gameFlow = gameFlow;
            _blackHole = blackHole;
            _collectionConfig = collectionConfig;
            _collectionPhase = collectionPhase;
        }

        public void Setup(DefenseWord word, Vector3 startPos, Vector3 targetPos, float speed)
        {
            _word = word;
            _targetPosition = targetPos;
            _speed = speed;
            transform.position = startPos;
            _isDead = false;

            if (dissolveRenderer != null)
            {
                _dissolveMaterial = dissolveRenderer.material;
                _dissolveMaterial.SetFloat(CutoffId, 0f);
                _dissolveMaterial.SetVector(NoiseOffsetId, new Vector4(Random.Range(0f, 100f), Random.Range(0f, 100f), 0f, 0f));
                dissolveRenderer.enabled = true;
            }

            transform.localScale = Vector3.zero;
            transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            label.alpha = 0f;
            label.DOFade(1f, 0.15f);

            UpdateLabel();
            SetupHpBar();
        }

        void Update()
        {
            if (_isDead) return;

            var state = _gameFlow.State;
            if (state == GameState.Collecting)
            {
                HomeTowardsBlackHole();
                return;
            }

            if (_speed > 0f)
            {
                var target = _blackHole.Position;
                transform.position = Vector3.MoveTowards(
                    transform.position, target, _speed * Time.deltaTime);

                if (Vector3.Distance(transform.position, target) <= arrivalThreshold)
                {
                    WordReachedBlackHole();
                    return;
                }
            }

            UpdateLabel();
        }

        void HomeTowardsBlackHole()
        {
            var bhPos = _blackHole.Position;
            // Uses Time.deltaTime (affected by slow-mo, so words move slowly)
            transform.position = Vector3.MoveTowards(
                transform.position, bhPos, _collectionConfig.wordHomingSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, bhPos) > arrivalThreshold) return;

            _isDead = true;
            _collectionPhase.HandleWordHitBlackHole();
            _wordManager.HandleWordReachedBlackHole(_word);

            // Spaghettification into BH
            var dirToBH = (bhPos - transform.position).normalized;
            var angle = Mathf.Atan2(dirToBH.y, dirToBH.x) * Mathf.Rad2Deg;

            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(transform.DORotate(new Vector3(0, 0, angle), 0.1f));
            seq.Join(transform.DOScaleX(2.5f, 0.3f).SetEase(Ease.InQuad));
            seq.Join(transform.DOScaleY(0.15f, 0.3f).SetEase(Ease.InQuad));
            seq.Join(transform.DOMove(bhPos, 0.3f).SetEase(Ease.InQuad));
            seq.Join(label.DOFade(0f, 0.2f));
            seq.OnComplete(() => Destroy(gameObject));
        }

        public void OnTextChanged()
        {
            UpdateLabel();
            UpdateHpBar();

            transform.DOComplete();
            transform.DOShakePosition(0.2f, 0.3f, 12);
        }

        void UpdateLabel()
        {
            var matched = _word.Text.Substring(0, _word.MatchedCount);
            var remaining = _word.Text.Substring(_word.MatchedCount);
            var matchedColor = _word.Type == WordType.Blue ? "#00B8D4" : "#2E7D32";
            var remainingColor = _word.Type == WordType.Blue ? "#0277BD" : "#263238";
            label.text = $"<color={matchedColor}>{matched}</color><color={remainingColor}>{remaining}</color>";
        }

        void SetupHpBar()
        {
            var showBar = _word.MaxHp > 1;
            hpBarBg.gameObject.SetActive(showBar);
            hpBarFill.gameObject.SetActive(showBar);

            if (!showBar) return;

            _hpBarFullWidth = hpBarFill.transform.localScale.x;
            hpBarFill.color = HealthyColor;
        }

        void UpdateHpBar()
        {
            if (_word.MaxHp <= 1) return;

            var ratio = (float)_word.CurrentHp / _word.MaxHp;
            var targetScaleX = _hpBarFullWidth * ratio;

            hpBarFill.transform.DOComplete();
            hpBarFill.transform.DOScaleX(targetScaleX, 0.15f).SetEase(Ease.OutCubic);

            var posOffset = -(_hpBarFullWidth - targetScaleX) * 0.5f;
            hpBarFill.transform.DOLocalMoveX(posOffset, 0.15f).SetEase(Ease.OutCubic);

            var color = ratio > 0.5f
                ? Color.Lerp(HurtColor, HealthyColor, (ratio - 0.5f) * 2f)
                : Color.Lerp(CriticalColor, HurtColor, ratio * 2f);
            hpBarFill.DOComplete();
            hpBarFill.DOColor(color, 0.15f);

            hpBarFill.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 8, 0f);
        }

        void WordReachedBlackHole()
        {
            _isDead = true;
            _wordManager.HandleWordReachedCenter(_word);

            var bhPos = _blackHole.Position;
            var dirToBH = (bhPos - transform.position).normalized;
            var angle = Mathf.Atan2(dirToBH.y, dirToBH.x) * Mathf.Rad2Deg;

            var seq = DOTween.Sequence();
            seq.Append(transform.DORotate(new Vector3(0, 0, angle), 0.1f));
            seq.Join(transform.DOScaleX(2.5f, 0.35f).SetEase(Ease.InQuad));
            seq.Join(transform.DOScaleY(0.15f, 0.35f).SetEase(Ease.InQuad));
            seq.Join(transform.DOMove(bhPos, 0.35f).SetEase(Ease.InQuad));
            seq.Join(label.DOFade(0f, 0.25f));
            seq.OnComplete(() => Destroy(gameObject));
        }

        public void OnCompleted()
        {
            _isDead = true;

            label.color = Color.white;
            label.DOColor(new Color(0.3f, 1f, 0.3f, 0f), 0.3f);

            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, 10, 0.5f);
            DOVirtual.DelayedCall(0.15f, () =>
            {
                transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutQuad)
                    .OnComplete(() => Destroy(gameObject));
            });

            PlayDissolve(new Color(0.3f, 1f, 0.3f), new Color(0.8f, 1f, 0.8f), 0.05f, 0.3f);
        }

        public void OnCriticalKill()
        {
            _isDead = true;

            Time.timeScale = 0.05f;
            DOVirtual.DelayedCall(0.03f, () => Time.timeScale = 1f).SetUpdate(true);

            label.color = Color.white;
            var seq = DOTween.Sequence();
            seq.AppendInterval(0.06f);
            seq.AppendCallback(() => label.DOColor(new Color(1f, 0.84f, 0f, 0f), 0.35f));

            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.4f, 0.25f, 12, 0.7f);
            DOVirtual.DelayedCall(0.18f, () =>
            {
                transform.DOScale(2f, 0.3f).SetEase(Ease.OutExpo)
                    .OnComplete(() => Destroy(gameObject));
            }).SetUpdate(true);

            PlayDissolve(new Color(1f, 0.84f, 0f), new Color(1f, 1f, 0.5f), 0.12f, 0.4f);
        }

        public void OnDissipated()
        {
            _isDead = true;

            label.DOFade(0f, 0.4f).SetEase(Ease.InQuad);
            transform.DOScale(0.5f, 0.4f).SetEase(Ease.InBack)
                .OnComplete(() => Destroy(gameObject));

            PlayDissolve(new Color(0.5f, 0.5f, 0.5f), new Color(0.3f, 0.3f, 0.3f), 0.08f, 0.4f);
        }

        void PlayDissolve(Color mainColor, Color edgeColor, float edgeWidth, float duration)
        {
            if (_dissolveMaterial == null) return;

            dissolveRenderer.enabled = true;
            _dissolveMaterial.SetColor(ColorId, mainColor);
            _dissolveMaterial.SetColor(EdgeColorId, edgeColor);
            _dissolveMaterial.SetFloat(EdgeWidthId, edgeWidth);
            _dissolveMaterial.SetFloat(CutoffId, 0f);

            _dissolveMaterial.DOFloat(1f, CutoffId, duration).SetEase(Ease.InQuad);
        }

        void OnDestroy()
        {
            transform.DOKill();
            if (_dissolveMaterial != null)
                Destroy(_dissolveMaterial);
        }

        public class Factory : PlaceholderFactory<DefenseWordView> { }
    }
}

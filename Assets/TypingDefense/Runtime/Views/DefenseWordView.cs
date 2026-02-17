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

        DefenseWord word;
        Vector3 targetPosition;
        float speed;
        WordManager wordManager;
        bool isDead;
        Material dissolveMaterial;
        float hpBarFullWidth;

        [Inject]
        public void Construct(WordManager wordManager)
        {
            this.wordManager = wordManager;
        }

        public void Setup(DefenseWord word, Vector3 startPos, Vector3 targetPos, float speed)
        {
            this.word = word;
            targetPosition = targetPos;
            this.speed = speed;
            transform.position = startPos;
            isDead = false;

            if (dissolveRenderer != null)
            {
                dissolveMaterial = dissolveRenderer.material;
                dissolveMaterial.SetFloat(CutoffId, 0f);
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
            if (isDead) return;

            if (speed > 0f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, targetPosition, speed * Time.deltaTime);

                if (Vector3.Distance(transform.position, targetPosition) <= arrivalThreshold)
                {
                    WordReachedCenter();
                    return;
                }
            }

            UpdateLabel();
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
            var matched = word.Text.Substring(0, word.MatchedCount);
            var remaining = word.Text.Substring(word.MatchedCount);
            label.text = $"<color=#2E7D32>{matched}</color><color=#263238>{remaining}</color>";
        }

        void SetupHpBar()
        {
            var showBar = word.MaxHp > 1;
            hpBarBg.gameObject.SetActive(showBar);
            hpBarFill.gameObject.SetActive(showBar);

            if (!showBar) return;

            hpBarFullWidth = hpBarFill.transform.localScale.x;
            hpBarFill.color = HealthyColor;
        }

        void UpdateHpBar()
        {
            if (word.MaxHp <= 1) return;

            var ratio = (float)word.CurrentHp / word.MaxHp;
            var targetScaleX = hpBarFullWidth * ratio;

            hpBarFill.transform.DOComplete();
            hpBarFill.transform.DOScaleX(targetScaleX, 0.15f).SetEase(Ease.OutCubic);

            // Shift left so bar drains from right
            var posOffset = -(hpBarFullWidth - targetScaleX) * 0.5f;
            hpBarFill.transform.DOLocalMoveX(posOffset, 0.15f).SetEase(Ease.OutCubic);

            var color = ratio > 0.5f
                ? Color.Lerp(HurtColor, HealthyColor, (ratio - 0.5f) * 2f)
                : Color.Lerp(CriticalColor, HurtColor, ratio * 2f);
            hpBarFill.DOComplete();
            hpBarFill.DOColor(color, 0.15f);

            hpBarFill.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 8, 0f);
        }

        void WordReachedCenter()
        {
            isDead = true;
            wordManager.HandleWordReachedCenter(word);
            transform.DOShakePosition(0.3f, 0.5f)
                .OnComplete(() => Destroy(gameObject));
        }

        public void OnCompleted()
        {
            isDead = true;

            label.color = Color.white;
            label.DOColor(new Color(0.3f, 1f, 0.3f, 0f), 0.3f);

            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.15f, 0.1f, 10, 0f);
            DOVirtual.DelayedCall(0.1f, () =>
            {
                transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutQuad)
                    .OnComplete(() => Destroy(gameObject));
            });

            PlayDissolve(new Color(0.3f, 1f, 0.3f), new Color(0.8f, 1f, 0.8f), 0.05f, 0.3f);
        }

        public void OnCriticalKill()
        {
            isDead = true;

            label.color = Color.white;
            var seq = DOTween.Sequence();
            seq.AppendInterval(0.05f);
            seq.AppendCallback(() => label.DOColor(new Color(1f, 0.84f, 0f, 0f), 0.35f));

            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.3f, 0.12f, 12, 0f);
            DOVirtual.DelayedCall(0.12f, () =>
            {
                transform.DOScale(2f, 0.3f).SetEase(Ease.OutExpo)
                    .OnComplete(() => Destroy(gameObject));
            });

            PlayDissolve(new Color(1f, 0.84f, 0f), new Color(1f, 1f, 0.5f), 0.12f, 0.4f);
        }

        void PlayDissolve(Color mainColor, Color edgeColor, float edgeWidth, float duration)
        {
            if (dissolveMaterial == null) return;

            dissolveRenderer.enabled = true;
            dissolveMaterial.SetColor(ColorId, mainColor);
            dissolveMaterial.SetColor(EdgeColorId, edgeColor);
            dissolveMaterial.SetFloat(EdgeWidthId, edgeWidth);
            dissolveMaterial.SetFloat(CutoffId, 0f);

            dissolveMaterial.DOFloat(1f, CutoffId, duration).SetEase(Ease.InQuad);
        }

        void OnDestroy()
        {
            if (dissolveMaterial != null)
                Destroy(dissolveMaterial);
        }

        public class Factory : PlaceholderFactory<DefenseWordView> { }
    }
}

using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class BossWordView : MonoBehaviour
    {
        [SerializeField] TextMeshPro label;
        [SerializeField] SpriteRenderer glowRenderer;
        [SerializeField] SpriteRenderer hpBarBg;
        [SerializeField] SpriteRenderer hpBarFill;
        [SerializeField] SpriteRenderer targetIndicator;

        static readonly Color HealthyColor = new(0.3f, 0.85f, 0.2f);
        static readonly Color HurtColor = new(1f, 0.85f, 0f);
        static readonly Color CriticalColor = new(0.9f, 0.15f, 0.1f);

        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int PulseSpeedId = Shader.PropertyToID("_PulseSpeed");
        static readonly int PulseMaxId = Shader.PropertyToID("_PulseMax");

        DefenseWord word;
        BossConfig bossConfig;
        Vector3 orbitCenter;
        float currentAngle;
        bool isDead;
        Material glowMaterial;
        float hpBarFullWidth;

        [Inject]
        public void Construct(BossConfig bossConfig)
        {
            this.bossConfig = bossConfig;
        }

        public void Setup(DefenseWord word, Vector3 center)
        {
            this.word = word;
            orbitCenter = center;
            currentAngle = 0f;
            isDead = false;
            UpdatePosition();
            UpdateLabel();

            if (glowRenderer != null)
            {
                glowMaterial = glowRenderer.material;
                glowMaterial.SetFloat(PulseSpeedId, 3f);
                glowMaterial.SetFloat(PulseMaxId, 0.5f);
            }

            hpBarFullWidth = hpBarFill.transform.localScale.x;
            hpBarFill.color = HealthyColor;

            transform.localScale = Vector3.zero;
            transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack);
        }

        void Update()
        {
            if (isDead) return;

            currentAngle += bossConfig.orbitalSpeed * Time.deltaTime;
            UpdatePosition();
            UpdateLabel();
        }

        void UpdatePosition()
        {
            var rad = currentAngle * Mathf.Deg2Rad;
            var offset = new Vector3(
                Mathf.Cos(rad) * bossConfig.orbitalRadius,
                Mathf.Sin(rad) * bossConfig.orbitalRadius,
                0f);
            transform.position = orbitCenter + offset;
        }

        void UpdateLabel()
        {
            var matched = word.Text.Substring(0, word.MatchedCount);
            var remaining = word.Text.Substring(word.MatchedCount);
            label.text = $"<color=#FF6600>{matched}</color><color=#FF0000>{remaining}</color>";
        }

        void UpdateHpBar()
        {
            var ratio = (float)word.CurrentHp / word.MaxHp;
            var targetScaleX = hpBarFullWidth * ratio;

            hpBarFill.transform.DOComplete();
            hpBarFill.transform.DOScaleX(targetScaleX, 0.2f).SetEase(Ease.OutCubic);

            // Shift left so bar drains from right
            var posOffset = -(hpBarFullWidth - targetScaleX) * 0.5f;
            hpBarFill.transform.DOLocalMoveX(posOffset, 0.2f).SetEase(Ease.OutCubic);

            var color = ratio > 0.5f
                ? Color.Lerp(HurtColor, HealthyColor, (ratio - 0.5f) * 2f)
                : Color.Lerp(CriticalColor, HurtColor, ratio * 2f);
            hpBarFill.DOComplete();
            hpBarFill.DOColor(color, 0.2f);

            hpBarFill.transform.DOPunchScale(Vector3.one * 0.25f, 0.25f, 10, 0f);

            hpBarBg.transform.DOComplete();
            hpBarBg.transform.DOPunchScale(Vector3.one * 0.1f, 0.15f, 6, 0f);
        }

        public void OnHit()
        {
            UpdateHpBar();

            transform.DOComplete();
            transform.DOShakePosition(0.15f, 0.4f, 15);

            UpdateGlowIntensity();
        }

        void UpdateGlowIntensity()
        {
            if (glowMaterial == null) return;

            var hpRatio = (float)word.CurrentHp / word.MaxHp;
            var urgency = Mathf.Pow(1f - hpRatio, 2f);

            var targetSpeed = Mathf.Lerp(3f, 15f, urgency);
            var targetMax = Mathf.Lerp(0.5f, 2.5f, urgency);
            var targetColor = Color.Lerp(
                new Color(1f, 0.3f, 0f),
                new Color(1f, 0f, 0f),
                urgency);

            // Flare bright on hit, then settle to new baseline
            glowMaterial.SetFloat(PulseMaxId, targetMax + 2f);
            glowMaterial.SetColor(ColorId, Color.white);
            glowMaterial.DOFloat(targetMax, PulseMaxId, 0.25f).SetEase(Ease.OutCubic);
            glowMaterial.DOColor(targetColor, ColorId, 0.25f).SetEase(Ease.OutCubic);
            glowMaterial.SetFloat(PulseSpeedId, targetSpeed);

            glowRenderer.transform.DOComplete();
            glowRenderer.transform.DOPunchScale(Vector3.one * 0.4f, 0.2f, 6, 0.5f);
        }

        public void OnTextChanged()
        {
            UpdateLabel();
            UpdateHpBar();

            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 8);
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

        public void OnDefeated()
        {
            isDead = true;
            SetTargeted(false);

            label.color = Color.white;
            label.DOColor(new Color(1f, 0.5f, 0f, 0f), 0.6f).SetUpdate(true);
            transform.DOScale(3f, 0.6f).SetEase(Ease.OutExpo).SetUpdate(true);
            transform.DOShakePosition(0.6f, 1f, 20).SetUpdate(true);

            if (glowMaterial != null)
            {
                glowMaterial.SetFloat(PulseSpeedId, 20f);
                glowMaterial.SetFloat(PulseMaxId, 3f);
                glowMaterial.SetColor(ColorId, Color.white);
                glowRenderer.transform.DOScale(5f, 0.6f).SetEase(Ease.OutQuad).SetUpdate(true);
            }

            DOVirtual.DelayedCall(0.7f, () => Destroy(gameObject)).SetUpdate(true);
        }

        void OnDestroy()
        {
            transform.DOKill();
            targetIndicator.transform.DOKill();
            if (glowMaterial != null)
                Destroy(glowMaterial);
        }
    }
}

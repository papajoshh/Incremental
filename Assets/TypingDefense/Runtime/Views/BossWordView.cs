using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class BossWordView : MonoBehaviour
    {
        [SerializeField] TextMeshPro label;
        [SerializeField] TextMeshPro hpLabel;

        DefenseWord word;
        BossConfig bossConfig;
        Vector3 orbitCenter;
        float currentAngle;
        bool isDead;

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
            UpdateHpLabel();

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

        void UpdateHpLabel()
        {
            hpLabel.text = $"{word.CurrentHp}/{word.MaxHp}";
        }

        public void OnHit()
        {
            UpdateHpLabel();

            transform.DOComplete();
            transform.DOShakePosition(0.15f, 0.4f, 15);
        }

        public void OnTextChanged()
        {
            UpdateLabel();
            UpdateHpLabel();

            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 8);
        }

        public void OnDefeated()
        {
            isDead = true;

            label.DOColor(new Color(1f, 0.5f, 0f, 0f), 0.6f);
            transform.DOScale(3f, 0.6f).SetEase(Ease.OutExpo);
            transform.DOShakePosition(0.6f, 1f, 20)
                .OnComplete(() => Destroy(gameObject));
        }

        public class Factory : PlaceholderFactory<BossWordView> { }
    }
}

using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class DefenseWordView : MonoBehaviour
    {
        [SerializeField] TextMeshPro label;
        [SerializeField] TextMeshPro hpLabel;
        [SerializeField] float arrivalThreshold = 0.3f;

        DefenseWord word;
        Vector3 targetPosition;
        float speed;
        WordManager wordManager;
        bool isDead;

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
            UpdateLabel();
            UpdateHpLabel();
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
            UpdateHpLabel();

            transform.DOComplete();
            transform.DOShakePosition(0.2f, 0.3f, 12);
        }

        void UpdateLabel()
        {
            var matched = word.Text.Substring(0, word.MatchedCount);
            var remaining = word.Text.Substring(word.MatchedCount);
            label.text = $"<color=#4CAF50>{matched}</color>{remaining}";
        }

        void UpdateHpLabel()
        {
            if (word.MaxHp <= 1)
            {
                hpLabel.gameObject.SetActive(false);
                return;
            }

            hpLabel.gameObject.SetActive(true);
            hpLabel.text = $"{word.CurrentHp}/{word.MaxHp}";
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
            label.DOColor(new Color(0.3f, 1f, 0.3f, 0f), 0.3f);
            transform.DOScale(1.5f, 0.3f)
                .OnComplete(() => Destroy(gameObject));
        }

        public void OnCriticalKill()
        {
            isDead = true;
            label.DOColor(new Color(1f, 0.84f, 0f, 0f), 0.4f);
            transform.DOScale(2f, 0.4f)
                .OnComplete(() => Destroy(gameObject));
        }

        public class Factory : PlaceholderFactory<DefenseWordView> { }
    }
}

using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class PhysicalLetter : MonoBehaviour
    {
        [SerializeField] TextMeshPro label;
        [SerializeField] SpriteRenderer background;

        static readonly Color[] LetterColors =
        {
            new(0.6f, 0.6f, 0.6f),   // A - grey
            new(0.3f, 0.7f, 1f),      // B - blue
            new(0.3f, 1f, 0.3f),      // C - green
            new(1f, 0.6f, 0f),        // D - orange
            new(1f, 0.3f, 1f),        // E - purple
        };

        static readonly List<PhysicalLetter> _active = new();
        public static IReadOnlyList<PhysicalLetter> Active => _active;

        float _driftDelay;

        public LetterType Type { get; private set; }
        public bool IsCollected { get; private set; }

        void OnEnable() => _active.Add(this);
        void OnDisable() => _active.Remove(this);

        public void Setup(LetterType type, Vector3 position)
        {
            Type = type;
            IsCollected = false;
            transform.position = position;

            if (label != null)
                label.text = type.ToString();

            if (background != null)
                background.color = LetterColors[(int)type];

            transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));

            // Spawn juice: pop in + scatter from death position
            transform.localScale = Vector3.zero;
            var scatter = (Vector3)Random.insideUnitCircle * 0.8f;
            transform.DOMove(position + scatter, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);
            transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);

            _driftDelay = 0.3f;
        }

        public void Collect(Vector3 blackHolePos)
        {
            if (IsCollected) return;
            IsCollected = true;

            transform.DOComplete();
            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(transform.DOMove(blackHolePos, 0.2f).SetEase(Ease.InQuad));
            seq.Join(transform.DOScale(0f, 0.2f).SetEase(Ease.InBack));
            seq.Join(transform.DORotate(new Vector3(0, 0, 360), 0.2f, RotateMode.FastBeyond360));
            seq.OnComplete(() => Destroy(gameObject));
        }

        public void Expire()
        {
            if (IsCollected) return;
            IsCollected = true;

            transform.DOComplete();
            transform.DOScale(0f, 0.15f).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() => Destroy(gameObject));
        }

        public static void ExpireAll()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
                _active[i].Expire();
        }

        void Update()
        {
            if (IsCollected) return;

            var speed = BlackHoleController.AttractionSpeed;
            if (speed <= 0f) return;

            if (_driftDelay > 0f)
            {
                _driftDelay -= Time.unscaledDeltaTime;
                return;
            }

            var direction = (BlackHoleController.AttractionTarget - transform.position).normalized;
            transform.position += direction * speed * Time.unscaledDeltaTime;
        }

        void OnDestroy()
        {
            transform.DOKill();
        }

        public class Factory : PlaceholderFactory<PhysicalLetter> { }
    }
}

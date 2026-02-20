using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TypingDefense
{
    public class CoinPopup : MonoBehaviour
    {
        [SerializeField] TextMeshPro label;

        static readonly Stack<CoinPopup> _pool = new();

        const float Duration = 0.6f;
        const float RiseDistance = 0.8f;

        public static CoinPopup Get(CoinPopup prefab, Vector3 pos)
        {
            var popup = _pool.Count > 0 ? _pool.Pop() : Instantiate(prefab);
            popup.transform.position = pos;
            popup.gameObject.SetActive(true);
            return popup;
        }

        public void Play(int coins, LetterType type, float streakBonus)
        {
            label.text = $"+{coins}";
            label.color = PhysicalLetter.LetterColors[(int)type];

            var baseScale = 1f + (int)type * 0.15f + streakBonus;
            transform.localScale = Vector3.zero;

            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(transform.DOScale(baseScale, 0.12f).SetEase(Ease.OutBack));
            seq.Append(transform.DOMove(transform.position + Vector3.up * RiseDistance, Duration)
                .SetEase(Ease.OutQuad));
            seq.Join(label.DOFade(0f, Duration).SetEase(Ease.InQuad));
            seq.OnComplete(ReturnToPool);
        }

        void ReturnToPool()
        {
            transform.DOKill();
            label.DOKill();
            gameObject.SetActive(false);
            _pool.Push(this);
        }

        void OnDestroy()
        {
            transform.DOKill();
            label.DOKill();
        }
    }
}

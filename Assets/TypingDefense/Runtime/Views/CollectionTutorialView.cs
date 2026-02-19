using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class CollectionTutorialView : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TextMeshProUGUI label;

        CollectionPhaseController _collectionPhase;
        DefenseSaveManager _saveManager;
        bool _waitingForInput;

        [Inject]
        public void Construct(
            CollectionPhaseController collectionPhase,
            DefenseSaveManager saveManager)
        {
            _collectionPhase = collectionPhase;
            _saveManager = saveManager;
            collectionPhase.ShouldHoldFreeze += OnShouldHoldFreeze;
            canvasGroup.alpha = 0f;
        }

        void OnDestroy()
        {
            _collectionPhase.ShouldHoldFreeze -= OnShouldHoldFreeze;
            label.transform.DOKill();
            canvasGroup.DOKill();
        }

        bool OnShouldHoldFreeze()
        {
            if (_saveManager.HasSeenCollectionTutorial) return false;

            _waitingForInput = true;
            canvasGroup.alpha = 0f;
            label.transform.localScale = Vector3.zero;

            var seq = DOTween.Sequence().SetUpdate(true);
            seq.AppendInterval(0.35f);
            seq.Append(label.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            seq.Join(canvasGroup.DOFade(1f, 0.2f));
            seq.Append(label.transform.DOPunchScale(Vector3.one * 0.12f, 0.2f, 6, 0.5f).SetUpdate(true));
            seq.OnComplete(StartBreathing);

            return true;
        }

        void StartBreathing()
        {
            label.transform.DOScale(1.04f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);

            canvasGroup.DOFade(0.7f, 1f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        void Update()
        {
            if (!_waitingForInput) return;
            if (!Input.GetKeyDown(KeyCode.UpArrow)
                && !Input.GetKeyDown(KeyCode.DownArrow)
                && !Input.GetKeyDown(KeyCode.LeftArrow)
                && !Input.GetKeyDown(KeyCode.RightArrow)) return;

            Dismiss();
        }

        void Dismiss()
        {
            _waitingForInput = false;
            _saveManager.MarkCollectionTutorialSeen();

            label.transform.DOKill();
            canvasGroup.DOKill();

            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(label.transform.DOPunchScale(Vector3.one * 0.15f, 0.12f, 10, 0f).SetUpdate(true));
            seq.Append(label.transform.DOScale(1.4f, 0.2f).SetEase(Ease.InBack).SetUpdate(true));
            seq.Join(canvasGroup.DOFade(0f, 0.2f).SetUpdate(true));
            seq.OnComplete(() =>
            {
                canvasGroup.gameObject.SetActive(false);
                _collectionPhase.ReleaseFreeze();
            });
        }
    }
}

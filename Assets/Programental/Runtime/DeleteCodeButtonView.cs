using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Zenject;

namespace Programental
{
    public class DeleteCodeButtonView : MonoBehaviour
    {
        [Inject] private LinesTracker linesTracker;
        [Inject] private BaseMultiplierTracker baseMultiplierTracker;
        [Inject] private AudioPlayer audioPlayer;
        [Inject] private ScreenShaker screenShaker;

        [SerializeField] private Button button;
        [SerializeField] private GameObject background;
        [SerializeField] private GameObject trashIcon;
        [SerializeField] private RectTransform buttonTransform;
        [SerializeField] private string deleteSfxKey = "delete";

        private bool _functional;

        private void Awake()
        {
            background.SetActive(false);
            trashIcon.SetActive(false);
            button.interactable = false;
        }

        private void OnEnable()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(OnClick);
        }

        public void Show(bool animate = true)
        {
            background.SetActive(true);
            button.interactable = true;

            if (!animate) return;

            buttonTransform.localScale = Vector3.zero;
            buttonTransform.localRotation = Quaternion.Euler(0, 0, 15f);

            var seq = DOTween.Sequence();
            seq.Append(buttonTransform.DOScale(1.3f, 0.25f).SetEase(Ease.OutQuad));
            seq.Append(buttonTransform.DOScale(1f, 0.15f).SetEase(Ease.InOutQuad));
            seq.Join(buttonTransform.DORotateQuaternion(Quaternion.identity, 0.15f).SetEase(Ease.OutQuad));
            seq.Append(buttonTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0));
            seq.Join(buttonTransform.DOShakeRotation(0.3f, 10f, 12));
        }

        public void EnableFunctionality(bool animate = true)
        {
            _functional = true;
            trashIcon.SetActive(true);

            if (!animate) return;

            {
                var iconTransform = trashIcon.transform;
                iconTransform.localScale = Vector3.zero;
                iconTransform.localRotation = Quaternion.Euler(0, 0, -20f);

                var seq = DOTween.Sequence();
                seq.Append(iconTransform.DOScale(1.4f, 0.2f).SetEase(Ease.OutQuad));
                seq.Append(iconTransform.DOScale(1f, 0.12f).SetEase(Ease.InOutQuad));
                seq.Join(iconTransform.DORotateQuaternion(Quaternion.identity, 0.12f).SetEase(Ease.OutQuad));
                seq.Append(iconTransform.DOPunchScale(Vector3.one * 0.25f, 0.25f, 10, 0));
                seq.Join(iconTransform.DOShakeRotation(0.25f, 12f, 14));
                seq.Join(buttonTransform.DOPunchScale(Vector3.one * 0.1f, 0.25f, 6, 0));
            }
        }

        private void OnClick()
        {
            buttonTransform.DOComplete();
            buttonTransform.DOPunchScale(Vector3.one * -0.15f, 0.2f, 5, 0);

            if (!_functional) return;
            if (linesTracker.AvailableLines <= 0) return;

            var deleted = linesTracker.DeleteAllLines();
            baseMultiplierTracker.AddDeletedLines(deleted);

            audioPlayer.PlaySfx(deleteSfxKey);
            screenShaker.Shake(0.5f, 0.8f, 40f, 25);

            buttonTransform.DOShakeRotation(0.4f, 25f, 20);
            trashIcon.transform.DOComplete();
            trashIcon.transform.DOPunchScale(Vector3.one * -0.4f, 0.3f, 10, 0);
        }
    }
}

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Programental
{
    public class TaskbarView : MonoBehaviour
    {
        [SerializeField] private RectTransform bar;
        [SerializeField] private RectTransform deleteButton;
        [SerializeField] private Button codeTabButton;
        [SerializeField] private Button upgradesTabButton;
        [SerializeField] private TaskbarClock clock;
        [SerializeField] private IDEScreenView ideScreen;
        [SerializeField] private CodeStructuresScreenView structuresScreen;

        private void Awake()
        {
            bar.gameObject.SetActive(false);
            codeTabButton.gameObject.SetActive(false);
            upgradesTabButton.gameObject.SetActive(false);
            clock.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            codeTabButton.onClick.AddListener(SwitchToCode);
            upgradesTabButton.onClick.AddListener(SwitchToUpgrades);
        }

        private void OnDisable()
        {
            codeTabButton.onClick.RemoveListener(SwitchToCode);
            upgradesTabButton.onClick.RemoveListener(SwitchToUpgrades);
        }

        public void ShowBar(bool animate = true)
        {
            bar.gameObject.SetActive(true);

            if (!animate)
            {
                bar.anchoredPosition = new Vector2(bar.anchoredPosition.x, 0);
                MoveDeleteButtonAboveBar();
                return;
            }

            var targetY = 0;
            bar.anchoredPosition = new Vector2(bar.anchoredPosition.x, targetY - bar.rect.height);
            bar.DOAnchorPosY(targetY, 0.4f).SetEase(Ease.OutBack);

            var deleteTargetY = deleteButton.anchoredPosition.y + bar.rect.height;
            deleteButton.DOAnchorPosY(deleteTargetY, 0.4f).SetEase(Ease.OutBack);
        }

        private void MoveDeleteButtonAboveBar()
        {
            var pos = deleteButton.anchoredPosition;
            pos.y += bar.rect.height;
            deleteButton.anchoredPosition = pos;
        }

        public void ShowCodeTab(bool animate = true)
        {
            codeTabButton.gameObject.SetActive(true);

            if (!animate) return;
            codeTabButton.transform.localScale = Vector3.zero;
            codeTabButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        public void ShowClock(bool animate = true)
        {
            clock.gameObject.SetActive(true);
            clock.StartClock();

            if (!animate) return;
            clock.transform.localScale = Vector3.zero;
            clock.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        public void ShowUpgradesTab(bool animate = true)
        {
            upgradesTabButton.gameObject.SetActive(true);

            if (!animate) return;
            upgradesTabButton.transform.localScale = Vector3.zero;
            upgradesTabButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        private void SwitchToCode()
        {
            ideScreen.Show();
            structuresScreen.Hide();
        }

        private void SwitchToUpgrades()
        {
            ideScreen.Hide();
            structuresScreen.Show();
        }
    }
}

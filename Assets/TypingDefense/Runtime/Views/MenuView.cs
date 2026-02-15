using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TypingDefense
{
    public class MenuView : MonoBehaviour
    {
        [SerializeField] GameObject playButton;
        [SerializeField] GameObject upgradeGraphPanel;
        [SerializeField] TextMeshProUGUI coinsLabel;
        [SerializeField] Button playBtn;

        GameFlowController gameFlow;
        LetterTracker letterTracker;
        DefenseSaveManager saveManager;

        [Inject]
        public void Construct(
            GameFlowController gameFlow,
            LetterTracker letterTracker,
            DefenseSaveManager saveManager)
        {
            this.gameFlow = gameFlow;
            this.letterTracker = letterTracker;
            this.saveManager = saveManager;

            gameFlow.OnStateChanged += OnStateChanged;
            letterTracker.OnCoinsChanged += RefreshLabels;
            playBtn.onClick.AddListener(OnPlayClicked);
        }

        void Start()
        {
            OnStateChanged(gameFlow.State);
        }

        void OnDestroy()
        {
            gameFlow.OnStateChanged -= OnStateChanged;
            letterTracker.OnCoinsChanged -= RefreshLabels;
            playBtn.onClick.RemoveListener(OnPlayClicked);
        }

        void OnStateChanged(GameState state)
        {
            if (state == GameState.Menu)
            {
                gameObject.SetActive(true);
                ShowMenu();
                return;
            }

            gameObject.SetActive(false);
        }

        void ShowMenu()
        {
            playButton.SetActive(true);

            var hasCompleted = saveManager.HasCompletedFirstRun;
            upgradeGraphPanel.SetActive(hasCompleted);

            RefreshLabels();

            transform.localScale = Vector3.one * 0.9f;
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        void OnPlayClicked()
        {
            gameFlow.StartRun();
        }

        void RefreshLabels()
        {
            coinsLabel.text = $"Coins: {letterTracker.GetCoins()}";
        }
    }
}

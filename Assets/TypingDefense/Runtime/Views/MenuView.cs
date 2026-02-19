using DG.Tweening;
using TMPro;
using UnityEngine;

using Zenject;

namespace TypingDefense
{
    public class MenuView : MonoBehaviour
    {
        [Header("Always Visible")]
        [SerializeField] TextMeshProUGUI coinsLabel;

        [Header("Content")]
        [SerializeField] GameObject upgradeGraphPanel;

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
        }

        void Start()
        {
            OnStateChanged(gameFlow.State);
        }

        void OnDestroy()
        {
            gameFlow.OnStateChanged -= OnStateChanged;
            letterTracker.OnCoinsChanged -= RefreshLabels;
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
            upgradeGraphPanel.SetActive(saveManager.HasCompletedFirstRun);
            RefreshLabels();

            transform.localScale = Vector3.one * 0.9f;
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        void RefreshLabels()
        {
            coinsLabel.text = $"Coins: {letterTracker.GetCoins()}";
        }
    }
}

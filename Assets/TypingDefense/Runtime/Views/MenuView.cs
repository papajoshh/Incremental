using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TypingDefense
{
    public enum MenuTab { Upgrades, Converter }

    public class MenuView : MonoBehaviour
    {
        [Header("Always Visible")]
        [SerializeField] Button playBtn;
        [SerializeField] TextMeshProUGUI coinsLabel;

        [Header("Tabs")]
        [SerializeField] Button upgradesTabBtn;
        [SerializeField] Button converterTabBtn;
        [SerializeField] GameObject upgradesTabHighlight;
        [SerializeField] GameObject converterTabHighlight;

        [Header("Tab Content")]
        [SerializeField] GameObject upgradeGraphPanel;
        [SerializeField] ConverterView converterView;

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
            gameFlow.OnReturnedFromRun += OnReturnedFromRun;
            letterTracker.OnCoinsChanged += RefreshLabels;
            playBtn.onClick.AddListener(OnPlayClicked);
            upgradesTabBtn.onClick.AddListener(() => ShowTab(MenuTab.Upgrades));
            converterTabBtn.onClick.AddListener(() => ShowTab(MenuTab.Converter));
        }

        void Start()
        {
            OnStateChanged(gameFlow.State);
        }

        void OnDestroy()
        {
            gameFlow.OnStateChanged -= OnStateChanged;
            gameFlow.OnReturnedFromRun -= OnReturnedFromRun;
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

            converterView.Hide();
            gameObject.SetActive(false);
        }

        void OnReturnedFromRun()
        {
            ShowTab(MenuTab.Converter);
        }

        void ShowMenu()
        {
            var hasCompleted = saveManager.HasCompletedFirstRun;
            upgradesTabBtn.gameObject.SetActive(hasCompleted);
            converterTabBtn.gameObject.SetActive(hasCompleted);

            if (hasCompleted)
            {
                ShowTab(MenuTab.Upgrades);
            }
            else
            {
                upgradeGraphPanel.SetActive(false);
                converterView.Hide();
            }

            RefreshLabels();

            transform.localScale = Vector3.one * 0.9f;
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        void ShowTab(MenuTab tab)
        {
            upgradeGraphPanel.SetActive(tab == MenuTab.Upgrades);
            upgradesTabHighlight.SetActive(tab == MenuTab.Upgrades);
            converterTabHighlight.SetActive(tab == MenuTab.Converter);

            if (tab == MenuTab.Converter)
                converterView.Show();
            else
                converterView.Hide();
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

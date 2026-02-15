using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TypingDefense
{
    public class ConverterView : MonoBehaviour
    {
        [SerializeField] Transform blackHolePrefab;
        [SerializeField] Button continueButton;
        [SerializeField] TextMeshProUGUI coinsLabel;
        [SerializeField] TextMeshProUGUI lettersRemainingLabel;

        ConverterManager converterManager;
        PlayerStats playerStats;
        ConverterConfig converterConfig;
        GameFlowController gameFlow;
        LetterTracker letterTracker;
        ConverterLetterView.Factory letterViewFactory;

        readonly List<Transform> blackHoleViews = new();
        readonly Dictionary<ConverterLetter, ConverterLetterView> letterViews = new();

        [Inject]
        public void Construct(
            ConverterManager converterManager,
            PlayerStats playerStats,
            ConverterConfig converterConfig,
            GameFlowController gameFlow,
            LetterTracker letterTracker,
            ConverterLetterView.Factory letterViewFactory)
        {
            this.converterManager = converterManager;
            this.playerStats = playerStats;
            this.converterConfig = converterConfig;
            this.gameFlow = gameFlow;
            this.letterTracker = letterTracker;
            this.letterViewFactory = letterViewFactory;

            gameFlow.OnStateChanged += OnStateChanged;
            converterManager.OnLetterSpawned += OnLetterSpawned;
            converterManager.OnLetterCollected += OnLetterCollected;
            converterManager.OnCoinsEarned += OnCoinsEarned;
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        void Start()
        {
            OnStateChanged(gameFlow.State);
        }

        void OnDestroy()
        {
            gameFlow.OnStateChanged -= OnStateChanged;
            converterManager.OnLetterSpawned -= OnLetterSpawned;
            converterManager.OnLetterCollected -= OnLetterCollected;
            converterManager.OnCoinsEarned -= OnCoinsEarned;
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        void OnStateChanged(GameState state)
        {
            if (state == GameState.Converting)
            {
                gameObject.SetActive(true);
                converterManager.StartConverting();
                SpawnBlackHoleViews();
                RefreshLabels();
                return;
            }

            CleanUp();
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (gameFlow.State != GameState.Converting) return;

            var holes = converterManager.BlackHoles;
            for (var i = 0; i < holes.Count && i < blackHoleViews.Count; i++)
            {
                blackHoleViews[i].position = holes[i].Position;
                var size = converterConfig.sizeLevels[Mathf.Min(playerStats.ConverterSizeLevel, converterConfig.sizeLevels.Length - 1)];
                blackHoleViews[i].localScale = Vector3.one * size;
            }

            foreach (var kvp in letterViews)
                kvp.Value.transform.position = kvp.Key.Position;
        }

        void SpawnBlackHoleViews()
        {
            var holes = converterManager.BlackHoles;
            foreach (var hole in holes)
            {
                var view = Instantiate(blackHolePrefab);
                view.position = hole.Position;
                view.localScale = Vector3.zero;
                var size = converterConfig.sizeLevels[Mathf.Min(playerStats.ConverterSizeLevel, converterConfig.sizeLevels.Length - 1)];
                view.DOScale(Vector3.one * size, 0.4f).SetEase(Ease.OutBack);
                blackHoleViews.Add(view);
            }
        }

        void OnLetterSpawned(ConverterLetter letter)
        {
            var view = letterViewFactory.Create();
            view.Setup(letter.Type, letter.Position);
            letterViews[letter] = view;
        }

        void OnLetterCollected(ConverterLetter letter)
        {
            if (!letterViews.TryGetValue(letter, out var view)) return;

            view.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                .OnComplete(() => Destroy(view.gameObject));

            letterViews.Remove(letter);
            RefreshLabels();
        }

        void OnCoinsEarned(int amount)
        {
            coinsLabel.transform.DOComplete();
            coinsLabel.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 6);
            RefreshLabels();
        }

        void RefreshLabels()
        {
            coinsLabel.text = $"Coins: {letterTracker.GetCoins()}";
            lettersRemainingLabel.text = $"Letters: {converterManager.ActiveLetters.Count}";
        }

        void OnContinueClicked()
        {
            converterManager.FinishConverting();
        }

        void CleanUp()
        {
            foreach (var view in blackHoleViews)
            {
                if (view != null) Destroy(view.gameObject);
            }
            blackHoleViews.Clear();

            foreach (var kvp in letterViews)
            {
                if (kvp.Value != null) Destroy(kvp.Value.gameObject);
            }
            letterViews.Clear();
        }
    }
}

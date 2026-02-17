using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class ConverterView : MonoBehaviour
    {
        [SerializeField] Transform blackHolePrefab;
        [SerializeField] TextMeshProUGUI coinsLabel;
        [SerializeField] TextMeshProUGUI lettersRemainingLabel;

        static readonly int StrengthId = Shader.PropertyToID("_Strength");

        ConverterManager converterManager;
        PlayerStats playerStats;
        LetterTracker letterTracker;
        ConverterLetterView.Factory letterViewFactory;
        GameFlowController gameFlow;

        readonly List<Transform> blackHoleViews = new();
        readonly List<Material> blackHoleMaterials = new();
        readonly Dictionary<ConverterLetter, ConverterLetterView> letterViews = new();

        [Inject]
        public void Construct(
            ConverterManager converterManager,
            PlayerStats playerStats,
            LetterTracker letterTracker,
            ConverterLetterView.Factory letterViewFactory,
            GameFlowController gameFlow)
        {
            this.converterManager = converterManager;
            this.playerStats = playerStats;
            this.letterTracker = letterTracker;
            this.letterViewFactory = letterViewFactory;
            this.gameFlow = gameFlow;

            converterManager.OnLetterSpawned += OnLetterSpawned;
            converterManager.OnLetterCollected += OnLetterCollected;
            converterManager.OnCoinsEarned += OnCoinsEarned;
            converterManager.OnConvertingFinished += OnConvertingFinished;
            gameFlow.OnStateChanged += OnStateChanged;
        }

        void OnDestroy()
        {
            converterManager.OnLetterSpawned -= OnLetterSpawned;
            converterManager.OnLetterCollected -= OnLetterCollected;
            converterManager.OnCoinsEarned -= OnCoinsEarned;
            converterManager.OnConvertingFinished -= OnConvertingFinished;
            gameFlow.OnStateChanged -= OnStateChanged;
        }

        void Update()
        {
            if (!converterManager.IsConverting) return;
            if (!gameObject.activeSelf) return;

            var holes = converterManager.BlackHoles;
            for (var i = 0; i < holes.Count && i < blackHoleViews.Count; i++)
            {
                blackHoleViews[i].position = holes[i].Position;
                blackHoleViews[i].localScale = Vector3.one * playerStats.ConverterSize;
            }

            foreach (var kvp in letterViews)
                kvp.Value.transform.position = kvp.Key.Position;
        }

        public void Show()
        {
            gameObject.SetActive(true);

            if (converterManager.IsConverting)
            {
                SetWorldObjectsVisible(true);
                RefreshLabels();
                return;
            }

            converterManager.StartConverting();
            SpawnBlackHoleViews();
            RefreshLabels();
        }

        public void Hide()
        {
            SetWorldObjectsVisible(false);
            gameObject.SetActive(false);
        }

        void SetWorldObjectsVisible(bool visible)
        {
            foreach (var hole in blackHoleViews)
            {
                if (hole != null) hole.gameObject.SetActive(visible);
            }

            foreach (var kvp in letterViews)
            {
                if (kvp.Value != null) kvp.Value.gameObject.SetActive(visible);
            }
        }

        void OnStateChanged(GameState state)
        {
            if (state != GameState.Playing) return;

            if (converterManager.IsConverting)
                converterManager.FinishConverting();

            CleanUp();
            gameObject.SetActive(false);
        }

        void OnConvertingFinished()
        {
            CleanUp();
        }

        void SpawnBlackHoleViews()
        {
            var holes = converterManager.BlackHoles;
            foreach (var hole in holes)
            {
                var view = Instantiate(blackHolePrefab);
                view.position = hole.Position;
                view.localScale = Vector3.zero;
                view.DOScale(Vector3.one * playerStats.ConverterSize, 0.4f).SetEase(Ease.OutBack);
                blackHoleViews.Add(view);

                var spriteRenderer = view.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null) continue;

                var mat = spriteRenderer.material;
                var targetStrength = mat.GetFloat(StrengthId);
                mat.SetFloat(StrengthId, 0f);
                mat.DOFloat(targetStrength, StrengthId, 0.4f).SetEase(Ease.OutCubic);
                blackHoleMaterials.Add(mat);
            }
        }

        void OnLetterSpawned(ConverterLetter letter)
        {
            if (!gameObject.activeSelf) return;

            var view = letterViewFactory.Create();
            view.Setup(letter.Type, letter.Position);
            letterViews[letter] = view;
        }

        void OnLetterCollected(ConverterLetter letter)
        {
            if (!letterViews.TryGetValue(letter, out var view)) return;

            var nearest = FindNearestBlackHole(view.transform.position);
            if (nearest != null)
            {
                var seq = DOTween.Sequence();
                seq.Append(view.transform.DOMove(nearest.position, 0.25f).SetEase(Ease.InQuad));
                seq.Join(view.transform.DOScale(0f, 0.25f).SetEase(Ease.InBack));
                seq.Join(view.transform.DORotate(new Vector3(0, 0, 360), 0.25f, RotateMode.FastBeyond360));
                seq.OnComplete(() => Destroy(view.gameObject));
            }
            else
            {
                view.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                    .OnComplete(() => Destroy(view.gameObject));
            }

            letterViews.Remove(letter);
            RefreshLabels();
        }

        Transform FindNearestBlackHole(Vector3 pos)
        {
            Transform nearest = null;
            var minDist = float.MaxValue;
            foreach (var hole in blackHoleViews)
            {
                var dist = Vector3.Distance(pos, hole.position);
                if (dist >= minDist) continue;
                minDist = dist;
                nearest = hole;
            }
            return nearest;
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

        void CleanUp()
        {
            for (var i = 0; i < blackHoleViews.Count; i++)
            {
                var view = blackHoleViews[i];
                if (view == null) continue;

                var mat = i < blackHoleMaterials.Count ? blackHoleMaterials[i] : null;

                view.DOScale(0f, 0.3f).SetEase(Ease.InBack);
                if (mat != null)
                {
                    mat.DOFloat(0f, StrengthId, 0.3f).SetEase(Ease.InCubic)
                        .OnComplete(() =>
                        {
                            if (mat != null) Destroy(mat);
                            if (view != null) Destroy(view.gameObject);
                        });
                }
                else
                {
                    DOVirtual.DelayedCall(0.3f, () =>
                    {
                        if (view != null) Destroy(view.gameObject);
                    });
                }
            }
            blackHoleViews.Clear();
            blackHoleMaterials.Clear();

            foreach (var kvp in letterViews)
            {
                if (kvp.Value != null) Destroy(kvp.Value.gameObject);
            }
            letterViews.Clear();
        }
    }
}

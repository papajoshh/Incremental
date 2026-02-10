using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Programental
{
    public class CodeStructuresScreenView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private StructureSlotUI[] slots;

        [Inject] private CodeStructuresTracker _tracker;
        [Inject] private LinesTracker _linesTracker;

        private bool _shown;

        private void Awake()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            for (var i = 0; i < slots.Length; i++)
            {
                slots[i].root.SetActive(false);
                var index = i;
                slots[i].buyButton.onClick.AddListener(() => OnBuyClicked(index));
            }
        }

        private void OnEnable()
        {
            _tracker.OnStructureChanged += HandleStructureChanged;
            _linesTracker.OnAvailableLinesChanged += HandleCurrencyChanged;
        }

        private void OnDisable()
        {
            _tracker.OnStructureChanged -= HandleStructureChanged;
            _linesTracker.OnAvailableLinesChanged -= HandleCurrencyChanged;
        }

        public void Show(bool animate = true)
        {
            _shown = true;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (animate)
            {
                canvasGroup.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
                transform.localScale = Vector3.one * 0.8f;
                transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            }
            else
            {
                canvasGroup.alpha = 1f;
                transform.localScale = Vector3.one;
            }

            RefreshAll();
            CheckReveals();
        }

        private void HandleStructureChanged(int index)
        {
            if (!_shown) return;
            RefreshAll();
            CheckReveals();
            slots[index].root.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 8, 0);
        }

        private void HandleCurrencyChanged(int _)
        {
            if (!_shown) return;
            RefreshAll();
            CheckReveals();
        }

        private void CheckReveals()
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i].root.activeSelf) continue;
                if (!_tracker.CanAfford(i)) continue;
                RevealSlot(i);
            }
        }

        private void RevealSlot(int index)
        {
            var slot = slots[index];
            slot.root.SetActive(true);
            slot.root.transform.localScale = Vector3.zero;
            slot.root.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            UpdateSlot(index);
        }

        private void RefreshAll()
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (!_tracker.IsRevealed(i) && !slots[i].root.activeSelf) continue;
                if (!slots[i].root.activeSelf)
                {
                    slots[i].root.SetActive(true);
                    slots[i].root.transform.localScale = Vector3.one;
                }
                UpdateSlot(i);
            }
        }

        private void UpdateSlot(int index)
        {
            var slot = slots[index];
            slot.nameText.text = _tracker.GetDisplayName(index);
            slot.levelText.text = $"Lv {_tracker.GetLevel(index)}";
            slot.costText.text = $"{_tracker.GetNextCost(index)}";
            slot.currencyText.text = $"{_tracker.GetCurrency(index)}";
            slot.buyButton.interactable = _tracker.CanAfford(index);
        }

        private void OnBuyClicked(int index)
        {
            _tracker.TryPurchase(index);
        }

        [Serializable]
        public class StructureSlotUI
        {
            public GameObject root;
            public TMP_Text nameText;
            public TMP_Text levelText;
            public TMP_Text costText;
            public TMP_Text currencyText;
            public Button buyButton;
        }
    }
}

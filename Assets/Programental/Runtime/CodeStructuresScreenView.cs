using DG.Tweening;
using I2.Loc;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class CodeStructuresScreenView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private StructureSlotUI[] slots;
        [SerializeField] private string linesLocalizationKey = "CodeStructures/Lines";

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
                slots[i].Hide();
                slots[i].SetConvertVisible(false);

                if (i == slots.Length - 1) continue;

                var structIndex = i;
                slots[i].OnConvertClicked += () => _tracker.TryPurchase(structIndex);
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

            if (!slots[0].gameObject.activeSelf) slots[0].Reveal(animate);

            RefreshAll();
            CheckReveals();
        }

        private void HandleStructureChanged(int structIndex)
        {
            if (!_shown) return;
            RefreshAll();
            CheckReveals();
            slots[structIndex + 1].PunchScale();
        }

        public void Hide()
        {
            _shown = false;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, 0.2f);
        }

        private void HandleCurrencyChanged(int _)
        {
            if (!_shown) return;
            RefreshAll();
            CheckReveals();
        }

        private void CheckReveals()
        {
            for (var i = 0; i < _tracker.StructureCount; i++)
            {
                var slotIndex = i + 1;
                if (slots[slotIndex].gameObject.activeSelf) continue;
                if (!_tracker.IsRevealed(i)) continue;
                slots[slotIndex].Reveal(true);
                UpdateSlot(slotIndex);
            }
        }

        private void RefreshAll()
        {
            UpdateSlot(0);

            for (var i = 0; i < _tracker.StructureCount; i++)
            {
                var slotIndex = i + 1;
                if (!_tracker.IsRevealed(i) && !slots[slotIndex].gameObject.activeSelf) continue;

                if (!slots[slotIndex].gameObject.activeSelf) slots[slotIndex].Reveal(false);
                UpdateSlot(slotIndex);
            }
        }

        private void UpdateSlot(int slotIndex)
        {
            if (slotIndex == 0)
            {
                var name = LocalizationManager.GetTranslation(linesLocalizationKey);
                var count = _linesTracker.AvailableLines;
                var cost = _tracker.GetNextCost(0);
                var canConvert = _tracker.CanAfford(0);
                slots[0].UpdateData(name, count, cost, canConvert);
                slots[0].SetConvertVisible(canConvert || _tracker.IsRevealed(0));
                return;
            }

            var structIndex = slotIndex - 1;
            var isLast = slotIndex == slots.Length - 1;

            var structName = _tracker.GetDisplayName(structIndex);
            var structCount = _tracker.GetAvailable(structIndex);
            var convertCost = isLast ? 0 : _tracker.GetNextCost(structIndex + 1);
            var canAfford = !isLast && _tracker.CanAfford(structIndex + 1);
            var ability = FormatAbility(structIndex);

            slots[slotIndex].UpdateData(structName, structCount, convertCost, canAfford, ability);

            if (!isLast)
                slots[slotIndex].SetConvertVisible(canAfford || _tracker.IsRevealed(structIndex + 1));
        }

        private string FormatAbility(int structIndex)
        {
            var abilityId = _tracker.GetAbilityId(structIndex);
            if (string.IsNullOrEmpty(abilityId)) return null;

            var level = _tracker.GetAbilityEffectiveLevel(structIndex);

            switch (abilityId)
            {
                case "auto_type":
                    return string.Format(LocalizationManager.GetTranslation("CodeStructures/AutoType"), level);
                case "multi_key":
                    return string.Format(LocalizationManager.GetTranslation("CodeStructures/MultiKey"), 1 + level);
                case "clone_lines":
                    return string.Format(LocalizationManager.GetTranslation("CodeStructures/CloneLines"), level);
                default:
                    return null;
            }
        }
    }
}

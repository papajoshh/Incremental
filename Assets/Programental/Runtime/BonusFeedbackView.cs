using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class BonusFeedbackView : MonoBehaviour, IBonusFeedback
    {
        [Inject] private DiContainer container;

        [SerializeField] private BonusFeedbackItem itemPrefab;
        [SerializeField] private RectTransform itemsParent;

        private readonly Dictionary<string, BonusFeedbackItem> _activeItems = new();

        public void ShowBonus(BonusInfo info)
        {
            var item = container.InstantiatePrefabForComponent<BonusFeedbackItem>(
                itemPrefab, itemsParent);
            item.Play(info);
            _activeItems[info.BonusId] = item;
            item.OnExpired += HandleItemExpired;
        }

        public void ExtendBonus(string bonusId, float addedDuration)
        {
            if (!_activeItems.TryGetValue(bonusId, out var item)) return;
            item.Extend(addedDuration);
        }

        private void HandleItemExpired(BonusFeedbackItem item)
        {
            _activeItems.Remove(item.BonusId);
        }
    }
}

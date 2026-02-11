using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Programental
{
    public class StructureSlotUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text abilityText;
        [SerializeField] private Button convertButton;

        public event Action OnConvertClicked;

        private void Awake()
        {
            convertButton.onClick.AddListener(() => OnConvertClicked?.Invoke());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Reveal(bool animate)
        {
            gameObject.SetActive(true);

            if (!animate)
            {
                transform.localScale = Vector3.one;
                return;
            }

            transform.localScale = Vector3.zero;
            transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }

        public void UpdateData(string name, int count, int cost, bool canConvert, string ability = null)
        {
            nameText.text = name;
            countText.text = $"{count}";
            costText.text = $"{cost}";
            convertButton.interactable = canConvert;

            var hasAbility = !string.IsNullOrEmpty(ability);
            abilityText.gameObject.SetActive(hasAbility);
            if (hasAbility) abilityText.text = ability;
        }

        public void SetConvertVisible(bool visible)
        {
            convertButton.gameObject.SetActive(visible);
            costText.gameObject.SetActive(visible);
        }

        public void PunchScale()
        {
            transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 8, 0);
        }
    }
}

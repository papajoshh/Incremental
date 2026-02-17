using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TypingDefense
{
    public class UpgradeNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] Image iconImage;
        [SerializeField] Image borderImage;
        [SerializeField] Image glowImage;

        [Header("State Colors — Border")]
        [SerializeField] Color colorBorderMax = new(1f, 0.84f, 0f, 1f);
        [SerializeField] Color colorBorderAvailable = new(0.4f, 0.75f, 1f, 1f);
        [SerializeField] Color colorBorderLocked = new(0.3f, 0.3f, 0.3f, 1f);

        [Header("State Colors — Icon")]
        [SerializeField] Color colorIconFull = Color.white;
        [SerializeField] Color colorIconDimmed = new(0.4f, 0.4f, 0.4f, 0.7f);

        [Header("State Colors — Glow")]
        [SerializeField] Color colorGlowMax = new(1f, 0.84f, 0f, 0.4f);
        [SerializeField] Color colorGlowOff = new(1f, 1f, 1f, 0f);

        string _nodeId;
        bool _interactable;
        Color _currentBorderColor;
        Action<UpgradeNodeView> _onHoverEnter;
        Action<UpgradeNodeView> _onHoverExit;
        Action<string> _onClicked;

        public string NodeId => _nodeId;

        public void Initialize(
            string nodeId,
            Sprite icon,
            Vector2 anchoredPosition,
            Action<UpgradeNodeView> onHoverEnter,
            Action<UpgradeNodeView> onHoverExit,
            Action<string> onClicked)
        {
            _nodeId = nodeId;
            _onHoverEnter = onHoverEnter;
            _onHoverExit = onHoverExit;
            _onClicked = onClicked;

            iconImage.sprite = icon;
            glowImage.color = colorGlowOff;

            var rect = (RectTransform)transform;
            rect.anchoredPosition = anchoredPosition;
        }

        public void UpdateVisualState(int level, int maxLevel, bool canAfford)
        {
            var isMaxLevel = level >= maxLevel;

            if (isMaxLevel)
            {
                SetColors(colorBorderMax, colorIconFull, colorGlowMax);
                _interactable = false;
                return;
            }

            if (canAfford)
            {
                SetColors(colorBorderAvailable, colorIconFull, colorGlowOff);
                _interactable = true;
                return;
            }

            SetColors(colorBorderLocked, colorIconDimmed, colorGlowOff);
            _interactable = false;
        }

        void SetColors(Color border, Color icon, Color glow)
        {
            _currentBorderColor = border;
            borderImage.color = border;
            iconImage.color = icon;
            glowImage.color = glow;
        }

        public void PlayPurchaseJuice()
        {
            var rect = (RectTransform)transform;
            rect.DOComplete();
            rect.DOPunchScale(Vector3.one * 0.35f, 0.3f, 10);

            iconImage.DOComplete();
            iconImage.DOColor(Color.white, 0.05f)
                .OnComplete(() => iconImage.DOColor(colorIconFull, 0.2f));

            var targetColor = _currentBorderColor;
            borderImage.DOComplete();
            borderImage.DOColor(Color.white, 0.08f)
                .OnComplete(() => borderImage.DOColor(targetColor, 0.25f));
        }

        public void PlayEntranceAnimation(float delay)
        {
            var rect = (RectTransform)transform;
            rect.localScale = Vector3.zero;
            rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(delay);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOComplete();
            transform.DOScale(1.15f, 0.1f).SetEase(Ease.OutQuad);
            _onHoverEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOComplete();
            transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
            _onHoverExit(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_interactable) return;
            _onClicked(_nodeId);
        }
    }
}

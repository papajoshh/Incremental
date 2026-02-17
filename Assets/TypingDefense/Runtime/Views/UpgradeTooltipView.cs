using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TypingDefense
{
    public class UpgradeTooltipView : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform panel;
        [SerializeField] TextMeshProUGUI titleLabel;
        [SerializeField] TextMeshProUGUI descriptionLabel;
        [SerializeField] TextMeshProUGUI levelLabel;
        [SerializeField] TextMeshProUGUI costLabel;
        [SerializeField] Vector2 offset = new(80f, 0f);

        [Header("Cost Colors")]
        [SerializeField] Color colorCostAffordable = new(0.7f, 1f, 0.7f, 1f);
        [SerializeField] Color colorCostTooExpensive = new(1f, 0.4f, 0.4f, 1f);
        [SerializeField] Color colorCostMaxed = new(1f, 0.84f, 0f, 1f);

        RectTransform _canvasRect;
        Tween _showTween;
        Tween _hideTween;
        bool _isVisible;
        string _currentNodeId;

        void Awake()
        {
            _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            panel.localScale = Vector3.one * 0.8f;
            gameObject.SetActive(false);
        }

        public void Show(UpgradeNode node, int level, bool canAfford, Vector2 screenPosition)
        {
            _currentNodeId = node.nodeId;
            gameObject.SetActive(true);

            PopulateContent(node, level, canAfford);
            PositionTooltip(screenPosition);
            PlayShowAnimation();
        }

        public void Refresh(UpgradeNode node, int level, bool canAfford)
        {
            if (!_isVisible) return;
            if (_currentNodeId != node.nodeId) return;
            PopulateContent(node, level, canAfford);
        }

        public void Hide()
        {
            if (!_isVisible) return;
            _isVisible = false;
            _currentNodeId = null;

            _showTween?.Kill();
            _hideTween?.Kill();

            _hideTween = DOTween.Sequence()
                .Append(canvasGroup.DOFade(0f, 0.12f))
                .Join(panel.DOScale(0.8f, 0.12f).SetEase(Ease.InBack))
                .OnComplete(() => gameObject.SetActive(false));
        }

        public bool IsShowingNode(string nodeId) => _isVisible && _currentNodeId == nodeId;

        void PopulateContent(UpgradeNode node, int level, bool canAfford)
        {
            var isMaxLevel = level >= node.maxLevel;

            titleLabel.text = node.displayName;
            levelLabel.text = $"Level {level}/{node.maxLevel}";

            if (isMaxLevel)
            {
                descriptionLabel.text = $"{node.description}\n<size=80%>Value: {node.valuesPerLevel[level - 1]}</size>";
                costLabel.text = "MAX";
                costLabel.color = colorCostMaxed;
                return;
            }

            descriptionLabel.text = level > 0
                ? $"{node.description}\n<size=80%>Current: {node.valuesPerLevel[level - 1]}\nNext: {node.valuesPerLevel[level]}</size>"
                : $"{node.description}\n<size=80%>Next: {node.valuesPerLevel[level]}</size>";

            var cost = node.costsPerLevel[level];
            costLabel.text = $"{cost} coins";
            costLabel.color = canAfford ? colorCostAffordable : colorCostTooExpensive;
        }

        void PositionTooltip(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPosition, null, out var localPoint);

            panel.anchoredPosition = localPoint + offset;
            ClampToScreen();
        }

        void ClampToScreen()
        {
            var canvasSize = _canvasRect.rect.size;
            var halfCanvas = canvasSize * 0.5f;
            var panelSize = panel.rect.size;
            var pos = panel.anchoredPosition;
            var pivot = panel.pivot;

            var minX = -halfCanvas.x + panelSize.x * pivot.x;
            var maxX = halfCanvas.x - panelSize.x * (1f - pivot.x);
            var minY = -halfCanvas.y + panelSize.y * pivot.y;
            var maxY = halfCanvas.y - panelSize.y * (1f - pivot.y);

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            panel.anchoredPosition = pos;
        }

        void PlayShowAnimation()
        {
            _showTween?.Kill();
            _hideTween?.Kill();
            _isVisible = true;

            canvasGroup.alpha = 0f;
            panel.localScale = Vector3.one * 0.8f;

            _showTween = DOTween.Sequence()
                .Append(canvasGroup.DOFade(1f, 0.15f))
                .Join(panel.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        }
    }
}

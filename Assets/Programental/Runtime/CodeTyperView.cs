using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace Programental
{
    [Serializable]
    public struct Milestone
    {
        public int linesRequired;
        public MilestoneReward reward;
    }

    public class CodeTyperView : MonoBehaviour
    {
        [Header("References")]
        [Inject] private CodeTyper codeTyper;
        [SerializeField] private TextMeshProUGUI codeText;

        [Header("Floating Line")]
        [SerializeField] private float floatUpDistance = 150f;
        [SerializeField] private float floatUpDuration = 0.8f;

        [Header("Milestones")]
        [SerializeField] private Milestone[] milestones;

        private int _nextMilestoneIndex;

        private void Awake()
        {
            codeText.richText = false;
        }

        private void OnEnable()
        {
            codeTyper.OnCharTyped += HandleCharTyped;
            codeTyper.OnLineCompleted += HandleLineCompleted;
        }

        private void OnDisable()
        {
            codeTyper.OnCharTyped -= HandleCharTyped;
            codeTyper.OnLineCompleted -= HandleLineCompleted;
        }

        private void HandleCharTyped(char c, string visibleText)
        {
            codeText.text = visibleText;
        }

        private void HandleLineCompleted(string completedLine, int totalLines)
        {
            SpawnFloatingLine(completedLine);
            CheckMilestones(totalLines);
        }

        private void SpawnFloatingLine(string line)
        {
            var go = Instantiate(codeText.gameObject, codeText.transform.parent);
            var rt = go.GetComponent<RectTransform>();
            var tmp = go.GetComponent<TextMeshProUGUI>();

            rt.anchoredPosition = codeText.rectTransform.anchoredPosition;
            rt.localScale = Vector3.one;
            tmp.text = line;
            tmp.richText = false;

            var targetY = rt.anchoredPosition.y + floatUpDistance;
            rt.DOAnchorPosY(targetY, floatUpDuration).SetEase(Ease.OutQuad);
            tmp.DOFade(0f, floatUpDuration).SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(go));
        }

        private void CheckMilestones(int totalLines)
        {
            while (_nextMilestoneIndex < milestones.Length &&
                   milestones[_nextMilestoneIndex].linesRequired <= totalLines)
            {
                milestones[_nextMilestoneIndex].reward?.Unlock();
                _nextMilestoneIndex++;
            }
        }
    }
}

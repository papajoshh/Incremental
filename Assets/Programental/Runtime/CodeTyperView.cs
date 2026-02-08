using TMPro;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class CodeTyperView : MonoBehaviour
    {
        [Inject] private CodeTyper codeTyper;
        [Inject] private LinesTracker linesTracker;
        [SerializeField] private TextMeshProUGUI codeText;

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
            linesTracker.AddCompletedLine();
        }
    }
}

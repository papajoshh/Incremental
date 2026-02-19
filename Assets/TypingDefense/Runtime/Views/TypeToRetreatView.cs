using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class TypeToRetreatView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI retreatLabel;
        [SerializeField] string retreatLocKey = "Defense/Retreat";

        GameFlowController _gameFlow;
        RunManager _runManager;
        string _retreatText;
        int _matchedCount;
        bool _retreatTriggered;

        [Inject]
        public void Construct(GameFlowController gameFlow, RunManager runManager)
        {
            _gameFlow = gameFlow;
            _runManager = runManager;
            gameFlow.OnStateChanged += OnStateChanged;
        }

        void OnDestroy()
        {
            _gameFlow.OnStateChanged -= OnStateChanged;
        }

        void OnStateChanged(GameState state)
        {
            var visible = state == GameState.Playing;
            gameObject.SetActive(visible);

            if (!visible) return;

            _retreatText = LocalizationManager.GetTranslation(retreatLocKey).ToLower();
            _matchedCount = 0;
            _retreatTriggered = false;
            UpdateLabel();
        }

        void Update()
        {
            if (_gameFlow.State != GameState.Playing) return;
            if (_retreatTriggered) return;

            var input = Input.inputString;
            foreach (var c in input)
                TryMatchRetreat(c);
        }

        void TryMatchRetreat(char c)
        {
            if (_matchedCount >= _retreatText.Length) return;

            if (char.ToLower(c) != _retreatText[_matchedCount])
            {
                _matchedCount = 0;
                UpdateLabel();
                return;
            }

            _matchedCount++;
            UpdateLabel();

            var progress = (float)_matchedCount / _retreatText.Length;
            var punchIntensity = Mathf.Lerp(0.05f, 0.25f, progress);

            retreatLabel.transform.DOComplete();
            retreatLabel.transform.DOPunchScale(Vector3.one * punchIntensity, 0.1f, 10, 0f);

            if (_matchedCount < _retreatText.Length) return;

            _retreatTriggered = true;
            retreatLabel.DOComplete();
            retreatLabel.color = Color.green;
            retreatLabel.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 10, 0f);
            DOVirtual.DelayedCall(0.15f, () => _runManager.Retreat());
        }

        void UpdateLabel()
        {
            var matched = _retreatText.Substring(0, _matchedCount).ToUpper();
            var remaining = _retreatText.Substring(_matchedCount).ToUpper();
            retreatLabel.text = $"<color=#FF6600>{matched}</color><color=#FFFFFF>{remaining}</color>";
        }
    }
}

using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class TypeToSelectView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI levelLabel;
        [SerializeField] TextMeshProUGUI inputFeedbackLabel;
        [SerializeField] string playLocKey = "Defense/Play";

        GameFlowController _gameFlow;
        DefenseSaveManager _saveManager;
        LevelProgressionConfig _levelConfig;

        string _playText;
        int _selectedLevel;
        int _playMatchedCount;
        bool _playTriggered;

        [Inject]
        public void Construct(GameFlowController gameFlow, DefenseSaveManager saveManager, LevelProgressionConfig levelConfig)
        {
            _gameFlow = gameFlow;
            _saveManager = saveManager;
            _levelConfig = levelConfig;
            _playText = LocalizationManager.GetTranslation(playLocKey).ToLower();
            gameFlow.OnStateChanged += OnStateChanged;
        }

        void OnDestroy()
        {
            _gameFlow.OnStateChanged -= OnStateChanged;
        }

        void OnStateChanged(GameState state)
        {
            var visible = state == GameState.Menu;
            gameObject.SetActive(visible);

            if (!visible) return;

            _playText = LocalizationManager.GetTranslation(playLocKey).ToLower();
            _selectedLevel = _saveManager.HighestUnlockedLevel;
            _playMatchedCount = 0;
            _playTriggered = false;
            UpdateDisplay();
        }

        void Update()
        {
            if (_gameFlow.State != GameState.Menu) return;
            if (_playTriggered) return;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                ChangeLevel(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow))
                ChangeLevel(1);

            var input = Input.inputString;
            foreach (var c in input)
                ProcessChar(c);
        }

        void ProcessChar(char c)
        {
            if (c == '<') { ChangeLevel(-1); return; }
            if (c == '>') { ChangeLevel(1); return; }

            TryMatchPlay(c);
        }

        void ChangeLevel(int delta)
        {
            var highest = _saveManager.HighestUnlockedLevel;
            var newLevel = Mathf.Clamp(_selectedLevel + delta, 1, highest);
            if (newLevel == _selectedLevel) return;

            _selectedLevel = newLevel;
            _playMatchedCount = 0;
            UpdateDisplay();

            levelLabel.transform.DOComplete();
            levelLabel.transform.DOPunchScale(Vector3.one * 0.15f, 0.15f, 10, 0f);
        }

        void TryMatchPlay(char c)
        {
            if (_playMatchedCount >= _playText.Length) return;

            if (char.ToLower(c) != _playText[_playMatchedCount])
            {
                _playMatchedCount = 0;
                UpdatePlayFeedback();
                return;
            }

            _playMatchedCount++;
            UpdatePlayFeedback();

            var progress = (float)_playMatchedCount / _playText.Length;
            var punchIntensity = Mathf.Lerp(0.05f, 0.25f, progress);

            inputFeedbackLabel.transform.DOComplete();
            inputFeedbackLabel.transform.DOPunchScale(Vector3.one * punchIntensity, 0.1f, 10, 0f);

            if (_playMatchedCount < _playText.Length) return;

            _playTriggered = true;
            inputFeedbackLabel.DOComplete();
            inputFeedbackLabel.color = Color.green;
            inputFeedbackLabel.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 10, 0f);
            DOVirtual.DelayedCall(0.25f, () => _gameFlow.StartRun(_selectedLevel)).SetUpdate(true);
        }

        void UpdateDisplay()
        {
            var highest = _saveManager.HighestUnlockedLevel;
            var config = _levelConfig.GetLevel(_selectedLevel);
            var defeated = _saveManager.IsBossDefeated(_selectedLevel);

            var levelName = string.IsNullOrEmpty(config.displayName)
                ? $"Level {_selectedLevel}"
                : config.displayName;

            var status = defeated ? " <color=#00FF00>[CLEAR]</color>" : "";

            var canGoLeft = highest > 1 && _selectedLevel > 1;
            var canGoRight = highest > 1 && _selectedLevel < highest;

            var left = canGoLeft ? "<color=#FFD700><</color>  " : "";
            var right = canGoRight ? "  <color=#FFD700>></color>" : "";

            levelLabel.text = $"{left}{levelName}{status}{right}";

            UpdatePlayFeedback();
        }

        void UpdatePlayFeedback()
        {
            var matched = _playText.Substring(0, _playMatchedCount).ToUpper();
            var remaining = _playText.Substring(_playMatchedCount).ToUpper();
            inputFeedbackLabel.color = Color.white;
            inputFeedbackLabel.text = $"<color=#FFD700>{matched}</color><color=#FFFFFF>{remaining}</color>";
        }
    }
}

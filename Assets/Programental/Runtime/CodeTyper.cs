using System;

namespace Programental
{
    public class CodeTyper
    {
        private string _currentLine;
        private int _currentCharIndex;
        private int _linesCompleted;

        public int LinesCompleted => _linesCompleted;
        public event Action<char, string> OnCharTyped;
        public event Action<string, int> OnLineCompleted;

        public void Initialize()
        {
            CodeLinePool.Init();
            LoadNextLine();
        }

        public void TypeNextChar()
        {
            if (_currentCharIndex >= _currentLine.Length) return;

            char c = _currentLine[_currentCharIndex];
            _currentCharIndex++;
            string visibleText = _currentLine.Substring(0, _currentCharIndex);

            OnCharTyped?.Invoke(c, visibleText);

            if (_currentCharIndex >= _currentLine.Length)
            {
                _linesCompleted++;
                OnLineCompleted?.Invoke(_currentLine, _linesCompleted);
                LoadNextLine();
            }
        }

        private void LoadNextLine()
        {
            _currentLine = CodeLinePool.GetNext();
            _currentCharIndex = 0;
            OnCharTyped?.Invoke('\0', "");
        }
    }
}

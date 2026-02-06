using UnityEngine;

namespace Programental
{
    public static class CodeLinePool
    {
        private static string[] _lines;
        private static int _index = -1;
        private static bool _initialized;

        public static void Init()
        {
            if (_initialized) return;

            var textAsset = Resources.Load<TextAsset>("CodeLines");
            if (textAsset == null)
            {
                Debug.LogError("CodeLinePool: CodeLines.txt not found in Resources!");
                _lines = new[] { "Debug.Log(\"Hello World!\");" };
                _initialized = true;
                return;
            }

            _lines = textAsset.text.Split('\n');
            // Filter empty lines
            var filtered = new System.Collections.Generic.List<string>(_lines.Length);
            for (int i = 0; i < _lines.Length; i++)
            {
                var trimmed = _lines[i].Trim();
                if (trimmed.Length > 0)
                    filtered.Add(trimmed);
            }
            _lines = filtered.ToArray();
            Shuffle();
            _initialized = true;

            Debug.Log($"CodeLinePool: Loaded {_lines.Length} lines");
        }

        public static string GetNext()
        {
            if (!_initialized) Init();

            _index++;
            if (_index >= _lines.Length)
            {
                Shuffle();
                _index = 0;
            }
            return _lines[_index];
        }

        private static void Shuffle()
        {
            for (int i = _lines.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_lines[i], _lines[j]) = (_lines[j], _lines[i]);
            }
        }
    }
}

using System;
using UnityEngine;

namespace Programental
{
    public static class CodeLinePool
    {
        private static string[] _lines;
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

            var raw = textAsset.text.Split('\n');
            var filtered = new System.Collections.Generic.List<string>(raw.Length);
            for (int i = 0; i < raw.Length; i++)
            {
                var trimmed = raw[i].Trim();
                if (trimmed.Length > 0)
                    filtered.Add(trimmed);
            }

            _lines = filtered.ToArray();
            Array.Sort(_lines, (a, b) => a.Length.CompareTo(b.Length));
            _initialized = true;

            Debug.Log($"CodeLinePool: Loaded {_lines.Length} lines");
        }

        public static string GetNext(int minLength = 0, int maxLength = int.MaxValue)
        {
            if (!_initialized) Init();

            var lo = FindLowerBound(minLength);
            var hi = FindUpperBound(maxLength);
            if (lo > hi) lo = hi;
            return _lines[UnityEngine.Random.Range(lo, hi + 1)];
        }

        private static int FindLowerBound(int minLength)
        {
            int lo = 0, hi = _lines.Length - 1, result = 0;
            while (lo <= hi)
            {
                var mid = (lo + hi) / 2;
                if (_lines[mid].Length >= minLength)
                {
                    result = mid;
                    hi = mid - 1;
                }
                else
                {
                    lo = mid + 1;
                }
            }
            return result;
        }

        private static int FindUpperBound(int maxLength)
        {
            int lo = 0, hi = _lines.Length - 1, result = 0;
            while (lo <= hi)
            {
                var mid = (lo + hi) / 2;
                if (_lines[mid].Length <= maxLength)
                {
                    result = mid;
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }
            return result;
        }
    }
}

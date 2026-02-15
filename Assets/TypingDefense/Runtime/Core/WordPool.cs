using System;
using System.Collections.Generic;
using UnityEngine;

namespace TypingDefense
{
    public class WordPool
    {
        private static readonly string[] WordListFiles = { "WordLists/words_short", "WordLists/words_medium", "WordLists/words_long" };

        private readonly System.Random _random = new();
        private readonly List<string> _allWords = new();

        public WordPool()
        {
            LoadAllWordLists();
        }

        public string GetRandomWord(int minLength, int maxLength)
        {
            var candidates = new List<string>();

            foreach (var word in _allWords)
            {
                if (word.Length >= minLength && word.Length <= maxLength)
                    candidates.Add(word);
            }

            if (candidates.Count == 0) return _allWords[_random.Next(_allWords.Count)];

            return candidates[_random.Next(candidates.Count)];
        }

        private void LoadAllWordLists()
        {
            foreach (var path in WordListFiles)
            {
                var textAsset = Resources.Load<TextAsset>(path);
                if (textAsset == null) continue;

                var lines = textAsset.text.Split('\n');
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length > 0) _allWords.Add(trimmed);
                }
            }

            if (_allWords.Count > 0) return;

            _allWords.AddRange(new[]
            {
                "var", "int", "for", "new", "get", "set", "bool", "char", "void",
                "public", "static", "return", "string", "struct", "switch",
                "interface", "abstract", "property", "override", "delegate"
            });
        }
    }
}

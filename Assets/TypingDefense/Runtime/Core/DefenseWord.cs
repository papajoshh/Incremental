namespace TypingDefense
{
    public class DefenseWord
    {
        public string Text { get; private set; }
        public int MatchedCount { get; private set; }
        public bool IsCompleted => MatchedCount >= Text.Length;
        public char NextChar => Text[MatchedCount];

        public int MaxHp { get; }
        public int CurrentHp { get; private set; }
        public bool IsDead => CurrentHp <= 0;
        public bool IsBoss { get; }

        public DefenseWord(string text, int maxHp = 1, bool isBoss = false)
        {
            Text = text;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            IsBoss = isBoss;
        }

        public bool TryMatchChar(char c)
        {
            if (IsCompleted) return false;
            if (char.ToLower(c) != char.ToLower(NextChar)) return false;
            MatchedCount++;
            return true;
        }

        public bool TakeDamage(int amount)
        {
            CurrentHp -= amount;
            return CurrentHp <= 0;
        }

        public void ChangeText(string newText)
        {
            Text = newText;
            MatchedCount = 0;
        }

        public void Reset()
        {
            MatchedCount = 0;
        }
    }
}

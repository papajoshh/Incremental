namespace Programental
{
    public class BonusMultipliers
    {
        public float BaseMultiplier { get; set; } = 1f;
        public float TemporaryLineMultiplier { get; set; } = 1f;
        public float TotalLineMultiplier => BaseMultiplier * TemporaryLineMultiplier;

        public int BaseCharsPerKeypress { get; set; } = 1;
        public int BonusCharsPerKeypress { get; set; }
        public int CharsPerKeypress => BaseCharsPerKeypress + BonusCharsPerKeypress;

        public float BonusDurationMultiplier { get; set; } = 1f;

        public int AutoTypeLevel { get; set; }
        public int CloneLineCount { get; set; }
    }
}

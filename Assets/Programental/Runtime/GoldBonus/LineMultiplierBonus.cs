namespace Programental
{
    public class LineMultiplierBonus : IGoldenCodeBonus
    {
        private readonly GoldenCodeConfig _config;
        private readonly BonusMultipliers _multipliers;

        public string BonusId => "LineMultiplier";

        public LineMultiplierBonus(GoldenCodeConfig config, BonusMultipliers multipliers)
        {
            _config = config;
            _multipliers = multipliers;
        }

        public BonusInfo Apply()
        {
            _multipliers.TemporaryLineMultiplier = _config.lineMultiplierValue;
            return new BonusInfo
            {
                BonusId = BonusId,
                LocalizationKey = "Bonus/LineMultiplier",
                Value = _config.lineMultiplierValue,
                Duration = _config.lineMultiplierDuration
            };
        }

        public void Revert()
        {
            _multipliers.TemporaryLineMultiplier = 1f;
        }
    }
}

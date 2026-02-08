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
            _multipliers.LineMultiplier = _config.lineMultiplierValue;
            return new BonusInfo
            {
                BonusId = BonusId,
                LocalizationKey = "Bonus/LineMultiplier",
                Value = _config.lineMultiplierValue,
                Duration = _config.bonusDuration
            };
        }

        public void Revert()
        {
            _multipliers.LineMultiplier = 1;
        }
    }
}

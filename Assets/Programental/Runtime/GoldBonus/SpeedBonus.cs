namespace Programental
{
    public class SpeedBonus : IGoldenCodeBonus
    {
        private readonly GoldenCodeConfig _config;
        private readonly BonusMultipliers _multipliers;

        public string BonusId => "Speed";

        public SpeedBonus(GoldenCodeConfig config, BonusMultipliers multipliers)
        {
            _config = config;
            _multipliers = multipliers;
        }

        public BonusInfo Apply()
        {
            _multipliers.BonusCharsPerKeypress = _config.charsPerKeypressValue;
            return new BonusInfo
            {
                BonusId = BonusId,
                LocalizationKey = "Bonus/Speed",
                Value = _config.charsPerKeypressValue,
                Duration = _config.bonusDuration
            };
        }

        public void Revert()
        {
            _multipliers.BonusCharsPerKeypress = 0;
        }
    }
}

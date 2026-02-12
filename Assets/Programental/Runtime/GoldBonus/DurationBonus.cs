namespace Programental
{
    public class DurationBonus : IGoldenCodeBonus
    {
        private readonly GoldenCodeConfig _config;
        private readonly BonusMultipliers _multipliers;

        public string BonusId => "Duration";

        public DurationBonus(GoldenCodeConfig config, BonusMultipliers multipliers)
        {
            _config = config;
            _multipliers = multipliers;
        }

        public BonusInfo Apply()
        {
            _multipliers.BonusDurationMultiplier = _config.durationMultiplierFactor;
            return new BonusInfo
            {
                BonusId = BonusId,
                LocalizationKey = "Bonus/Duration",
                Value = _config.durationMultiplierFactor,
                Duration = _config.durationBonusDuration
            };
        }

        public void Revert()
        {
            _multipliers.BonusDurationMultiplier = 1f;
        }
    }
}

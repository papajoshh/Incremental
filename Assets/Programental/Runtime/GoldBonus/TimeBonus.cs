namespace Programental
{
    public class TimeBonus : IGoldenCodeBonus
    {
        private readonly GoldenCodeConfig _config;
        private readonly BonusMultipliers _multipliers;

        public string BonusId => "Time";

        public TimeBonus(GoldenCodeConfig config, BonusMultipliers multipliers)
        {
            _config = config;
            _multipliers = multipliers;
        }

        public BonusInfo Apply()
        {
            _multipliers.GoldenCodeTimeBonus = _config.goldenCodeTimeBonus;
            return new BonusInfo
            {
                LocalizationKey = "Bonus/Time",
                Value = _config.goldenCodeTimeBonus,
                Duration = _config.bonusDuration
            };
        }

        public void Revert()
        {
            _multipliers.GoldenCodeTimeBonus = 0f;
        }
    }
}

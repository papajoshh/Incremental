namespace TypingDefense
{
    public class PlayerStats
    {
        readonly UpgradeGraphConfig _config;

        public int MaxHp;
        public float MaxEnergy;
        public float DrainMultiplier;
        public int LettersPerKill;
        public float CritChance;
        public float AutoTypeInterval;
        public int AutoTypeCount;
        public float EnergyPerKill;
        public float[] LetterDropChances = new float[5];
        public bool ShieldProtocol;
        public int PowerUpKillInterval;
        public float PowerUpDurationBonus;
        public bool ComboBreaker;

        public int BaseDamage;
        public int BossBonusDamage;
        public float EnergyPerBossHit;

        public float ConverterSpeed;
        public float ConverterSize;
        public float ConverterAutoMoveRatio;
        public int ConverterExtraHoles;

        public PlayerStats(UpgradeGraphConfig config)
        {
            _config = config;
            ResetToBase();
        }

        public void ResetToBase()
        {
            var b = _config.baseStats;
            MaxHp = b.MaxHp;
            MaxEnergy = b.MaxEnergy;
            DrainMultiplier = b.DrainMultiplier;
            LettersPerKill = b.LettersPerKill;
            CritChance = b.CritChance;
            AutoTypeInterval = b.AutoTypeInterval;
            AutoTypeCount = b.AutoTypeCount;
            EnergyPerKill = b.EnergyPerKill;
            LetterDropChances = new float[5];
            ShieldProtocol = false;
            ComboBreaker = false;
            BaseDamage = b.BaseDamage;
            BossBonusDamage = b.BossBonusDamage;
            EnergyPerBossHit = b.EnergyPerBossHit;
            PowerUpKillInterval = b.PowerUpKillInterval;
            PowerUpDurationBonus = b.PowerUpDurationBonus;
            ConverterSpeed = b.ConverterSpeed;
            ConverterSize = b.ConverterSize;
            ConverterAutoMoveRatio = b.ConverterAutoMoveRatio;
            ConverterExtraHoles = b.ConverterExtraHoles;
        }

        public void ApplyUpgrade(UpgradeId id, float value)
        {
            switch (id)
            {
                case UpgradeId.MaxHp: MaxHp = (int)value; break;
                case UpgradeId.MaxEnergy: MaxEnergy = value; break;
                case UpgradeId.DrainMultiplier: DrainMultiplier = value; break;
                case UpgradeId.LettersPerKill: LettersPerKill = (int)value; break;
                case UpgradeId.CritChance: CritChance = value; break;
                case UpgradeId.AutoTypeInterval:
                    AutoTypeInterval = value;
                    if (AutoTypeCount < 1) AutoTypeCount = 1;
                    break;
                case UpgradeId.AutoTypeCount: AutoTypeCount = (int)value; break;
                case UpgradeId.EnergyPerKill: EnergyPerKill = value; break;
                case UpgradeId.DropChanceB: LetterDropChances[(int)LetterType.B] = value; break;
                case UpgradeId.DropChanceC: LetterDropChances[(int)LetterType.C] = value; break;
                case UpgradeId.DropChanceD: LetterDropChances[(int)LetterType.D] = value; break;
                case UpgradeId.DropChanceE: LetterDropChances[(int)LetterType.E] = value; break;
                case UpgradeId.ShieldProtocol: ShieldProtocol = value >= 1f; break;
                case UpgradeId.PowerUpKillInterval: PowerUpKillInterval = (int)value; break;
                case UpgradeId.PowerUpDurationBonus: PowerUpDurationBonus = value; break;
                case UpgradeId.ComboBreaker: ComboBreaker = value >= 1f; break;
                case UpgradeId.BaseDamage: BaseDamage = (int)value; break;
                case UpgradeId.BossBonusDamage: BossBonusDamage = (int)value; break;
                case UpgradeId.EnergyPerBossHit: EnergyPerBossHit = value; break;
                case UpgradeId.ConverterSpeed: ConverterSpeed = value; break;
                case UpgradeId.ConverterSize: ConverterSize = value; break;
                case UpgradeId.ConverterAutoMove: ConverterAutoMoveRatio = value; break;
                case UpgradeId.ConverterExtraHoles: ConverterExtraHoles = (int)value; break;
            }
        }
    }
}

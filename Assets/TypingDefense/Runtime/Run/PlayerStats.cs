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
        public bool AutoTargetUnlocked;
        public float AutoTargetInterval;
        public int AutoTargetCount;
        public float BlackHoleSizeBonus;
        public float CoinMultiplier;
        public float EnergyPerKill;
        public float[] LetterDropChances = new float[5];
        public bool ShieldProtocol;

        public int BaseDamage;
        public int BossBonusDamage;
        public float EnergyPerBossHit;

        public float CollectionSpeed;
        public float LetterAttraction;
        public float CollectionDuration;
        public int WallRevealLevel;

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
            AutoTargetUnlocked = false;
            AutoTargetInterval = b.AutoTargetInterval;
            AutoTargetCount = b.AutoTargetCount;
            BlackHoleSizeBonus = b.BlackHoleSizeBonus;
            CoinMultiplier = b.CoinMultiplier;
            EnergyPerKill = b.EnergyPerKill;
            LetterDropChances = new float[5];
            ShieldProtocol = false;
            BaseDamage = b.BaseDamage;
            BossBonusDamage = b.BossBonusDamage;
            EnergyPerBossHit = b.EnergyPerBossHit;
            CollectionSpeed = b.CollectionSpeed;
            LetterAttraction = b.LetterAttraction;
            CollectionDuration = b.CollectionDuration;
            WallRevealLevel = 0;
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
                case UpgradeId.AutoTargetUnlock:
                    AutoTargetUnlocked = value >= 1f;
                    if (AutoTargetUnlocked && AutoTargetCount < 1) AutoTargetCount = 1;
                    if (AutoTargetUnlocked && AutoTargetInterval <= 0f) AutoTargetInterval = 1f;
                    break;
                case UpgradeId.AutoTargetSpeed: AutoTargetInterval = value; break;
                case UpgradeId.AutoTargetMulti: AutoTargetCount = (int)value; break;
                case UpgradeId.BlackHoleSize: BlackHoleSizeBonus = value; break;
                case UpgradeId.CoinMultiplier: CoinMultiplier = value; break;
                case UpgradeId.EnergyPerKill: EnergyPerKill = value; break;
                case UpgradeId.ShieldProtocol: ShieldProtocol = value >= 1f; break;
                case UpgradeId.BaseDamage: BaseDamage = (int)value; break;
                case UpgradeId.BossBonusDamage: BossBonusDamage = (int)value; break;
                case UpgradeId.EnergyPerBossHit: EnergyPerBossHit = value; break;
                case UpgradeId.CollectionSpeed: CollectionSpeed = value; break;
                case UpgradeId.LetterAttraction: LetterAttraction = value; break;
                case UpgradeId.CollectionDuration: CollectionDuration = value; break;
                case UpgradeId.WallRevealRing0: WallRevealLevel = UnityEngine.Mathf.Max(WallRevealLevel, 1); break;
                case UpgradeId.WallRevealRing1: WallRevealLevel = UnityEngine.Mathf.Max(WallRevealLevel, 2); break;
            }
        }
    }
}

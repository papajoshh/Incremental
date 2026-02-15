namespace TypingDefense
{
    public class PlayerStats
    {
        public int MaxHp = 1;
        public float MaxEnergy = 5f;
        public float DrainMultiplier = 1f;
        public int LettersPerKill = 1;
        public float CritChance = 0f;
        public float AutoTypeInterval = 0f;
        public int AutoTypeCount = 0;
        public float EnergyPerKill = 0f;
        public float[] LetterDropChances = new float[5];
        public bool ShieldProtocol = false;
        public int PowerUpKillInterval = 10;
        public float PowerUpDurationBonus = 0f;
        public bool ComboBreaker = false;

        // Damage system
        public int BaseDamage = 1;
        public int BossBonusDamage = 0;
        public float EnergyPerBossHit = 0f;

        // Converter stats (applied via graph upgrades)
        public int ConverterSpeedLevel = 0;
        public int ConverterSizeLevel = 0;
        public int ConverterAutoMoveLevel = 0;
        public int ConverterExtraHolesLevel = 0;

        public void ResetToBase()
        {
            MaxHp = 1;
            MaxEnergy = 5f;
            DrainMultiplier = 1f;
            LettersPerKill = 1;
            CritChance = 0f;
            AutoTypeInterval = 0f;
            AutoTypeCount = 0;
            EnergyPerKill = 0f;
            LetterDropChances = new float[5];
            ShieldProtocol = false;
            PowerUpKillInterval = 10;
            PowerUpDurationBonus = 0f;
            ComboBreaker = false;
            BaseDamage = 1;
            BossBonusDamage = 0;
            EnergyPerBossHit = 0f;
            ConverterSpeedLevel = 0;
            ConverterSizeLevel = 0;
            ConverterAutoMoveLevel = 0;
            ConverterExtraHolesLevel = 0;
        }

        public void ApplyUpgrade(UpgradeId id, int level)
        {
            switch (id)
            {
                case UpgradeId.ECO1: LettersPerKill = 2; break;
                case UpgradeId.ECO2: LetterDropChances[(int)LetterType.B] = 0.30f; break;
                case UpgradeId.ECO3: LetterDropChances[(int)LetterType.C] = 0.20f; break;
                case UpgradeId.ECO4: LettersPerKill = 3; break;
                case UpgradeId.ECO5: LetterDropChances[(int)LetterType.D] = 0.15f; break;
                case UpgradeId.ECO6:
                    LetterDropChances[(int)LetterType.B] += 0.10f;
                    LetterDropChances[(int)LetterType.C] += 0.10f;
                    LetterDropChances[(int)LetterType.D] += 0.10f;
                    LetterDropChances[(int)LetterType.E] += 0.10f;
                    break;
                case UpgradeId.ECO7: LetterDropChances[(int)LetterType.E] = 0.10f; break;
                case UpgradeId.ECO8: LettersPerKill = 4; break;

                case UpgradeId.OFF1: AutoTypeInterval = 12f; AutoTypeCount = 1; break;
                case UpgradeId.OFF2: CritChance = 0.12f; break;
                case UpgradeId.OFF3: AutoTypeInterval = 8f; break;
                case UpgradeId.OFF4: CritChance = 0.25f; break;
                case UpgradeId.OFF5: AutoTypeCount = 2; break;
                case UpgradeId.OFF6: CritChance = 0.40f; break;
                case UpgradeId.OFF7: AutoTypeInterval = 5f; break;
                case UpgradeId.OFF8: AutoTypeCount = 3; break;

                case UpgradeId.DEF1: MaxHp = 2; break;
                case UpgradeId.DEF2: MaxHp = 3; break;
                case UpgradeId.DEF3: MaxHp = 4; break;
                case UpgradeId.DEF4: MaxHp = 5; break;
                case UpgradeId.DEF5: ShieldProtocol = true; break;

                case UpgradeId.SUR1: EnergyPerKill = 0.5f; break;
                case UpgradeId.SUR2: DrainMultiplier = 0.85f; break;
                case UpgradeId.SUR3: EnergyPerKill = 1.0f; break;
                case UpgradeId.SUR4: MaxEnergy = 7f; break;
                case UpgradeId.SUR5: EnergyPerKill = 1.5f; break;
                case UpgradeId.SUR6: DrainMultiplier = 0.55f; break;

                case UpgradeId.UTI1: PowerUpKillInterval = 8; break;
                case UpgradeId.UTI2: PowerUpDurationBonus = 3f; break;
                case UpgradeId.UTI3: ComboBreaker = true; break;

                case UpgradeId.DMG: BaseDamage = 1 + level; break;
                case UpgradeId.BDMG: BossBonusDamage = level * 2; break;
                case UpgradeId.BEHP: EnergyPerBossHit = level * 0.5f; break;

                case UpgradeId.CONV_SPEED: ConverterSpeedLevel = level; break;
                case UpgradeId.CONV_SIZE: ConverterSizeLevel = level; break;
                case UpgradeId.CONV_AUTO: ConverterAutoMoveLevel = level; break;
                case UpgradeId.CONV_EXTRA: ConverterExtraHolesLevel = level; break;
            }
        }
    }
}

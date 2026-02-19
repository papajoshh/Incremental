namespace TypingDefense
{
    [System.Serializable]
    public class DefenseSaveData
    {
        public int[] Letters = new int[5];
        public int Coins;
        public UpgradeSaveEntry[] Upgrades = System.Array.Empty<UpgradeSaveEntry>();
        public bool HasCompletedFirstRun;
        public bool HasReachedLevel10;
        public bool HasSeenCollectionTutorial;
        public int PrestigeCurrency;
        public int HighestUnlockedLevel = 1;
        public bool[] DefeatedBossLevels = System.Array.Empty<bool>();
    }

    [System.Serializable]
    public class UpgradeSaveEntry
    {
        public string NodeId;
        public int Level;
    }
}

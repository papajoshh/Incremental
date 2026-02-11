using System;

namespace Programental
{
    [Serializable]
    public class GameSaveData
    {
        public LinesData lines;
        public StructureData[] structures;
        public BaseMultiplierData baseMultiplier;
        public GoldenCodeData goldenCode;
        public MilestoneData milestones;
    }

    [Serializable]
    public class LinesData
    {
        public int totalLinesEver;
        public int totalLinesDeleted;
        public float fractionalAccumulator;
    }

    [Serializable]
    public class StructureData
    {
        public int level;
        public int spentOnNext;
        public bool revealed;
    }

    [Serializable]
    public class BaseMultiplierData
    {
        public int currentLevel;
        public int availableLinesToInvest;
    }

    [Serializable]
    public class GoldenCodeData
    {
        public int wordsCompleted;
    }

    [Serializable]
    public class MilestoneData
    {
        public int nextMilestoneIndex;
    }
}
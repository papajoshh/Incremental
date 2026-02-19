using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class DefenseSaveManager : IInitializable, ITickable
    {
        const string SaveKey = "TypingDefenseSave";
        const float AutoSaveInterval = 5f;

        readonly LetterTracker _letterTracker;
        readonly UpgradeTracker _upgradeTracker;
        readonly RunManager _runManager;

        float _saveTimer = AutoSaveInterval;
        DefenseSaveData _cachedData;

        public DefenseSaveManager(
            LetterTracker letterTracker,
            UpgradeTracker upgradeTracker,
            RunManager runManager)
        {
            _letterTracker = letterTracker;
            _upgradeTracker = upgradeTracker;
            _runManager = runManager;
        }

        public bool HasCompletedFirstRun => _cachedData != null && _cachedData.HasCompletedFirstRun;
        public bool HasReachedLevel10 => _cachedData != null && _cachedData.HasReachedLevel10;
        public bool HasSeenCollectionTutorial => _cachedData != null && _cachedData.HasSeenCollectionTutorial;
        public int HighestUnlockedLevel => _cachedData?.HighestUnlockedLevel ?? 1;

        public void Initialize()
        {
            Load();
        }

        public void Tick()
        {
            _saveTimer -= Time.deltaTime;
            if (_saveTimer > 0f) return;
            Save();
        }

        public void Save()
        {
            _saveTimer = AutoSaveInterval;

            if (_cachedData == null) _cachedData = new DefenseSaveData();

            var letterState = _letterTracker.CaptureState();
            _cachedData.Letters = letterState.Letters;
            _cachedData.Coins = letterState.Coins;
            _cachedData.Upgrades = _upgradeTracker.CaptureState();
            _cachedData.PrestigeCurrency = _runManager.PrestigeCurrency;

            var json = JsonUtility.ToJson(_cachedData);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public void MarkFirstRunCompleted()
        {
            if (_cachedData == null) _cachedData = new DefenseSaveData();
            _cachedData.HasCompletedFirstRun = true;
            Save();
        }

        public void MarkLevel10Reached()
        {
            if (_cachedData == null) _cachedData = new DefenseSaveData();
            _cachedData.HasReachedLevel10 = true;
            Save();
        }

        public void MarkCollectionTutorialSeen()
        {
            if (_cachedData == null) _cachedData = new DefenseSaveData();
            _cachedData.HasSeenCollectionTutorial = true;
            Save();
        }

        public bool IsBossDefeated(int level)
        {
            var index = level - 1;
            if (_cachedData == null) return false;
            if (_cachedData.DefeatedBossLevels.Length <= index) return false;
            return _cachedData.DefeatedBossLevels[index];
        }

        public void MarkBossDefeated(int level, int totalLevels)
        {
            if (_cachedData == null) _cachedData = new DefenseSaveData();

            if (_cachedData.DefeatedBossLevels.Length < totalLevels)
            {
                var newArray = new bool[totalLevels];
                System.Array.Copy(_cachedData.DefeatedBossLevels, newArray, _cachedData.DefeatedBossLevels.Length);
                _cachedData.DefeatedBossLevels = newArray;
            }

            _cachedData.DefeatedBossLevels[level - 1] = true;

            if (level >= _cachedData.HighestUnlockedLevel && level < totalLevels)
                _cachedData.HighestUnlockedLevel = level + 1;

            Save();
        }

        public void ResetBossProgression()
        {
            if (_cachedData == null) return;
            _cachedData.DefeatedBossLevels = System.Array.Empty<bool>();
            Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return;

            var json = PlayerPrefs.GetString(SaveKey);
            _cachedData = JsonUtility.FromJson<DefenseSaveData>(json);

            _letterTracker.RestoreState(_cachedData);

            if (_cachedData.Upgrades != null)
                _upgradeTracker.RestoreState(_cachedData.Upgrades);

            _runManager.RestorePrestigeCurrency(_cachedData.PrestigeCurrency);
        }
    }
}

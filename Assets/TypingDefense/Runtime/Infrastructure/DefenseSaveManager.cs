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

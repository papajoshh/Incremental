using UnityEngine;
using Zenject;

namespace Programental
{
    public class SaveManager : IInitializable, ITickable
    {
        private const string SaveKey = "GameSave";
        private const float AutoSaveInterval = 5f;

        private readonly LinesTracker _lines;
        private readonly MilestoneTracker _milestones;
        private readonly CodeStructuresTracker _structures;
        private readonly BaseMultiplierTracker _baseMultiplier;
        private readonly GoldenCodeManager _goldenCode;
        private float _timeSinceLastSave;

        public SaveManager(
            LinesTracker lines,
            MilestoneTracker milestones,
            CodeStructuresTracker structures,
            BaseMultiplierTracker baseMultiplier,
            GoldenCodeManager goldenCode)
        {
            _lines = lines;
            _milestones = milestones;
            _structures = structures;
            _baseMultiplier = baseMultiplier;
            _goldenCode = goldenCode;
        }

        public void Initialize()
        {
            Load();
        }

        public void Tick()
        {
            _timeSinceLastSave += Time.deltaTime;
            if (_timeSinceLastSave < AutoSaveInterval) return;
            Save();
        }

        public void Save()
        {
            _timeSinceLastSave = 0f;
            var data = new GameSaveData
            {
                lines = _lines.CaptureState(),
                structures = _structures.CaptureState(),
                baseMultiplier = _baseMultiplier.CaptureState(),
                goldenCode = _goldenCode.CaptureState(),
                milestones = _milestones.CaptureState()
            };
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return;
            var json = PlayerPrefs.GetString(SaveKey);
            var data = JsonUtility.FromJson<GameSaveData>(json);

            _lines.RestoreState(data.lines);
            _milestones.RestoreState(data.milestones);
            _structures.RestoreState(data.structures);
            _baseMultiplier.RestoreState(data.baseMultiplier);
            _goldenCode.RestoreState(data.goldenCode);
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
        }
    }
}
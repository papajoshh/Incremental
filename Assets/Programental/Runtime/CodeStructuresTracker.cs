using System;
using UnityEngine;

namespace Programental
{
    public class CodeStructuresTracker
    {
        private readonly CodeStructuresConfig _config;
        private readonly LinesTracker _linesTracker;
        private readonly BonusMultipliers _bonusMultipliers;
        private readonly StructureState[] _states;

        public event Action<int> OnStructureChanged;

        public CodeStructuresTracker(
            CodeStructuresConfig config,
            LinesTracker linesTracker,
            BonusMultipliers bonusMultipliers)
        {
            _config = config;
            _linesTracker = linesTracker;
            _bonusMultipliers = bonusMultipliers;

            _states = new StructureState[config.structures.Length];
            for (var i = 0; i < _states.Length; i++)
            {
                _states[i] = new StructureState();
                LoadState(i);
            }

            ApplyAllAbilities();
        }

        public int StructureCount => _config.structures.Length;
        public string GetDisplayName(int index) => _config.structures[index].displayName;
        public int GetLevel(int index) => _states[index].Level;
        public int GetAvailable(int index) => _states[index].Level - _states[index].SpentOnNext;
        public bool IsRevealed(int index) => _states[index].Revealed;

        public int GetNextCost(int index)
        {
            var def = _config.structures[index];
            return (int)Mathf.Pow(def.costBase, _states[index].Level + 1);
        }

        public int GetCurrency(int index)
        {
            if (index == 0) return _linesTracker.AvailableLines;
            return GetAvailable(index - 1);
        }

        public bool CanAfford(int index) => GetCurrency(index) >= GetNextCost(index);

        public bool TryPurchase(int index)
        {
            var cost = GetNextCost(index);

            if (index == 0)
            {
                if (!_linesTracker.TrySpendLines(cost)) return false;
            }
            else
            {
                if (GetAvailable(index - 1) < cost) return false;
                _states[index - 1].SpentOnNext += cost;
                SaveState(index - 1);
                OnStructureChanged?.Invoke(index - 1);
            }

            _states[index].Level++;
            if (!_states[index].Revealed) _states[index].Revealed = true;
            SaveState(index);
            ApplyAllAbilities();
            OnStructureChanged?.Invoke(index);
            return true;
        }

        private void ApplyAllAbilities()
        {
            for (var i = 0; i < _config.structures.Length; i++)
            {
                var abilityId = _config.structures[i].abilityId;
                if (string.IsNullOrEmpty(abilityId)) continue;

                var effectiveLevel = _config.abilityScalesWithAvailable
                    ? GetAvailable(i)
                    : _states[i].Level;

                switch (abilityId)
                {
                    case "auto_type":
                        _bonusMultipliers.AutoTypeCount = Mathf.Max(0, effectiveLevel);
                        break;
                    case "multi_key":
                        _bonusMultipliers.BaseCharsPerKeypress = 1 + Mathf.Max(0, effectiveLevel);
                        break;
                }
            }
        }

        private void SaveState(int index)
        {
            var id = _config.structures[index].id;
            PlayerPrefs.SetInt($"Structure_{id}_Level", _states[index].Level);
            PlayerPrefs.SetInt($"Structure_{id}_SpentOnNext", _states[index].SpentOnNext);
            PlayerPrefs.SetInt($"Structure_{id}_Revealed", _states[index].Revealed ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadState(int index)
        {
            var id = _config.structures[index].id;
            _states[index].Level = PlayerPrefs.GetInt($"Structure_{id}_Level", 0);
            _states[index].SpentOnNext = PlayerPrefs.GetInt($"Structure_{id}_SpentOnNext", 0);
            _states[index].Revealed = PlayerPrefs.GetInt($"Structure_{id}_Revealed", 0) == 1;
        }

        private class StructureState
        {
            public int Level;
            public int SpentOnNext;
            public bool Revealed;
        }
    }
}

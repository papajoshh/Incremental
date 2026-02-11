using System;
using I2.Loc;
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
                _states[i] = new StructureState();
        }

        public int StructureCount => _config.structures.Length;
        public string GetDisplayName(int index) => LocalizationManager.GetTranslation(_config.structures[index].localizationKey);
        public int GetLevel(int index) => _states[index].Level;
        public int GetAvailable(int index) => _states[index].Level - _states[index].SpentOnNext;
        public bool IsRevealed(int index) => _states[index].Revealed;

        public int GetNextCost(int index)
        {
            var def = _config.structures[index];
            return (int)Mathf.Pow(def.costBase, _states[index].Level + 1) + def.costOffset;
        }

        public int GetCurrency(int index)
        {
            if (index == 0) return _linesTracker.AvailableLines;
            return GetAvailable(index - 1);
        }

        public bool CanAfford(int index) => GetCurrency(index) >= GetNextCost(index);
        public string GetAbilityId(int index) => _config.structures[index].abilityId;

        public int GetAbilityEffectiveLevel(int index)
        {
            var level = _config.abilityScalesWithAvailable ? GetAvailable(index) : _states[index].Level;
            return Mathf.Max(0, level);
        }

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
                OnStructureChanged?.Invoke(index - 1);
            }

            _states[index].Level++;
            if (!_states[index].Revealed) _states[index].Revealed = true;
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
                        _bonusMultipliers.AutoTypeLevel = Mathf.Max(0, effectiveLevel);
                        break;
                    case "multi_key":
                        _bonusMultipliers.BaseCharsPerKeypress = 1 + Mathf.Max(0, effectiveLevel);
                        break;
                    case "clone_lines":
                        _bonusMultipliers.CloneLineCount = Mathf.Max(0, effectiveLevel);
                        break;
                }
            }
        }

        public StructureData[] CaptureState()
        {
            var result = new StructureData[_states.Length];
            for (var i = 0; i < _states.Length; i++)
            {
                result[i] = new StructureData
                {
                    level = _states[i].Level,
                    spentOnNext = _states[i].SpentOnNext,
                    revealed = _states[i].Revealed
                };
            }
            return result;
        }

        public void RestoreState(StructureData[] data)
        {
            for (var i = 0; i < data.Length && i < _states.Length; i++)
            {
                _states[i].Level = data[i].level;
                _states[i].SpentOnNext = data[i].spentOnNext;
                _states[i].Revealed = data[i].revealed;
            }
            ApplyAllAbilities();
        }

        private class StructureState
        {
            public int Level;
            public int SpentOnNext;
            public bool Revealed;
        }
    }
}

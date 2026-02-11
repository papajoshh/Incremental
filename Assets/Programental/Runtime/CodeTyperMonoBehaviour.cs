using System;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class CodeTyperMonoBehaviour : MonoBehaviour
    {
        [Inject] private CodeTyper _codeTyper;
        [Inject] private BonusMultipliers _bonusMultipliers;
        [Inject] private CodeStructuresConfig _structuresConfig;

        public event Action<char> OnKeyPressed;
        private float _autoTypeTimer;

        private void Start()
        {
            _codeTyper.Initialize();
            _autoTypeTimer = _structuresConfig.autoTypeBaseInterval;
        }

        private void Update()
        {
            HandleAutoType();
            HandleManualType();
        }

        private void HandleAutoType()
        {
            if (_bonusMultipliers.AutoTypeLevel <= 0) return;

            _autoTypeTimer -= Time.deltaTime;
            if (_autoTypeTimer > 0f) return;

            for (var i = 0; i < _bonusMultipliers.CharsPerKeypress; i++)
                _codeTyper.TypeNextChar();

            var interval = _structuresConfig.autoTypeBaseInterval
                           - _bonusMultipliers.AutoTypeLevel * _structuresConfig.autoTypeReductionPerLevel;
            _autoTypeTimer = Mathf.Max(interval, _structuresConfig.autoTypeMinInterval);
        }

        private void HandleManualType()
        {
            if (!Input.anyKeyDown) return;
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) return;
            if (Input.GetKeyDown(KeyCode.Escape)) return;

            var charsToType = _bonusMultipliers.CharsPerKeypress;
            for (var i = 0; i < charsToType; i++)
                _codeTyper.TypeNextChar();

            foreach (var c in Input.inputString)
                OnKeyPressed?.Invoke(c);
        }
    }
}

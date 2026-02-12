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
        private float _autoTypeAccumulator;

        private void Start()
        {
            _codeTyper.Initialize();
        }

        private void Update()
        {
            HandleAutoType();
            HandleManualType();
        }

        private void HandleAutoType()
        {
            if (_bonusMultipliers.AutoTypeLevel <= 0) return;

            var keystrokesPerSecond = _bonusMultipliers.AutoTypeLevel * _structuresConfig.autoKeystrokesPerSecPerLevel;
            _autoTypeAccumulator += keystrokesPerSecond * Time.deltaTime;

            while (_autoTypeAccumulator >= 1f)
            {
                for (var i = 0; i < _bonusMultipliers.CharsPerKeypress; i++)
                    _codeTyper.TypeNextChar();
                _autoTypeAccumulator -= 1f;
            }
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

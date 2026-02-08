using System;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class CodeTyperMonoBehaviour : MonoBehaviour
    {
        [Inject] private CodeTyper _codeTyper;
        [Inject] private BonusMultipliers _bonusMultipliers;

        public event Action<char> OnKeyPressed;

        private void Start()
        {
            _codeTyper.Initialize();
        }

        private void Update()
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

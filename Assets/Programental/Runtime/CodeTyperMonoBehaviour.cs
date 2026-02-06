using UnityEngine;
using Zenject;

namespace Programental
{
    public class CodeTyperMonoBehaviour : MonoBehaviour
    {
        [Inject] private CodeTyper _codeTyper;

        private void Start()
        {
            _codeTyper.Initialize();
        }

        private void Update()
        {
            if (!Input.anyKeyDown) return;
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) return;
            if (Input.GetKeyDown(KeyCode.Escape)) return;

            _codeTyper.TypeNextChar();
        }
    }
}

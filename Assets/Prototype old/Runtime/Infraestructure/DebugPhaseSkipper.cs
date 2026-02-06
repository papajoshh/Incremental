using System.Collections.Generic;
using Runtime.Application;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class DebugPhaseSkipper : MonoBehaviour
    {
        [Inject] private readonly SaveManager _saveManager;
        [Inject] private readonly List<ISkippable> _phases;

#if UNITY_EDITOR
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5)) _saveManager.Save();
            if (Input.GetKeyDown(KeyCode.F9)) _saveManager.Load();

            for (int i = 0; i < _phases.Count && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    SkipToPhase(i);
            }
        }

        private void SkipToPhase(int phaseIndex)
        {
            for (int i = 0; i <= phaseIndex; i++)
                _phases[i].Skip();
        }
#endif
    }
}

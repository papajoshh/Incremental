using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class DebugPhaseSkipper : MonoBehaviour
    {
        [Inject] private readonly SaveManager _saveManager;

#if UNITY_EDITOR
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5)) _saveManager.Save();
            if (Input.GetKeyDown(KeyCode.F9)) _saveManager.Load();
        }
#endif
    }
}

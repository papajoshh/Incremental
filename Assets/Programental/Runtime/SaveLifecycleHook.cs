using UnityEngine;
using Zenject;

namespace Programental
{
    public class SaveLifecycleHook : MonoBehaviour
    {
        [Inject] private SaveManager _saveManager;

        private void OnApplicationPause(bool paused)
        {
            if (paused) _saveManager.Save();
        }

        private void OnApplicationQuit()
        {
            _saveManager.Save();
        }
    }
}
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class DefenseSaveLifecycleHook : MonoBehaviour
    {
        DefenseSaveManager saveManager;

        [Inject]
        public void Construct(DefenseSaveManager saveManager)
        {
            this.saveManager = saveManager;
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) saveManager.Save();
        }

        void OnApplicationQuit()
        {
            saveManager.Save();
        }
    }
}

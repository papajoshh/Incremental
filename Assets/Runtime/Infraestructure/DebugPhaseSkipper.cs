using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class DebugPhaseSkipper : MonoBehaviour
    {
        [Inject] private readonly SaveManager _saveManager;
        [Inject] private SalaDeCargaPrincipalMonoBehaviour sala;

#if UNITY_EDITOR
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5)) _saveManager.Save();
            if (Input.GetKeyDown(KeyCode.F9)) _saveManager.Load();
            if (Input.GetKeyDown(KeyCode.Alpha0)) ResetGame();
            if (Input.GetKeyDown(KeyCode.Alpha1)) sala.SkipToStart();
            if (Input.GetKeyDown(KeyCode.Alpha2)) sala.SkipTo2Moñecos();
            if (Input.GetKeyDown(KeyCode.Alpha3)) sala.SkipToAllOccupied();
        }

        private void ResetGame()
        {
            _saveManager.DeleteSave();
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
#endif
    }
}

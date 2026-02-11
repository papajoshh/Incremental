using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Programental
{
    public class QaToolMonoBehaviour : MonoBehaviour
    {
        [Inject] private LinesTracker _linesTracker;
        [Inject] private SaveManager _saveManager;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) AddLines(10);
            if (Input.GetKeyDown(KeyCode.F2)) AddLines(100);
            if (Input.GetKeyDown(KeyCode.F3)) AddLines(1000);
            if (Input.GetKeyDown(KeyCode.F4)) ResetProgress();
        }

        private void AddLines(int count)
        {
            for (var i = 0; i < count; i++)
                _linesTracker.AddCompletedLine();
        }

        private void ResetProgress()
        {
            _saveManager.DeleteSave();
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using Runtime.Application;
using Runtime.Domain;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class SaveManager : MonoBehaviour
    {
        [Inject] private readonly BagOfMoñecos _bag;

        private string SavePath => Path.Combine(UnityEngine.Application.persistentDataPath, "save.json");

        private List<ISaveable> _saveables;

        private void Start()
        {
            _saveables = FindObjectsOfType<MonoBehaviour>()
                .OfType<ISaveable>().ToList();

            ValidateUniqueIds();

            if (HasSave()) Load();
        }

        private void ValidateUniqueIds()
        {
            var seen = new HashSet<string>();
            foreach (var s in _saveables)
            {
                if (!seen.Add(s.SaveId))
                    Debug.LogError($"[SaveManager] SaveId duplicado: \"{s.SaveId}\" en {(s as MonoBehaviour)?.gameObject.name}. El save NO funcionará correctamente.");
                    Debug.Break();
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        public void Save()
        {
            var camera = Camera.main;
            var ids = new List<string>();
            var jsons = new List<string>();

            foreach (var s in _saveables)
            {
                ids.Add(s.SaveId);
                jsons.Add(s.CaptureStateJson());
            }

            var data = new GameSaveData
            {
                version = 1,
                totalMoñecos = _bag.MoñecosInside,
                cameraX = camera.transform.localPosition.x,
                cameraY = camera.transform.localPosition.y,
                cameraZ = camera.transform.localPosition.z,
                cameraSize = camera.orthographicSize,
                saveIds = ids.ToArray(),
                saveJsons = jsons.ToArray(),
            };

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveManager] Saved to {SavePath}");
        }

        public void Load()
        {
            if (!HasSave()) return;

            DOTween.KillAll(false);
            Time.timeScale = 1f;

            var json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<GameSaveData>(json);

            var camera = Camera.main;
            camera.transform.localPosition = new Vector3(data.cameraX, data.cameraY, data.cameraZ);
            camera.orthographicSize = data.cameraSize;

            _bag.RestoreCount(data.totalMoñecos);

            var lookup = new Dictionary<string, string>();
            for (int i = 0; i < data.saveIds.Length; i++)
                lookup[data.saveIds[i]] = data.saveJsons[i];

            foreach (var s in _saveables.OrderBy(s => s.RestoreOrder))
            {
                if (lookup.TryGetValue(s.SaveId, out var j))
                    s.RestoreStateJson(j);
            }

            Debug.Log("[SaveManager] Loaded save");
        }

        public bool HasSave() => File.Exists(SavePath);

        [ContextMenu("Delete Save")]
        public void DeleteSave()
        {
            if (HasSave()) File.Delete(SavePath);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using Runtime.Application;
using Runtime.Domain;
using Runtime.Infrastructure;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class SaveManager : MonoBehaviour
    {
        [Inject] private readonly BagOfMoñecos _bag;
        [Inject] private readonly FirstStickman _firstStickman;
        [Inject] private readonly DiContainer _container;
        [SerializeField] private GameObject moñecoPrefab;

        private string SavePath => Path.Combine(UnityEngine.Application.persistentDataPath, "save.json");

        private void Start()
        {
            if (HasSave()) Load();
        }

        private void OnDisable()
        {
            Save();
        }

        public void Save()
        {
            var camera = Camera.main;
            var machines = FindObjectsOfType<SingleMoñecoCreatingMachineGameObject>();
            var computers = FindObjectsOfType<RepairableComputerGameObject>();
            var doors = FindObjectsOfType<DoorMonoBehaviour>();
            var rooms = FindObjectsOfType<RoomMonoBehaviour>();
            var moñecos = FindObjectsOfType<MoñecoMonoBehaviour>();
            var sala = FindObjectOfType<SalaDeCargaPrincipalMonoBehaviour>();

            var data = new GameSaveData
            {
                version = 1,
                workbenchCompleted = IsWorkbenchCompleted(),
                totalMoñecos = _bag.MoñecosInside,
                cameraX = camera.transform.localPosition.x,
                cameraY = camera.transform.localPosition.y,
                cameraZ = camera.transform.localPosition.z,
                cameraSize = camera.orthographicSize,
                milestone2MoñecosTriggered = sala != null && sala.Milestone2MoñecosTriggered,
                machineOccupiedCount = sala != null ? sala.MachineOccupiedCount : 0,
                moñecos = moñecos.Select(m => m.CaptureState()).ToArray(),
                machines = machines.Select(m => m.CaptureState()).ToArray(),
                computers = computers.Select(c => c.CaptureState()).ToArray(),
                doors = doors.Select(d => d.CaptureState()).ToArray(),
                rooms = rooms.Select(r => r.CaptureState()).ToArray(),
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

            if (data.workbenchCompleted)
            {
                var workbench = FindObjectOfType<StickmanWorkbench>();
                if (workbench) workbench.gameObject.SetActive(false);
            }

            var sala = FindObjectOfType<SalaDeCargaPrincipalMonoBehaviour>();
            if (sala != null)
                sala.RestoreMilestones(data.milestone2MoñecosTriggered, data.machineOccupiedCount);

            var machinesById = FindObjectsOfType<SingleMoñecoCreatingMachineGameObject>()
                .ToDictionary(m => m.SaveId);
            foreach (var md in data.machines)
            {
                if (machinesById.TryGetValue(md.id, out var machine))
                    machine.RestoreState(md);
            }

            var computersById = FindObjectsOfType<RepairableComputerGameObject>()
                .ToDictionary(c => c.SaveId);
            foreach (var cd in data.computers)
            {
                if (computersById.TryGetValue(cd.id, out var computer))
                    computer.RestoreState(cd);
            }

            var doorsById = FindObjectsOfType<DoorMonoBehaviour>()
                .ToDictionary(d => d.SaveId);
            foreach (var dd in data.doors)
            {
                if (doorsById.TryGetValue(dd.id, out var door))
                    door.RestoreState(dd);
            }

            var roomsById = FindObjectsOfType<RoomMonoBehaviour>()
                .ToDictionary(r => r.SaveId);
            foreach (var rd in data.rooms)
            {
                if (roomsById.TryGetValue(rd.id, out var room))
                    room.RestoreState(rd);
            }

            foreach (var existing in FindObjectsOfType<MoñecoMonoBehaviour>())
                Destroy(existing.gameObject);

            var interactablesById = new Dictionary<string, Interactable>();
            foreach (var kvp in machinesById) interactablesById[kvp.Key] = kvp.Value;
            foreach (var kvp in computersById) interactablesById[kvp.Key] = kvp.Value;

            foreach (var md in data.moñecos)
            {
                var go = _container.InstantiatePrefab(moñecoPrefab, new Vector3(md.x, md.y, 0), Quaternion.identity, null);
                var moñeco = go.GetComponent<MoñecoMonoBehaviour>();

                if (md.isInteracting && !string.IsNullOrEmpty(md.assignedMachineId))
                {
                    if (interactablesById.TryGetValue(md.assignedMachineId, out var interactable))
                    {
                        moñeco.RestoreInteraction(interactable, md.direction);

                        if (interactable is SingleMoñecoCreatingMachineGameObject machine)
                            machine.RestoreWorker(moñeco);
                        else if (interactable is RepairableComputerGameObject comp)
                            comp.RestoreWorker(moñeco);
                    }
                    else
                    {
                        moñeco.RestoreWalking(md.direction);
                    }
                }
                else
                {
                    moñeco.RestoreWalking(md.direction);
                }
            }

            Physics2D.SyncTransforms();
            Debug.Log("[SaveManager] Loaded save");
        }

        public bool HasSave() => File.Exists(SavePath);

        public void DeleteSave()
        {
            if (HasSave()) File.Delete(SavePath);
        }

        private bool IsWorkbenchCompleted()
        {
            return _firstStickman.HeadFullfilled
                && _firstStickman.LeftLegFullfilled
                && _firstStickman.RightLegFullfilled;
        }
    }
}

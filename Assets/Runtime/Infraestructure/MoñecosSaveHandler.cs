using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Application;
using Runtime.Domain;
using Runtime.Infrastructure;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class MoñecosSaveHandler : MonoBehaviour, ISaveable
    {
        [Inject] private readonly MoñecoInstantiator _instantiator;

        private readonly List<MoñecoMonoBehaviour> _moñecos = new();

        public string SaveId => "moñecos";
        public int RestoreOrder => 10;

        public void Track(MoñecoMonoBehaviour m) => _moñecos.Add(m);
        public void Untrack(MoñecoMonoBehaviour m) => _moñecos.Remove(m);

        public string CaptureStateJson()
        {
            var data = new MoñecosCollectionSaveData
            {
                moñecos = _moñecos.Select(m => m.CaptureState()).ToArray()
            };
            return JsonUtility.ToJson(data);
        }

        public void RestoreStateJson(string json)
        {
            var data = JsonUtility.FromJson<MoñecosCollectionSaveData>(json);

            foreach (var existing in _moñecos.ToArray())
                Destroy(existing.gameObject);
            _moñecos.Clear();

            var interactables = FindInteractables();

            foreach (var md in data.moñecos)
            {
                var moñeco = _instantiator.Spawn(new Vector3(md.x, md.y, 0));

                if (md.isInteracting && !string.IsNullOrEmpty(md.assignedMachineId)
                    && interactables.TryGetValue(md.assignedMachineId, out var interactable))
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

            Physics2D.SyncTransforms();
        }

        private Dictionary<string, Interactable> FindInteractables()
        {
            var result = new Dictionary<string, Interactable>();
            foreach (var machine in FindObjectsOfType<SingleMoñecoCreatingMachineGameObject>())
                result[machine.SaveId] = machine;
            foreach (var computer in FindObjectsOfType<RepairableComputerGameObject>())
                result[computer.SaveId] = computer;
            return result;
        }

        [Serializable]
        private class MoñecosCollectionSaveData
        {
            public MoñecoSaveData[] moñecos;
        }
    }
}

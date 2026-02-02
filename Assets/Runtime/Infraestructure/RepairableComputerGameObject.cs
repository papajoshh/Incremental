using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Runtime.Application;
using Runtime.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Runtime.Infraestructure
{
    public class RepairableComputerGameObject : MonoBehaviour, Interactable, IPointerDownHandler, ISkippable
    {
        [SerializeField] private int ticksToRepair = 300;
        [SerializeField] private int totalSlots = 3;
        [SerializeField] private Transform[] slotsPositions;
        [SerializeField] private Transform progressMask;
        [SerializeField] private Vector3 progressMaskStart;
        [SerializeField] private Vector3 progressMaskEnd;
        [SerializeField] private DoorMonoBehaviour door;
        [SerializeField] private PressFeedback pressFeedback;
        [SerializeField] private Collider2D interactionCollider;
        [SerializeField] private string saveId;

        [Inject] private readonly MoñecoInstantiator instantiator;
        public string SaveId => saveId;

        private RepairableComputer _computer;
        private Tween _progressTween;

        private void Awake()
        {
            _computer = new RepairableComputer(ticksToRepair, totalSlots);
            _computer.OnRepaired += OnRepaired;
            progressMask.localPosition = progressMaskStart;
        }

        private void OnDestroy()
        {
            _computer.OnRepaired -= OnRepaired;
        }

        public bool CanInteract(Interactor interactor) => !_computer.Repaired && _computer.HasFreeSlot;

        public void StartInteraction(Interactor interactor)
        {
            int slotIndex = _computer.GetWorkers().Count;
            _computer.AddWorker(interactor);
            interactor.SetPositionToInteract(slotsPositions[slotIndex]);
        }

        public Task OnInteractionTick(Interactor interactor)
        {
            if (_computer.Repaired) return Task.CompletedTask;
            _computer.Impulse();
            UpdateProgressBar();
            return Task.CompletedTask;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_computer.Repaired) return;
            if (_computer.GetWorkers().Count == 0) return;
            _computer.Impulse();
            if (pressFeedback) pressFeedback.Play();
            UpdateProgressBar();
        }

        public void EndInteraction(Interactor interactor) { }

        public ComputerSaveData CaptureState()
        {
            return new ComputerSaveData
            {
                id = saveId,
                currentPresses = _computer.CurrentPresses,
                repaired = _computer.Repaired
            };
        }

        public void RestoreState(ComputerSaveData data)
        {
            _computer.Restore(data.currentPresses, data.repaired);
            progressMask.localPosition = Vector3.Lerp(progressMaskStart, progressMaskEnd, _computer.Progress);
            if (data.repaired)
            {
                door.Open();
                interactionCollider.enabled = false;
            }
        }

        public void RestoreWorker(Interactor worker)
        {
            _computer.RestoreWorker(worker);
        }

        private void UpdateProgressBar()
        {
            _progressTween?.Kill();
            _progressTween = progressMask.DOLocalMove(
                Vector3.Lerp(progressMaskStart, progressMaskEnd, _computer.Progress), 0.5f);
        }

        private void OnRepaired()
        {
            foreach (var worker in _computer.GetWorkers())
                worker.StopInteraction();

            door.Open();
            interactionCollider.enabled = false;
        }

        public void Skip()
        {
            for (var i = _computer.GetWorkers().Count; i < totalSlots; i++)
            {
                var moñeco = instantiator.GiveBirth(slotsPositions[i].position).Result;
                moñeco.RestoreInteraction(this, 1);
                _computer.RestoreWorker(moñeco);
            }

            _computer.Restore(ticksToRepair, true);
            progressMask.localPosition = progressMaskEnd;
            door.Open();
            interactionCollider.enabled = false;
        }
    }
}

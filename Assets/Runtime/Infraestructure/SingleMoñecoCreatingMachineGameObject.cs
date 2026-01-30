using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Runtime.Application;
using Runtime.Domain;
using Runtime.Infraestructure;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Runtime.Infrastructure
{
    public class SingleMoñecoCreatingMachineGameObject: MonoBehaviour, Interactable, MoñecoMachine, IPointerDownHandler
    {
        [Serializable]
        public class BodyPartVisual
        {
            public Transform mask;
            public Vector3 endPosition;
            public bool useScale;
            [HideInInspector] public Vector3 initialPosition;
            [HideInInspector] public Tween tween;
        }

        [SerializeField] private int ticksToSpawn = 300;
        [SerializeField] private GameObject _moñecoPrefab;
        [SerializeField] private Transform positionToSpawn;
        [SerializeField] private Transform positionToInteract;
        [SerializeField] private Collider2D collider;
        [SerializeField] private bool startEnabled = true;
        [SerializeField] private bool _canBeInteracted = true;
        [SerializeField] private GameObject visualMoñeco;
        
        [Header("Body Parts")]
        [SerializeField] private BodyPartVisual[] bodyParts;
        [SerializeField] private PressFeedback _pressFeedback;
        [SerializeField] private string saveId;
        public string SaveId => saveId;
        public Vector3 InteractPosition => positionToInteract.position;

        private Interactor _currentUser;
        
        public MoñecoCreatingMachine CurrentMachine { get; private set; }

        private void Awake()
        {
            foreach (var part in bodyParts)
                part.initialPosition = part.mask.localPosition;
            if(startEnabled)
                TurnOn();
            else
                TurnOff();
        }

        [Inject]
        private void Construct(BagOfMoñecos bag)
        {
            CurrentMachine = new MoñecoCreatingMachine(bag, ticksToSpawn, this, new List<Interactor>());
        }

        public bool CanInteract(Interactor interactor) => _currentUser == null && _canBeInteracted;

        public void StartInteraction(Interactor interactor)
        {
            _currentUser = interactor;
            interactor.SetPositionToInteract(positionToInteract);
            CurrentMachine.AddWorkerMoñeco(interactor);
        }

        public async Task OnInteractionTick(Interactor interactor)
        {
            if (!_canBeInteracted) return;
            await CurrentMachine.ImpulseMoñecoCreation();
            UpdateVisuals();
        }

        public async void OnPointerDown(PointerEventData eventData)
        {
            if (!_canBeInteracted) return;
            if (_currentUser == null) return;
            await CurrentMachine.ImpulseMoñecoCreation();
            if (_pressFeedback) _pressFeedback.Play();
            UpdateVisuals();
        }

        public void EndInteraction(Interactor interactor)
        {
            if (_currentUser != interactor) return;
            _currentUser = null;
        }

        public async Task GiveBirth()
        {
            var moñeco = Instantiate(_moñecoPrefab, positionToSpawn.position, Quaternion.identity);
            _canBeInteracted = false;
            await moñeco.GetComponent<MoñecoMonoBehaviour>().Birth();
            ResetVisuals();
            _canBeInteracted = true;
        }

        public void TurnOn()
        {
            visualMoñeco.SetActive(true);
            collider.enabled = true;
        }

        public void TurnOff()
        {
            visualMoñeco.SetActive(false);
            collider.enabled = false;
        }

        private void UpdateVisuals()
        {
            float progress = CurrentMachine.Progress;
            int partCount = bodyParts.Length;

            for (int i = 0; i < partCount; i++)
            {
                float partStart = (float)i / partCount;
                float partEnd = (float)(i + 1) / partCount;
                float localProgress = Mathf.Clamp01((progress - partStart) / (partEnd - partStart));

                var part = bodyParts[i];
                part.tween?.Kill();

                if (part.useScale)
                    part.tween = part.mask.DOScale(localProgress, 0.5f);
                else
                    part.tween = part.mask.DOLocalMove(Vector3.Lerp(part.initialPosition, part.endPosition, localProgress), 0.5f);
            }
        }

        public MachineSaveData CaptureState()
        {
            return new MachineSaveData
            {
                id = saveId,
                currentPresses = CurrentMachine.CurrentPresses,
                hasWorker = _currentUser != null,
                isEnabled = collider.enabled
            };
        }

        public void RestoreState(MachineSaveData data)
        {
            CurrentMachine.RestorePresses(data.currentPresses);
            if (data.isEnabled) TurnOn();
            else TurnOff();
            SetVisualsImmediate();
        }

        public void RestoreWorker(Interactor worker)
        {
            _currentUser = worker;
            CurrentMachine.RestoreWorker(worker);
        }

        public MoñecoMonoBehaviour SpawnWorker()
        {
            var go = Instantiate(_moñecoPrefab, positionToInteract.position, Quaternion.identity);
            var moñeco = go.GetComponent<MoñecoMonoBehaviour>();
            moñeco.RestoreInteraction(this, 1);
            RestoreWorker(moñeco);
            return moñeco;
        }

        private void SetVisualsImmediate()
        {
            float progress = CurrentMachine.Progress;
            int partCount = bodyParts.Length;
            for (int i = 0; i < partCount; i++)
            {
                float partStart = (float)i / partCount;
                float partEnd = (float)(i + 1) / partCount;
                float localProgress = Mathf.Clamp01((progress - partStart) / (partEnd - partStart));
                var part = bodyParts[i];
                part.tween?.Kill();
                if (part.useScale)
                    part.mask.localScale = Vector3.one * localProgress;
                else
                    part.mask.localPosition = Vector3.Lerp(part.initialPosition, part.endPosition, localProgress);
            }
        }

        private void ResetVisuals()
        {
            foreach (var part in bodyParts)
            {
                part.tween?.Kill();

                if (part.useScale)
                    part.mask.localScale = Vector3.zero;
                else
                    part.mask.localPosition = part.initialPosition;
            }
        }
    }
}

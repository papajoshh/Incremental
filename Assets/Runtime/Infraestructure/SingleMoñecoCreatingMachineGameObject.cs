using System;
using System.Threading.Tasks;
using DG.Tweening;
using Runtime.Application;
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

        [Header("Body Parts")]
        [SerializeField] private BodyPartVisual[] bodyParts;
        [SerializeField] private PressFeedback _pressFeedback;

        private Interactor _currentUser;
        private bool _canBeInteracted = true;
        private MoñecoCreatingMachine _currentMachine;

        private void Awake()
        {
            foreach (var part in bodyParts)
                part.initialPosition = part.mask.localPosition;
        }

        [Inject]
        private void Construct(BagOfMoñecos bag)
        {
            _currentMachine = new MoñecoCreatingMachine(bag, ticksToSpawn, this);
        }

        public bool CanInteract(Interactor interactor) => _currentUser == null && _canBeInteracted;

        public void StartInteraction(Interactor interactor)
        {
            _currentUser = interactor;
            interactor.SetPositionToInteract(positionToInteract);
        }

        public async Task OnInteractionTick(Interactor interactor)
        {
            if (!_canBeInteracted) return;
            await _currentMachine.ImpulseMoñecoCreation();
            UpdateVisuals();
        }

        public async void OnPointerDown(PointerEventData eventData)
        {
            if (!_canBeInteracted) return;
            if (_currentUser == null) return;
            await _currentMachine.ImpulseMoñecoCreation();
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
            await moñeco.GetComponent<Moñeco>().Birth();
            ResetVisuals();
            _canBeInteracted = true;
        }

        private void UpdateVisuals()
        {
            float progress = _currentMachine.Progress;
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

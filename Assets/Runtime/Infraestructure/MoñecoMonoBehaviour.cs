using System.Threading.Tasks;
using Runtime.Application;
using Runtime.Domain;
using Runtime.Infraestructure.MoñecoStates;
using UnityEngine;
using Zenject;

namespace Runtime.Infraestructure
{
    public class MoñecoMonoBehaviour : MonoBehaviour, Interactor
    {
        [Header("Movement")]
        [SerializeField] private float stepDistance = 0.1f;
        [SerializeField] private float fallStepDistance = 0.15f;

        [Header("Ground Detection")]
        [SerializeField] private Transform feetPosition;
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Wall Detection")]
        [SerializeField] private Transform bodyPosition;
        [SerializeField] private float bodyHalfWidth = 0.25f;
        [SerializeField] private LayerMask wallLayer;

        [Header("Initial State")]
        [SerializeField] private bool startWalking = true;

        private IMoñecoState _state;
        [SerializeField] private Animator _animator;
        private bool _restored;
        private TaskCompletionSource<bool> _birthTcs;
        private TaskCompletionSource<bool> _enterTcs;
        public Interactable CurrentInteractable { get; set; }
        private Interactable _pendingInteractable;
        private float? _walkToTargetX;
        
        [Inject] private readonly MoñecosSaveHandler _moñecosSaveHandler;

        public Transform Transform => transform;
        public int Direction { get; set; } = 1;
        public float StepDistance => stepDistance;
        public float FallStepDistance => fallStepDistance;
        public bool IsWalking => _state is WalkingState;

        void Start()
        {
            if (_state is BirthState) return;
            if (_restored) return;
            EvaluateAir();
        }

        void Update()
        {
            _state?.OnUpdate(this);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            CheckToInteract(collision);
            CheckToCrossDoor(collision);
        }

        private void CheckToInteract(Collider2D collision)
        {
            if (!IsWalking) return;
            if (_walkToTargetX.HasValue) return;

            var interactable = collision.GetComponent<Interactable>();
            if (interactable == null) return;
            if (!interactable.CanInteract(this)) return;
            _pendingInteractable = interactable;
            _pendingInteractable.StartInteraction(this);
        }

        private void CheckToCrossDoor(Collider2D collision)
        {
            if (!IsWalking) return;
            var door = collision.GetComponent<Door>();
            door?.CrossTo(gameObject.transform);
        }

        public void OnAnimStep() => _state?.OnStep(this);

        public void OnAnimComplete() => _state?.OnComplete(this);

        public void ChangeState<T>() where T : IMoñecoState, new()
        {
            if(_state is T) return;
            _state = new T();
            _state.OnEnter(this);
            var hash = _state.GetAnimationHash(this);
            if (hash.HasValue && _animator != null)
                _animator.Play(hash.Value);
        }

        public void EvaluateAir()
        {
            if (IsGrounded())
            {
                if (startWalking)
                {
                    Direction = 1;
                    ChangeState<WalkingState>();
                }
                else
                {
                    ChangeState<IdleState>();
                }
            }
            else
            {
                ChangeState<FallingState>();
            }
        }

        public bool IsGrounded()
        {
            var hit = Physics2D.Raycast(feetPosition.position, Vector2.down, groundCheckDistance, groundLayer);
            return hit.collider != null;
        }

        public bool CheckWallAhead(out RaycastHit2D hit)
        {
            Vector2 center = bodyPosition != null ? bodyPosition.position : transform.position;
            Vector2 origin = center + Vector2.right * Direction * bodyHalfWidth;
            hit = Physics2D.Raycast(origin, Vector2.right * Direction, stepDistance, wallLayer);
            return hit.collider != null;
        }

        public bool CheckGroundBelow(out RaycastHit2D hit)
        {
            hit = Physics2D.Raycast(feetPosition.position, Vector2.down, fallStepDistance, groundLayer);
            return hit.collider != null;
        }

        public void Move(Vector3 delta) => transform.position += delta;

        public bool HasReachedInteractionTarget()
        {
            if (!_walkToTargetX.HasValue) return false;
            return (Direction > 0 && transform.position.x >= _walkToTargetX.Value)
                || (Direction < 0 && transform.position.x <= _walkToTargetX.Value);
        }

        public void ArriveAtInteraction()
        {
            transform.position = new Vector3(_walkToTargetX.Value, transform.position.y, transform.position.z);
            CurrentInteractable = _pendingInteractable;
            _pendingInteractable = null;
            _walkToTargetX = null;
            ChangeState<InteractingState>();
        }

        public void CompleteBirth() => _birthTcs?.TrySetResult(true);

        public void DestroySelf()
        {
            _enterTcs.TrySetResult(true);
            _moñecosSaveHandler.Untrack(this);
            Destroy(gameObject);
        }

        public void PauseInteraction()
        {
            if (_state is not InteractingState) return;
            _animator.speed = 0;
        }

        public void ResumeInteraction()
        {
            if (_state is not InteractingState) return;
            _animator.speed = 1;
        }

        public void StopInteraction()
        {
            CurrentInteractable?.EndInteraction(this);
            CurrentInteractable = null;
            _walkToTargetX = null;
            _pendingInteractable = null;
            ChangeState<WalkingState>();
        }

        public void SetPositionToInteract(Transform interactPosition)
        {
            _walkToTargetX = interactPosition.position.x;
            if (HasReachedInteractionTarget())
                ArriveAtInteraction();
        }

        public async Task Birth()
        {
            ChangeState<BirthState>();
            _birthTcs = new TaskCompletionSource<bool>();
            await _birthTcs.Task;
        }

        public async Task EnterPortal()
        {
            ChangeState<EnterPortalState>();
            _enterTcs = new TaskCompletionSource<bool>();
            await _enterTcs.Task;
        }

        public void GoToBag() => ChangeState<EnterPortalState>();

        public MoñecoSaveData CaptureState()
        {
            string machineId = null;
            if (_state is InteractingState && CurrentInteractable != null)
            {
                machineId = CurrentInteractable.CurrentInteractionInfo.InteractableId;
            }

            return new MoñecoSaveData
            {
                x = transform.position.x,
                y = transform.position.y,
                direction = Direction,
                isInteracting = _state is InteractingState,
                assignedMachineId = machineId,
            };
        }

        public void RestoreInteraction(Interactable interactable, int savedDirection)
        {
            _restored = true;
            Direction = savedDirection;
            CurrentInteractable = interactable;
            _pendingInteractable = null;
            _walkToTargetX = null;
            ChangeState<InteractingState>();
        }

        public void RestoreWalking(int savedDirection)
        {
            _restored = true;
            Direction = savedDirection;
            ChangeState<WalkingState>();
        }

        private void OnDrawGizmosSelected()
        {
            if (feetPosition == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(feetPosition.position, feetPosition.position + Vector3.down * groundCheckDistance);
        }
    }
}

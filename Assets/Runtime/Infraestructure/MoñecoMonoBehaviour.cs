using System.Threading.Tasks;
using Runtime.Application;
using Runtime.Domain;
using Runtime.Infrastructure;
using Unity.VisualScripting;
using UnityEngine;

namespace Runtime.Infraestructure
{
    public class MoñecoMonoBehaviour : MonoBehaviour, Interactor
    {
        public enum State
        {
            Idle,
            Walking,
            Falling,
            Landing,
            Turning,
            Interacting,
            Birth
        }

        [Header("Movement")]
        [SerializeField] private float stepDistance = 0.1f;
        [SerializeField] private float fallStepDistance = 0.15f;
        [SerializeField] private float continuousSpeed = 2f;

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

        
        public bool IsWalking => currentState == State.Walking;
        private State currentState;
        private int direction = 1; // 1 = right, -1 = left
        private bool _restored;
        private Animator animator;
        private Rigidbody2D rb;
        private Interactable _currentInteractable;
        private Interactable _pendingInteractable;
        private float? _walkToTargetX;

        static readonly int AnimIdle = Animator.StringToHash("Idle");
        static readonly int AnimWalkRight = Animator.StringToHash("WalkRight");
        static readonly int AnimWalkLeft = Animator.StringToHash("WalkLeft");
        static readonly int AnimFalling = Animator.StringToHash("Falling");
        static readonly int AnimLanding = Animator.StringToHash("Landing");
        static readonly int AnimBirth = Animator.StringToHash("Birth");
        static readonly int AnimInteracting = Animator.StringToHash("Interacting");
        
        private TaskCompletionSource<bool> _birthTcs; 
        void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            if(currentState == State.Birth) return;
            if (_restored) return;
            Air();
        }

        private void Air()
        {
            if (IsGrounded())
            {
                if (startWalking)
                {
                    StartWalking(1); // Start moving right
                }
                else
                {
                    ChangeState(State.Idle);
                }
            }
            else
            {
                ChangeState(State.Falling);
            }
        }

        void Update()
        {
            UpdateGroundedState();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            CheckToInteract(collision);
            CheckToCrossDoor(collision);
        }
        
        private void CheckToInteract(Collider2D collision)
        {
            if (currentState != State.Walking) return;
            if (IsWalkingToInteraction()) return;

            var interactable = collision.GetComponent<Interactable>();
            if (interactable == null) return;
            if (!interactable.CanInteract(this)) return;
            _pendingInteractable = interactable;
            _pendingInteractable.StartInteraction(this);
        }

        private void CheckToCrossDoor(Collider2D collision)
        {
            if (currentState != State.Walking) return;
            var door = collision.GetComponent<Door>();
            door?.CrossTo(gameObject.transform);
        }

        public void PauseInteraction()
        {
            if (currentState != State.Interacting) return;
            animator.speed = 0;
        }

        public void ResumeInteraction()
        {
            if (currentState != State.Interacting) return;
            animator.speed = 1;
        }
        public void OnInteractionTick()
        {
            if (currentState != State.Interacting) return;
            _currentInteractable.OnInteractionTick(this);
        }

        public void StopInteraction()
        {
            _currentInteractable.EndInteraction(this);
            _currentInteractable = null;
            ClearWalkTarget();
            StartWalking(direction);
        }

        public void SetPositionToInteract(Transform interactPosition)
        {
            _walkToTargetX = interactPosition.position.x;
            if (HasReachedTarget())
                ArriveAtInteraction();
        }

        private bool IsWalkingToInteraction() => _walkToTargetX.HasValue;

        private bool HasReachedTarget()
        {
            if (!_walkToTargetX.HasValue) return false;
            return (direction > 0 && transform.position.x >= _walkToTargetX.Value)
                || (direction < 0 && transform.position.x <= _walkToTargetX.Value);
        }

        private void ArriveAtInteraction()
        {
            transform.position = new Vector3(_walkToTargetX.Value, transform.position.y, transform.position.z);
            _currentInteractable = _pendingInteractable;
            _pendingInteractable = null;
            ClearWalkTarget();
            ChangeState(State.Interacting);
        }

        private void ClearWalkTarget()
        {
            _walkToTargetX = null;
            _pendingInteractable = null;
        }

        private void UpdateGroundedState()
        {
            bool isGrounded = IsGrounded();

            // Detect if started falling (walked off a ledge)
            if (!isGrounded && (currentState == State.Walking || currentState == State.Idle))
            {
                ChangeState(State.Falling);
            }
        }

        public void OnStep()
        {
            if (currentState != State.Walking) return;

            // Check if wall is within this step's distance (raycast from body edge)
            Vector2 center = bodyPosition != null ? bodyPosition.position : transform.position;
            Vector2 moveDirection = Vector2.right * direction;
            Vector2 origin = center + moveDirection * bodyHalfWidth;
            RaycastHit2D hit = Physics2D.Raycast(origin, moveDirection, stepDistance, wallLayer);

            if (hit.collider != null)
            {
                // Wall ahead - don't move, just turn
                ChangeState(State.Turning);
                return;
            }

            transform.position += Vector3.right * direction * stepDistance;

            if (HasReachedTarget())
                ArriveAtInteraction();
        }
        public void OnFallStep()
        {
            if (currentState != State.Falling) return;

            // Check if ground is within this step's distance
            Vector2 origin = feetPosition.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, fallStepDistance, groundLayer);

            if (hit.collider != null)
            {
                // Snap to ground - move exactly to where feet touch the ground
                float distanceToGround = hit.distance;
                transform.position += Vector3.down * distanceToGround;
                ChangeState(State.Landing);
                return;
            }

            transform.position += Vector3.down * fallStepDistance;
        }
        public void OnLandingComplete()
        {
            if (currentState != State.Landing) return;
            StartWalking(direction);
        }
        public void OnWalkCycleComplete()
        {
            if (currentState != State.Turning) return;

            // Now turn and start walking in the new direction
            direction *= -1;
            ChangeState(State.Walking);
        }
        public void OnBirthComplete()
        {
            if (currentState != State.Birth) return;
            _birthTcs?.TrySetResult(true);
            Air();
        }

        private void StartWalking(int newDirection)
        {
            direction = newDirection;
            ChangeState(State.Walking);
        }

        private void ChangeState(State newState)
        {
            currentState = newState;
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (animator == null) return;

            switch (currentState)
            {
                case State.Idle:
                    animator.Play(AnimIdle);
                    break;
                case State.Walking:
                    animator.Play(direction > 0 ? AnimWalkRight : AnimWalkLeft);
                    break;
                case State.Falling:
                    animator.Play(AnimFalling);
                    break;
                case State.Landing:
                    animator.Play(AnimLanding);
                    break;
                case State.Turning:
                    // Keep playing current animation until cycle completes
                    break;
                case State.Interacting:
                    animator.Play(AnimInteracting);
                    break;
                case State.Birth:
                    animator.Play(AnimBirth);
                    break;
                default:
                    animator.Play(AnimIdle);
                    break;
            }
        }

        private bool IsGrounded()
        {
            Vector2 origin = feetPosition.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
            return hit.collider != null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(feetPosition.position, feetPosition.position + Vector3.down * groundCheckDistance);
        }

        public async Task Birth()
        {
            ChangeState(State.Birth);
            _birthTcs = new TaskCompletionSource<bool>();
            await _birthTcs.Task;
        }

        public MoñecoSaveData CaptureState()
        {
            string machineId = null;
            if (currentState == State.Interacting && _currentInteractable is MonoBehaviour mb)
            {
                if (mb is SingleMoñecoCreatingMachineGameObject machine)
                    machineId = machine.SaveId;
                else if (mb is RepairableComputerGameObject computer)
                    machineId = computer.SaveId;
            }

            return new MoñecoSaveData
            {
                x = transform.position.x,
                y = transform.position.y,
                direction = direction,
                isInteracting = currentState == State.Interacting,
                assignedMachineId = machineId,
            };
        }

        public void RestoreInteraction(Interactable interactable, int savedDirection)
        {
            _restored = true;
            direction = savedDirection;
            _currentInteractable = interactable;
            _pendingInteractable = null;
            _walkToTargetX = null;
            ChangeState(State.Interacting);
        }

        public void RestoreWalking(int savedDirection)
        {
            _restored = true;
            direction = savedDirection;
            StartWalking(savedDirection);
        }
    }

}

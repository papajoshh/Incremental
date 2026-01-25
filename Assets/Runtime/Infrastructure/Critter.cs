using UnityEngine;

public class Critter : MonoBehaviour
{
    public enum State
    {
        Idle,
        Walking,
        Falling,
        Landing,
        Turning
    }

    public enum MovementMode
    {
        StopMotion,  // Only moves when OnStep() is called from animation events
        Continuous   // Moves smoothly every frame
    }

    [Header("Movement")]
    [SerializeField] private MovementMode movementMode = MovementMode.StopMotion;
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

    private State currentState;
    private int direction = 1; // 1 = right, -1 = left
    private Animator animator;
    private Rigidbody2D rb;
    private bool wasGrounded;

    static readonly int AnimIdle = Animator.StringToHash("Idle");
    static readonly int AnimWalkRight = Animator.StringToHash("WalkRight");
    static readonly int AnimWalkLeft = Animator.StringToHash("WalkLeft");
    static readonly int AnimFalling = Animator.StringToHash("Falling");
    static readonly int AnimLanding = Animator.StringToHash("Landing");

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
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

        // Continuous movement mode
        if (movementMode == MovementMode.Continuous && currentState == State.Walking)
        {
            transform.position += Vector3.right * direction * continuousSpeed * Time.deltaTime;
        }
    }

    private void UpdateGroundedState()
    {
        bool isGrounded = IsGrounded();

        // Detect if started falling (walked off a ledge)
        if (!isGrounded && (currentState == State.Walking || currentState == State.Idle))
        {
            ChangeState(State.Falling);
        }

        wasGrounded = isGrounded;
    }

    /// <summary>
    /// Called from Animation Events on each frame of walk animations (StopMotion mode only).
    /// </summary>
    public void OnStep()
    {
        if (movementMode != MovementMode.StopMotion) return;
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
    }

    /// <summary>
    /// Called from Animation Events on each frame of Falling animation (StopMotion mode only).
    /// </summary>
    public void OnFallStep()
    {
        if (movementMode != MovementMode.StopMotion) return;
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

    /// <summary>
    /// Called from Animation Event at the end of Landing animation.
    /// </summary>
    public void OnLandingComplete()
    {
        if (currentState != State.Landing) return;
        StartWalking(direction);
    }

    /// <summary>
    /// Called from Animation Event at the end of each walk cycle (WalkLeft/WalkRight).
    /// </summary>
    public void OnWalkCycleComplete()
    {
        if (currentState != State.Turning) return;

        // Now turn and start walking in the new direction
        direction *= -1;
        ChangeState(State.Walking);
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
}

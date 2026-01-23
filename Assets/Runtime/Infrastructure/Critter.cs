using System;
using UnityEngine;

public class Critter : MonoBehaviour
{
    public enum State
    {
        Idle,
        Walking,
        Falling
    }

    public enum MovementMode
    {
        StopMotion,  // Only moves when OnStep() is called from animation events
        Continuous   // Moves smoothly every frame
    }

    [Header("Movement")]
    [SerializeField] private MovementMode movementMode = MovementMode.StopMotion;
    [SerializeField] private float stepDistance = 0.1f;
    [SerializeField] private float continuousSpeed = 2f;

    [Header("Ground Detection")] 
    [SerializeField] private Transform feetPosition;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Initial State")]
    [SerializeField] private bool startWalking = true;

    private State currentState;
    private int direction = 1; // 1 = right, -1 = left
    private Animator animator;
    private float lastHeightOnGround = -Mathf.Infinity;
    private float gapToGetFalling = 0.2f;
    private Rigidbody2D rb;
    private bool wasGrounded;

    static readonly int AnimIdle = Animator.StringToHash("Idle");
    static readonly int AnimWalkRight = Animator.StringToHash("WalkRight");
    static readonly int AnimWalkLeft = Animator.StringToHash("WalkLeft");
    static readonly int AnimFalling = Animator.StringToHash("Falling");

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Initialize lastHeightOnGround to current position
        lastHeightOnGround = feetPosition.position.y;

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

        // Detect if started falling
        if (currentState != State.Falling && IsFalling())
        {
            ChangeState(State.Falling);
        }
        // Detect if landed
        else if (isGrounded && currentState == State.Falling)
        {
            // On landing, resume walking in the same direction
            StartWalking(direction);
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

        transform.position += Vector3.right * direction * stepDistance;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState != State.Walking) return;

        // Check if hit from the side (not from above/below)
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // If normal points mostly horizontal, it's a wall
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                Turn();
                break;
            }
        }
    }

    private void Turn()
    {
        direction *= -1;
        UpdateAnimation();

        // Flip the sprite
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
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
            default:
                animator.Play(AnimIdle);
                break;
        }
    }

    private bool IsGrounded()
    {
        Vector2 origin = feetPosition.position;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        if (hit.collider != null)
        {
            lastHeightOnGround = origin.y;
            return true;
        }
        return false;
    }

    private bool IsFalling()
    {
        if(IsGrounded()) return false;
        return Math.Abs(lastHeightOnGround - feetPosition.position.y) > gapToGetFalling;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(feetPosition.position, feetPosition.position + Vector3.down * groundCheckDistance);
    }
}

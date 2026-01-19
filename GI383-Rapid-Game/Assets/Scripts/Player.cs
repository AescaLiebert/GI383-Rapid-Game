using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    public int HP;
    public int level;
    [Header("Movement Stats")]
    public float jump;
    public float speed;

    [Header("Attack Stats")]
    private string weaponName;
    [SerializeField] private Transform atttackpoit;
    public Weapon currentWeapon;

    [Header("Dash Stats")]
    public float dashspeed;
    public float dashCooldown;
    public float dashTime;

    private bool canDash = true;
    public bool dashing;

    [Header("Movement Feel")]
    public float jumpStartupTime = 0.05f; // Time before jump force is applied
    public float landingLagTime = 0.1f;   // Time locked after landing
    public float bufferWindow = 0.3f;    // Time to store input

    // State Flags
    private bool isJumpStarting;
    private bool isLanding;
    private bool isAttacking;
    private bool wasGrounded;

    // Action Buffer
    public enum BufferedAction { None, Jump, Dash, Attack }
    private BufferedAction bufferedAction = BufferedAction.None;
    private float actionBufferTimer;

    public Animator anim;
    public SpriteRenderer spriteRenderer;
    private Vector2 moveInput;


    public Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }


    void Update()
    {
        bool isGrounded = IsGrounded();

        // Landing Detection
        if (isGrounded && !wasGrounded && rb.linearVelocity.y <= 0.1f)
        {
            // Only trigger landing lag if we were actually falling/in air significantly, 
            // but for simplicity, any ground touch triggers logic. 
            // We can check if we are not already landing to avoid re-triggering.
            if (!isLanding && !isJumpStarting && !dashing && !isAttacking)
            {
                StartCoroutine(LandingCoroutine());
            }
        }
        wasGrounded = isGrounded;

        // Process Action Buffer
        if (actionBufferTimer > 0)
        {
            actionBufferTimer -= Time.deltaTime;
            if (CanPerformAction(bufferedAction))
            {
                ExecuteAction(bufferedAction);
                bufferedAction = BufferedAction.None;
                actionBufferTimer = 0;
            }
        }
        else
        {
            bufferedAction = BufferedAction.None;
        }

        // Movement Logic
        // Lock Direction Change if Dashing OR JumpStarting OR Landing OR Attacking
        bool isMovementLocked = dashing || isJumpStarting || isLanding || isAttacking;

        if (!isMovementLocked && moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x > 0;
            if (currentWeapon != null)
            {
                currentWeapon.Turn(moveInput.x > 0);
            }
        }

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        // Jump Animation
        bool isGrounded = IsGrounded();
        anim.SetBool("IsJump", !isGrounded);

        // Attack Animation
        anim.SetBool("IsAttack", isAttacking);

        if (dashing)
        {
            anim.SetBool("IsIdle", false);
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsDash", true);
        }
        else if (moveInput.x != 0 && !isJumpStarting && !isLanding && !isAttacking)
        {
            anim.SetBool("IsIdle", false);
            anim.SetBool("IsWalk", true);
            anim.SetBool("IsDash", false);
        }
        else
        {
            anim.SetBool("IsIdle", true);
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsDash", false);
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            AttemptAction(BufferedAction.Jump);
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            AttemptAction(BufferedAction.Dash);
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            AttemptAction(BufferedAction.Attack);
        }
    }

    // --- Action Buffer System ---

    private void AttemptAction(BufferedAction action)
    {
        if (CanPerformAction(action))
        {
            ExecuteAction(action);
        }
        else
        {
            // Buffer the action
            bufferedAction = action;
            actionBufferTimer = bufferWindow;
        }
    }

    private bool CanPerformAction(BufferedAction action)
    {
        // Global locks safely handled here?
        // Note: Dashing usually blocks everything except maybe internal implementation details.

        switch (action)
        {
            case BufferedAction.Jump:
                return IsGrounded() && !dashing && !isJumpStarting && !isLanding && !isAttacking;

            case BufferedAction.Dash:
                return canDash && !dashing && !isJumpStarting && !isLanding && !isAttacking;

            case BufferedAction.Attack:
                // Cannot attack if dashing, jumping start, landing, or ALREADY attacking
                return currentWeapon != null && !dashing && !isJumpStarting && !isLanding && !isAttacking;

            default:
                return false;
        }
    }

    private void ExecuteAction(BufferedAction action)
    {
        switch (action)
        {
            case BufferedAction.Jump:
                StartCoroutine(JumpCoroutine());
                break;
            case BufferedAction.Dash:
                StartCoroutine(Dash());
                break;
            case BufferedAction.Attack:
                StartCoroutine(AttackCoroutine());
                break;
        }
    }

    // --- Coroutines & Physics ---

    void FixedUpdate()
    {
        if (dashing) return;

        // Disable movement if JumpStarting or Landing or Attacking
        if (isJumpStarting || isLanding || isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Stop X movement, keep Y (gravity)
            return;
        }

        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private IEnumerator JumpCoroutine()
    {
        isJumpStarting = true;

        // Startup Delay (Movement Locked)
        yield return new WaitForSeconds(jumpStartupTime);

        // Apply Force
        rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);

        isJumpStarting = false;
    }

    private IEnumerator LandingCoroutine()
    {
        isLanding = true;
        // Optional: Play landing anim or particulate

        // Landing Lag (Movement Locked)
        yield return new WaitForSeconds(landingLagTime);

        isLanding = false;
        // Upon unlocking, buffered actions might fire immediately next Update
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        currentWeapon.Attack();

        yield return new WaitForSeconds(currentWeapon.attackDuration);

        isAttacking = false;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        dashing = true;

        // Immediately invalidate other buffered actions logic handled by CanPerformAction?
        // Actually, if we buffer a Jump during Dash, it will fire after Dash finishes.

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        }

        StartCoroutine(ShowGhosts());

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        float dashDirection = spriteRenderer.flipX ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashspeed, 0f);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = originalGravity;
        dashing = false;

        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator ShowGhosts()
    {
        while (dashing)
        {
            GameObject ghost = new GameObject("Ghost");
            ghost.transform.position = transform.position;
            ghost.transform.rotation = transform.rotation;
            ghost.transform.localScale = transform.localScale;

            SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
            ghostSr.sprite = spriteRenderer.sprite;
            ghostSr.flipX = spriteRenderer.flipX;
            ghostSr.sortingLayerID = spriteRenderer.sortingLayerID;
            ghostSr.sortingOrder = spriteRenderer.sortingOrder - 1;

            ghost.AddComponent<GhostFade>();

            yield return new WaitForSeconds(0.038f);
        }

    }



    public void TakeDamage(int damage)
    {
        HP -= damage;

        if (HP <= 0)
        {
            Debug.Log("Player Dead");
        }
    }
}
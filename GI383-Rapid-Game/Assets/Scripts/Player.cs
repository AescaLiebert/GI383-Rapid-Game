using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    public int HP;
    public int level;
    
    [Header("UI References")]
    public HealthBarUI healthBarUI;
    [Header("Movement Stats")]
    public float jump;
    public float speed;

    [Header("Attack Stats")]
    private string weaponName;
    [SerializeField] private Transform atttackpoit;
    public Weapon currentWeapon;
    public Weapon knifeWeapon;

    [Header("Dash Stats")]
    public float dashspeed;
    public float dashCooldown;
    public float dashTime;

    private bool canDash = true;
    public bool dashing;

    [Header("Damage Settings")]
    public float knockbackForceY = 5f;
    public float hitStunTime = 0.2f;
    public float invincibilityTime = 1.0f;
    public float hitStopDuration = 0.1f;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.3f;
    private bool isHit = false;
    private bool isInvincible = false;
    private Color originalColor;

    [Header("Movement Feel")]
    public float jumpStartupTime = 0.05f; // Time before jump force is applied
    public float landingLagTime = 0.1f;   // Time locked after landing
    public float bufferWindow = 0.3f;    // Time to store input

    public GameObject deathPanel; //GameOver Panel
    public DeathSequenceController deathSequenceController;

    // State Flags
    private bool isJumpStarting;
    private bool isLanding;
    private bool isAttacking;
    private bool isShooting;
    private bool wasGrounded;
    private bool isDead = false;

    // Action Buffer
    public enum BufferedAction { None, Jump, Dash, Attack , Shoot}
    private BufferedAction bufferedAction = BufferedAction.None;
    private float actionBufferTimer;

    public Animator anim;
    public SpriteRenderer spriteRenderer;
    private Vector2 moveInput;


    public Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    private int enemyLayer;
    private CameraFollow camFollow;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        enemyLayer = LayerMask.NameToLayer("Enemy");
        camFollow = Camera.main.GetComponent<CameraFollow>();
        
        // Initialize health bar UI
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(HP);
        }
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
            if (!isLanding && !isJumpStarting && !dashing && !isAttacking && !isHit)
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
        // Lock Direction Change if Dashing OR JumpStarting OR Landing OR Attacking OR Hit
        bool isMovementLocked = dashing || isJumpStarting || isLanding || isAttacking || isHit || isShooting;

        if (!isMovementLocked && moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x > 0;
            if (currentWeapon != null)
            {
                currentWeapon.Turn(moveInput.x > 0);
            }

             if (knifeWeapon != null)
            {
                knifeWeapon.Turn(moveInput.x > 0);
            }
        }

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool isGrounded = IsGrounded();
        
        // 1. Core State Triggers
        // Priority: Hit > Attack > Shoot > Dash > Jump > Move/Idle
        anim.SetBool("IsAttack", isAttacking);
        anim.SetBool("IsShoot", isShooting);
        anim.SetBool("IsDash", dashing);
        anim.SetBool("IsTakeDamage", isHit);
        
        if (isHit)
        {
             // If Hit, suppress everything else usually, but Animator might handle AnyState -> Hit
             anim.SetBool("IsJump", false);
             anim.SetBool("IsAttack", false);
             anim.SetBool("IsShoot", false);
             anim.SetBool("IsDash", false);
             anim.SetBool("IsIdle", false);
             anim.SetBool("IsWalk", false);
             return; // Quick exit to ensure Hit takes over
        }

        if (isAttacking)
        {
            // If attacking, suppress Jump state to prevent animation flickering/interruption
            anim.SetBool("IsJump", false);
        }
        else if (isShooting)
        {
            // If shooting, suppress Jump state to prevent animation flickering/interruption
            anim.SetBool("IsJump", false);
        }
        else if (dashing)
        {
            // If dashing, suppress Jump state to allow Air Dash animation
            anim.SetBool("IsJump", false);
        }
        else
        {
            anim.SetBool("IsJump", !isGrounded);
        }

        // 2. Movement Logic (Idle/Walk)
        // If we are doing a High Priority action (Dash, Attack, Shoot, Jump/Fall), disable basic ground movement animations
        if (dashing || isAttacking || isShooting || !isGrounded)
        {
            anim.SetBool("IsIdle", false);
            anim.SetBool("IsWalk", false);
        }
        else 
        {
            // Grounded & Neutral
            bool isMoving = moveInput.x != 0;
            anim.SetBool("IsWalk", isMoving);
            anim.SetBool("IsIdle", !isMoving);
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

    public void OnShoot(InputValue value)
    {
        if (value.isPressed)
        {
            //Debug.Log("OnShoot called - Attempting to shoot");
            if (knifeWeapon == null)
            {
                //Debug.LogWarning("Cannot shoot: knifeWeapon is not assigned! Please assign a ThrowKnife weapon to the knifeWeapon field in the Inspector.");
                return;
            }
            AttemptAction(BufferedAction.Shoot);
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
                return IsGrounded() && !dashing && !isJumpStarting && !isLanding && !isAttacking && !isHit;

            case BufferedAction.Dash:
                return canDash && !dashing && !isJumpStarting && !isLanding && !isAttacking && !isHit;

            case BufferedAction.Attack:
                // Cannot attack if dashing, jumping start, landing, or ALREADY attacking
                return currentWeapon != null && !dashing && !isJumpStarting && !isLanding && !isAttacking && !isHit;
            
            case BufferedAction.Shoot: // [NEW] ปืน
                // ยิงได้ถ้ามีปืน และไม่ได้กำลังทำอย่างอื่น (รวมถึงไม่ได้ฟันดาบอยู่)
                return knifeWeapon != null && !dashing && !isJumpStarting && !isLanding && !isAttacking && !isShooting && !isHit;

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
            case BufferedAction.Shoot: 
                StartCoroutine(ShootCoroutine());
                break;
        }
    }

    // --- Coroutines & Physics ---

    void FixedUpdate()
    {
        if (dashing) return;

        // Disable movement if JumpStarting or Landing or Attacking or Hit
        if (isJumpStarting || isLanding || isAttacking || isHit || isShooting)
        {
            // If hit, we might want to respect knockback physics (which implies we shouldn't zero out X immediately IF we added X knockback)
            // But for now, user only asked for Y knock up. However, locking X movement is fine.
            // If we want pure physics knockback, we should avoid setting velocity here.
            
            if (isHit) 
            {
                 // Do not interfere with physics during knockback
                 return;
            }

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

        // Ignore Enemy Collision during jump
        IgnoreEnemyCollision(true);

        // Apply Force
        rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);

        isJumpStarting = false;
    }

    private IEnumerator LandingCoroutine()
    {
        isLanding = true;
        // Optional: Play landing anim or particulate

        // Restore Enemy Collision on landing
        if (!isInvincible)
        {
            IgnoreEnemyCollision(false);
        }

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

    private IEnumerator ShootCoroutine()
    {
        isShooting = true;
        Debug.Log("ShootCoroutine executing - Shooting now!");
        knifeWeapon.Attack();
        yield return new WaitForSeconds(knifeWeapon.attackDuration);
        isShooting = false;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        dashing = true;
        isInvincible = true;

        // Immediately invalidate other buffered actions logic handled by CanPerformAction?
        // Actually, if we buffer a Jump during Dash, it will fire after Dash finishes.

        int playerLayer = gameObject.layer;
        
        IgnoreEnemyCollision(true);

        StartCoroutine(ShowGhosts());

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        float dashDirection = spriteRenderer.flipX ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashspeed, 0f);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = originalGravity;
        dashing = false;

        // Only restore collision if we are arguably "safe" or grounded?
        // If we dash into the air, we want to keep ignoring collision (Jump behavior).
        // If we dash on ground, we restore it.
        // LandingCoroutine will handle restoring it if we land later.


        // Extended invincibility after dash
        yield return new WaitForSeconds(0.15f);
        if (!isHit) // Only turn off invincibility if we weren't hit (though we shouldn't be able to get hit if invincible)
        {
             isInvincible = false;
             
             // Restore collision if on ground
             if (IsGrounded())
             {
                 IgnoreEnemyCollision(false);
             }
        }

        yield return new WaitForSeconds(dashCooldown - 0.15f);
        canDash = true;
    }

    private void IgnoreEnemyCollision(bool ignore)
    {
        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, ignore);
        }
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
        if (isInvincible) return;

        HP -= damage;
        
        // Update health bar UI
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(HP);
        }

        if (HP <= 0 && !isDead)
        {
            isDead = true;
            if (deathSequenceController != null)
            {
                deathSequenceController.StartDeathSequence();
            }
            else
            {
                // Fallback
                if (deathPanel != null) deathPanel.SetActive(true);
                Time.timeScale = 0f;
            }
            Debug.Log("Player Dead");
        }

        if (camFollow != null) camFollow.TriggerShake(shakeDuration, shakeMagnitude);
        StartCoroutine(HitStop());
        StartCoroutine(HitRoutine());
    }

    private IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
    }

    private IEnumerator HitRoutine()
    {
        isHit = true;
        isInvincible = true;
        
        // Ignore enemy collision during invincibility
        IgnoreEnemyCollision(true);
        
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.up * knockbackForceY, ForceMode2D.Impulse);
        
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(hitStunTime);

        isHit = false;
        anim.SetBool("IsTakeDamage", false);
        spriteRenderer.color = originalColor; // Revert to original color (or white) before flickering

        // Invincibility Flicker
        float flashDelay = 0.1f;
        float timer = 0f;
        
        while (timer < invincibilityTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashDelay);
            timer += flashDelay;
        }

        spriteRenderer.enabled = true;
        isInvincible = false;
        
        // Restore enemy collision when invincibility ends
        IgnoreEnemyCollision(false);
    }
}
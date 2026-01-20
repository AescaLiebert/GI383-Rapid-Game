using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 3f;
    public int hp = 10;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Knockback Settings")] 
    public float knockbackForce = 5f; 
    public float knockbackDuration = 0.2f;

    [Header("Attack Settings")]
    public float attackRange = 1.5f; 
    public float attackCooldown = 1.5f; 
    public float attackDelay = 0.5f; // Delay before damage is dealt
    private float nextAttackTime = 0f;  
    private bool isAttacking = false;  
    public int damage = 10;
    public Transform attackPoint;
    public Vector2 attackArea = new Vector2(1f, 0.5f);

    public GameObject slash;


    private bool isKnockedBack = false;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine knockbackCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        
        // Find player by tag or type. defaulting to FindObjectOfType for simplicity if tag isn't set.
        // Assuming the Player script is attached to the player object.
        Player playerScript = FindFirstObjectByType<Player>();
        if (playerScript != null)
        {
            player = playerScript.transform;
        }
    }

    void FixedUpdate()
    {
        if (isKnockedBack || isAttacking) return;

        float distanceFromPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceFromPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackRoutine());
                nextAttackTime = Time.time + attackCooldown + attackDelay;
            }
        }
        // Anti-float logic:
        // If we are grounded, do not allow upward velocity (unless we add jumping later).
        // This prevents the enemy from 'climbing' the player collider.
        

        
        if (IsGrounded() && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Preserve Y velocity for gravity (platformer style)
        // Ensure we only apply X force.
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        // Visual handling checks
        if (direction.x != 0)
        {
            bool faceRight = direction.x > 0;
            spriteRenderer.flipX = faceRight;
            Vector3 currentPos = attackPoint.localPosition;

            if (faceRight)
            {
                
                currentPos.x = Mathf.Abs(currentPos.x);
            }
            else
            {
               
                currentPos.x = -Mathf.Abs(currentPos.x);
            }

            
            attackPoint.localPosition = currentPos;
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    public void TakeDamage(int damage)
    {

        if(hp > 0)
        {
            if (knockbackCoroutine != null)
            {
                StopCoroutine(knockbackCoroutine);
            }
            knockbackCoroutine = StartCoroutine(KnockedBackRoutine());
        }

        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Notify WaveManager or Spawner if needed (can be done via event or static reference)
        Destroy(gameObject);
    }

    IEnumerator KnockedBackRoutine()
    {
        isKnockedBack = true;
        Vector2 direction = (transform.position - player.position).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(knockbackDuration);
        
        isKnockedBack = false;
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(1f);
        spriteRenderer.color = originalColor;
    }
    void StopMoving()
    {
        
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        // Optional: Trigger "Prepare" animation or color change here to telegraph attack

        yield return new WaitForSeconds(attackDelay);

        if (slash != null)
        {
            GameObject currentSlash = Instantiate(slash, attackPoint.position, attackPoint.rotation);
            
            if(spriteRenderer.flipX == true)
            {
                Vector3 newScale = currentSlash.transform.localScale;
                newScale.x *= 1; 
                newScale.y *= -1; 
                currentSlash.transform.localScale = newScale;
            }
        }
        Debug.Log("โจมตีผู้เล่น!");

        
        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            // Re-check distance or collider here? 
            // For now, simple distance check or just hit if still in range?
            // User asked for dodge, so we should probably check if player is still within effective range or use a collider check.
            // But preserving original logic: "Enemy โจมตีโดนผู้เล่น" which was guaranteed if AttackPlayer was called.
            // If we want dodge, we must check for hit connection *after* delay.
            
            // Let's check distance again to see if player successfully dodged out of range.
            float distance = Vector2.Distance(transform.position, player.position);
            // We can treat attackArea as the effective hit box.
            // Since we don't have a real hitbox collider logic here (it was just direct damage), 
            // let's assume if player is within attackRange + small buffer, they get hit.
            if (distance <= attackRange * 1.2f) 
            {
                playerScript.TakeDamage(damage);
                Debug.Log("Enemy โจมตีโดนผู้เล่น!");
            }
            else
            {
                Debug.Log("Player Dodged!");
            }
        }
        
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;

        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);


        Gizmos.DrawWireCube(Vector3.zero, attackArea);
    }
}

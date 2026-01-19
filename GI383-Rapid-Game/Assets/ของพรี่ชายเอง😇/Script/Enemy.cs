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
    private float nextAttackTime = 0f;  
    public int damage = 10;
    public Transform attackPoint;
    public Vector2 attackArea = new Vector2(1f, 0.5f);

    public GameObject slash;


    private bool isKnockedBack = false;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
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
        if (isKnockedBack) return;

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
                AttackPlayer();
                nextAttackTime = Time.time + attackCooldown;
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
            StopCoroutine(KnockedBackRoutine());
            StartCoroutine(KnockedBackRoutine());
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

        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }
    void StopMoving()
    {
        
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void AttackPlayer()
    {

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
            playerScript.TakeDamage(damage);
            Debug.Log("Enemy โจมตีโดนผู้เล่น!");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;

        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);


        Gizmos.DrawWireCube(Vector3.zero, attackArea);
    }
}

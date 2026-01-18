using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 3f;
    public int hp = 10;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

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
        if (player != null)
        {
            MoveTowardsPlayer();
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
            spriteRenderer.flipX = direction.x > 0;
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    public void TakeDamage(int damage)
    {
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
}

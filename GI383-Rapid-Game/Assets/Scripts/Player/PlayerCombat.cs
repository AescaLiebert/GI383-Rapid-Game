using UnityEngine;
using System.Collections;
using System;

public class PlayerCombat : MonoBehaviour
{
    [Header("Melee Settings")]
    public Transform attackPoint;
    public Vector2 attackArea = new Vector2(1f, 0.5f);
    public LayerMask enemyLayers;
    public GameObject attackEffectPrefab;
    
    [Header("Combat Stats")]
    public float attackCooldown = 0.5f;
    public float knockbackForceX = 5f;
    public float knockbackForceY = 0f;
    public float stunDuration = 0.2f; // Stun duration on enemy AFTER landing
    public float attackDuration = 0.2f; // Player animation lock duration

    [Header("Weapons (Deprecated/Secondary)")]
    // Keeping references just in case, or for secondary weapons like Knife
    public Weapon knifeWeapon;
    
    // Dependencies
    public PlayerStats stats;
    private PlayerMovement movement;

    [Header("States")]
    public bool IsAttacking { get; private set; }
    public bool IsShooting { get; private set; }

    // Events
    public event Action OnAttackStart;
    public event Action OnAttackEnd;
    public event Action OnShootStart;
    public event Action OnShootEnd;

    private float nextAttackTime = 0f;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Handle Attack Direction Visuals (AttackPoint rotation)
        if (attackPoint != null && movement != null && movement.spriteRenderer != null)
        {
            // PlayerMovement: flipX = input.x > 0 (Right). 
            // So flipX (True) means Facing Right.
            
            float dir = movement.spriteRenderer.flipX ? 1f : -1f;
            
            // Flip Position (X) based on local relative
            Vector3 pos = attackPoint.localPosition;
            pos.x = Mathf.Abs(pos.x) * dir;
            attackPoint.localPosition = pos;

            // Flip Rotation (Y) - 0 is Right, 180 is Left
            Vector3 rot = attackPoint.localEulerAngles;
            rot.y = movement.spriteRenderer.flipX ? 0f : 180f;
            attackPoint.localEulerAngles = rot;
        }
    }

    public void Attack()
    {
        if (CanAttack())
        {
            StartCoroutine(AttackCoroutine());
        }
    }

    public void Shoot()
    {
        if (CanShoot())
        {
            StartCoroutine(ShootCoroutine());
        }
    }

    private bool CanAttack()
    {
        return Time.time >= nextAttackTime && !IsAttacking && !IsShooting && !movement.IsDashing && !movement.IsLanding; 
    }

    private bool CanShoot()
    {
        return knifeWeapon != null && !IsAttacking && !IsShooting && !movement.IsDashing && !movement.IsLanding;
    }

    private IEnumerator AttackCoroutine()
    {
        IsAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        
        // Optional: Stop movement during attack? If desired.
        if (movement != null) movement.CanMove = false;
        
        OnAttackStart?.Invoke();

        PerformMeleeAttack();

        yield return new WaitForSeconds(attackDuration);
        
        if (movement != null) movement.CanMove = true;
        IsAttacking = false;
        OnAttackEnd?.Invoke();
    }

    public void PerformMeleeAttack()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("AttackPoint is not assigned in PlayerCombat!");
            return;
        }

        // 1. Visual Effect
        if (attackEffectPrefab != null)
        {
            Instantiate(attackEffectPrefab, attackPoint.position, attackPoint.rotation);
        }

        // 2. Detect Enemies (AoE)
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackArea, attackPoint.eulerAngles.z, enemyLayers);

        // 3. Apply Damage & Knockback
        float damage = stats != null ? stats.attackDamage : 10f;

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Calculate Knockback Direction (Away from player/attackPoint)
                Vector2 dir = (enemy.transform.position - transform.position).normalized;
                
                // Use explicit X/Y forces
                float directionX = Mathf.Sign(dir.x);
                Vector2 knockbackVector = new Vector2(directionX * knockbackForceX, knockbackForceY);
                
                enemy.TakeDamage(damage, knockbackVector, stunDuration);
                Debug.Log($"Hit {enemy.name} for {damage} damage! KB: {knockbackVector}, Stun: {stunDuration}");
            }
        }
    }

    private IEnumerator ShootCoroutine()
    {
        IsShooting = true;
        if (movement != null) movement.CanMove = false;
        OnShootStart?.Invoke();
        
        if (knifeWeapon != null)
        {
            knifeWeapon.Attack();
            yield return new WaitForSeconds(knifeWeapon.attackDuration);
        }

        if (movement != null) movement.CanMove = true;
        IsShooting = false;
        OnShootEnd?.Invoke();
    }

    // Called by InputHandler via SendMessage or direct call
    public void PerformAttack() => Attack();
    public void PerformShoot() => Shoot();
    
    public void TurnWeapons(bool facingRight)
    {
        // Deprecated for primary weapon, but useful if we keep secondary logic
        if (knifeWeapon != null) knifeWeapon.Turn(facingRight);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, attackArea);
    }
}

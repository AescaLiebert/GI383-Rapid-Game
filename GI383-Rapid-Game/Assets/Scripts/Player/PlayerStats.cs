using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("Data")]
    public PlayerDataSO playerData;

    [Header("Runtime Stats")]
    public int currentHP;
    public int currentXP;
    public int currentLevel = 1;

    [Header("Calculated Stats")]
    public int maxHP;
    public float attackDamage;

    // Movement Stats calculated from Level
    public float CurrentMoveSpeed { get; private set; }
    public float CurrentJumpForce { get; private set; }
    public float CurrentDashSpeed { get; private set; }
    
    // Events
    public event Action<int> OnHealthChanged;
    public event Action<int, int> OnXPChanged; // current, max for next level
    public event Action<int> OnLevelUp;
    public event Action OnDeath;
    public event Action OnDamageTaken;

    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    void Start()
    {
        if (playerData != null)
        {
            InitializeStats();
        }
    }

    public bool IsHit { get; private set; }
    public bool IsDead => currentHP <= 0;
    public float hitStunTime = 0.2f;
    private float hitStunTimer = 0f;

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
            }
        }

        if (IsHit)
        {
            hitStunTimer -= Time.deltaTime;
            if (hitStunTimer <= 0)
            {
                IsHit = false;
            }
        }
    }

    public void InitializeStats()
    {
        // Calculate based on level
        CalculateStats();
        
        currentHP = maxHP;
        UpdateHealthUI();
    }

    private void CalculateStats()
    {
        if (playerData == null) return;

        // RPG Stats (Growth Factor)
        maxHP = playerData.CalculateRPGStat(playerData.baseMaxHP, currentLevel);
        attackDamage = playerData.CalculateAttackStat(playerData.baseAttackDamage, currentLevel);

        // Movement Stats (Asymptotic/Limit)
        CurrentMoveSpeed = playerData.CalculateLimitStat(playerData.baseMoveSpeed, playerData.limitMoveSpeed, currentLevel);
        CurrentJumpForce = playerData.CalculateLimitStat(playerData.baseJumpForce, playerData.limitJumpForce, currentLevel);
        CurrentDashSpeed = playerData.CalculateLimitStat(playerData.baseDashSpeed, playerData.limitDashSpeed, currentLevel);

        Debug.Log($"Stats Calculated [Level {currentLevel}]: HP={maxHP} (Base {playerData.baseMaxHP}), Atk={attackDamage} (Base {playerData.baseAttackDamage})"); 
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || IsHit) return;
        if (currentHP <= 0) return;

        currentHP -= damage;
        OnHealthChanged?.Invoke(currentHP);
        OnDamageTaken?.Invoke();
        
        if (SoundManager.Instance != null)
             SoundManager.Instance.PlaySound("Player_TakeDamage", transform.position);

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
        else
        {
            // Apply Hit Stun
            IsHit = true;
            hitStunTimer = hitStunTime;
            SetInvincible(1.0f); // Default invincibility time
        }
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        OnHealthChanged?.Invoke(currentHP);
        
        if (SoundManager.Instance != null && amount > 0)
             SoundManager.Instance.PlaySound("Player_Heal", transform.position);
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        
        CheckLevelUp();
        
        int xpToNext = GetXPToNextLevel();
        OnXPChanged?.Invoke(currentXP, xpToNext);
        
        if (SoundManager.Instance != null && amount > 0)
             SoundManager.Instance.PlaySound("Player_EXPCollect", transform.position);
    }

    private void CheckLevelUp()
    {
        int xpToNext = GetXPToNextLevel();
        // Simple loop in case we gain enough for multiple levels
        while (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            LevelUp();
            xpToNext = GetXPToNextLevel();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        
        // Recalculate stats
        CalculateStats();
        
        // Heal on level up?
        //currentHP = maxHP;
        
        OnLevelUp?.Invoke(currentLevel);
        OnHealthChanged?.Invoke(currentHP);
        
        if (SoundManager.Instance != null)
             SoundManager.Instance.PlaySound("Player_LevelUp", transform.position);
        
        if (FloatingTextManager.Instance != null)
            FloatingTextManager.Instance.ShowLevelUp(transform.position + Vector3.up);
        
        Debug.Log($"Level Up! New Level: {currentLevel}. HP: {maxHP}, Atk: {attackDamage}");
    }

    public int GetXPToNextLevel()
    {
        if (playerData != null)
        {
            return playerData.CalculateXPRequired(currentLevel);
        }
        return 999999;
    }

    private void Die()
    {
        OnDeath?.Invoke();
        if (SoundManager.Instance != null)
             SoundManager.Instance.PlaySound("Player_Death", transform.position);
    }

    public void SetInvincible(float duration)
    {
        isInvincible = true;
        invincibilityTimer = duration;
    }
    
    public bool IsInvincible() => isInvincible;

    // Helper for UI to manually refresh
    public void UpdateHealthUI()
    {
        OnHealthChanged?.Invoke(currentHP);
    }
}

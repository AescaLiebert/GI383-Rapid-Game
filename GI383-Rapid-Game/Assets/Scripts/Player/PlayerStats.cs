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
    public int attackDamage;
    
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
        maxHP = playerData.baseMaxHP + ((currentLevel - 1) * playerData.hpGainPerLevel);
        attackDamage = playerData.baseAttackDamage + ((currentLevel - 1) * playerData.attackGainPerLevel);
        
        currentHP = maxHP;
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || IsHit) return;
        if (currentHP <= 0) return;

        currentHP -= damage;
        OnHealthChanged?.Invoke(currentHP);
        OnDamageTaken?.Invoke();

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
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        
        CheckLevelUp();
        
        int xpToNext = GetXPToNextLevel();
        OnXPChanged?.Invoke(currentXP, xpToNext);
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
        maxHP += playerData.hpGainPerLevel;
        attackDamage += playerData.attackGainPerLevel;
        
        // Heal on level up?
        currentHP = maxHP;
        
        OnLevelUp?.Invoke(currentLevel);
        OnHealthChanged?.Invoke(currentHP);
        
        Debug.Log($"Level Up! New Level: {currentLevel}. HP: {maxHP}, Atk: {attackDamage}");
    }

    public int GetXPToNextLevel()
    {
        if (playerData.levelXPRequirements == null || playerData.levelXPRequirements.Length == 0) return 999999;
        
        int index = currentLevel - 1;
        if (index < playerData.levelXPRequirements.Length)
        {
            return playerData.levelXPRequirements[index];
        }
        else
        {
            // Cap level or use formula for high levels
            return playerData.levelXPRequirements[playerData.levelXPRequirements.Length - 1] + (index * 500);
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
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

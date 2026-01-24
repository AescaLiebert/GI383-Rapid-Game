using UnityEngine;

public abstract class SkillBase : MonoBehaviour
{
    [Header("Basic Settings")]
    public string skillName;
    public float baseCooldown;
    public float baseDamage; // สำหรับสกิลที่ไม่มีดาเมจให้ใส่ 0

    protected float lastUsedTime = -999f;
    protected Player player;

    public virtual void Initialize(Player _player)
    {
        this.player = _player;
    }

    public bool CanUse()
    {
        return Time.time >= lastUsedTime + baseCooldown;
    }

    public void Use()
    {
        if (CanUse())
        {
            lastUsedTime = Time.time;
            Activate();
        }
    }

    protected abstract void Activate();

    // สูตรคำนวณ Damage ตามเลเวลที่เชื่อมกับ PlayerStats 
    protected int GetLevelScaledDamage()
    {
        if (player == null || player.stats == null) return Mathf.RoundToInt(baseDamage);

        float level = player.stats.currentLevel;       
        float multiplier = 1f + (level / 10f);
        return Mathf.RoundToInt(baseDamage * multiplier);
    }
}
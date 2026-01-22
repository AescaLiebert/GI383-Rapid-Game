using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerStats stats;

    [Header("Skills")]
    // Example: List of unlocked skills
    public List<string> unlockedSkills = new List<string>();

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        if (stats != null)
        {
             stats.OnLevelUp += OnLevelUp;
        }
    }

    void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnLevelUp -= OnLevelUp;
        }
    }

    private void OnLevelUp(int newLevel)
    {
        Debug.Log($"SkillManager: Checking unlocks for level {newLevel}");
        
        // Example Unlocks
        if (newLevel == 2)
        {
            UnlockSkill("DoubleJump"); // Hypothetical
            Debug.Log("Unlocked: Double Jump!");
        }
        else if (newLevel == 3)
        {
            UnlockSkill("HeavyAttack");
            Debug.Log("Unlocked: Heavy Attack!");
        }
        else if (newLevel == 5)
        {
             UnlockSkill("Ultimate");
             Debug.Log("Unlocked: Ultimate!");
        }
    }

    public void UnlockSkill(string skillName)
    {
        if (!unlockedSkills.Contains(skillName))
        {
            unlockedSkills.Add(skillName);
            // Notify UI or other systems
        }
    }

    public bool HasSkill(string skillName)
    {
        return unlockedSkills.Contains(skillName);
    }
}

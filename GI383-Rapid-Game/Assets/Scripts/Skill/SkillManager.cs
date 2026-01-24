using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [Header("Dependencies")]
    public Player player;

    [Header("Skills Database")]
    public SkillBase skill1_ThrowKnife; 
    public SkillBase skill2_Invisible;
    public SkillBase skill3_Ultimate;

    
    private bool unlockSkill1 = false;
    private bool unlockSkill2 = false;
    private bool unlockSkill3 = false;

    void Start()
    {
        if (player == null) player = GetComponent<Player>();

        
        if (skill1_ThrowKnife) skill1_ThrowKnife.Initialize(player);
        if (skill2_Invisible) skill2_Invisible.Initialize(player);
        if (skill3_Ultimate) skill3_Ultimate.Initialize(player);

        
        if (player.stats != null)
        {
            player.stats.OnLevelUp += CheckUnlock;
            CheckUnlock(player.stats.currentLevel); 
        }
    }

    void OnDestroy()
    {
        if (player != null && player.stats != null)
            player.stats.OnLevelUp -= CheckUnlock;
    }

    private void CheckUnlock(int level)
    {
        if (level >= 2 && !unlockSkill1) { unlockSkill1 = true; Debug.Log("Unlocked: Throw Knife!"); }
        if (level >= 5 && !unlockSkill2) { unlockSkill2 = true; Debug.Log("Unlocked: Invisible!"); }
        if (level >= 10 && !unlockSkill3) { unlockSkill3 = true; Debug.Log("Unlocked: Ultimate!"); }
    }

    public void TryUseSkill(int slotIndex)
    {
        switch (slotIndex)
        {
            case 1:
                if (unlockSkill1 && skill1_ThrowKnife) skill1_ThrowKnife.Use();
                else Debug.Log("Skill 1 ยังไม่ปลดล็อก (ต้อง LV.2)");
                break;
            case 2:
                if (unlockSkill2 && skill2_Invisible) skill2_Invisible.Use();
                else Debug.Log("Skill 2 ยังไม่ปลดล็อก (ต้อง LV.5)");
                break;
            case 3:
                if (unlockSkill3 && skill3_Ultimate) skill3_Ultimate.Use();
                else Debug.Log("Skill 3 ยังไม่ปลดล็อก (ต้อง LV.10)");
                break;
        }
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerDataSO : ScriptableObject
{
    [Header("Base Stats")]
    public int baseMaxHP = 10;
    public float baseMoveSpeed = 5f;
    public float baseJumpForce = 10f;
    public float baseDashSpeed = 20f;
    public int baseAttackDamage = 1;

    [Header("Leveling")]
    [Tooltip("XP required for each level. Index 0 = XP for Level 2, etc.")]
    public int[] levelXPRequirements = new int[] 
    { 
        100, 250, 500, 1000, 2000 
    };

    [Header("Growth")]
    public int hpGainPerLevel = 2;
    public int attackGainPerLevel = 1;
}

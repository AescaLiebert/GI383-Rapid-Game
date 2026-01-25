using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image skillIcon;       // รูปไอคอนสกิล
    public Image cooldownOverlay; // รูปสีดำจางๆ (ต้องตั้ง Image Type = Filled)
    public GameObject lockIcon;   // รูปกุญแจล็อค (ถ้ามี)

    private SkillBase linkedSkill;

    void Awake()
    {
        // เริ่มต้นให้ปิดไอคอนสกิลและเปิดตัวล็อคไว้
        if (skillIcon) skillIcon.enabled = false;
        if (cooldownOverlay) cooldownOverlay.fillAmount = 0;
        if (lockIcon) lockIcon.SetActive(true);
    }

    void Update()
    {
        if (linkedSkill == null) return;

        // อัปเดตวงกลม Cooldown
        if (cooldownOverlay != null)
        {
            if (linkedSkill.IsOnCooldown())
            {
                cooldownOverlay.fillAmount = linkedSkill.GetCooldownRatio();
            }
            else
            {
                cooldownOverlay.fillAmount = 0f;
            }
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกตอนอนิเมชั่นบินมาถึง
    public void UnlockAndSetSkill(SkillBase skill, Sprite iconSprite)
    {
        linkedSkill = skill;

        if (lockIcon) lockIcon.SetActive(false); // ปิดกุญแจ

        if (skillIcon)
        {
            skillIcon.enabled = true;
            skillIcon.sprite = iconSprite; // ใส่รูป
            skillIcon.color = Color.white;
        }
    }
}
using UnityEngine;

public class ThrowKnifeSkill : SkillBase
{
    public GameObject knifePrefab;
    public float speed = 20f;

    protected override void Activate()
    {
        // ตรวจสอบทิศทางจาก SpriteRenderer ของ Player
        float dir = player.animHandler.GetComponentInChildren<SpriteRenderer>().flipX ? 1 : -1;
        // หมายเหตุ: เช็คดีๆว่าในเกมคุณ flipX = true คือซ้ายหรือขวา ถ้าปกติหันขวาแล้ว flipX=false ให้แก้ตรงนี้

        GameObject knife = Instantiate(knifePrefab, player.transform.position, Quaternion.identity);
        int dmg = GetLevelScaledDamage();

        // ส่งค่า Damage และทิศทางไปให้มีด
        knife.GetComponent<KnifeProjectile>().Setup(dir, speed, dmg);
    }
}
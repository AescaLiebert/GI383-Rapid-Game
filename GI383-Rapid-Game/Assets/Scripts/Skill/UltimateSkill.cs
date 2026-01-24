using UnityEngine;
using System.Collections;

public class UltimateSkill : SkillBase
{
    public int hitCount = 4;
    public float interval = 0.2f;
    public Vector2 hitBoxSize = new Vector2(5, 3);
    public LayerMask enemyLayer; // อย่าลืมตั้งค่าใน Inspector

    protected override void Activate()
    {
        StartCoroutine(UltimateRoutine());
    }

    private IEnumerator UltimateRoutine()
    {
        // หยุดขยับชั่วคราว (ถ้าต้องการ)
        player.movement.CanMove = false;
        player.stats.SetInvincible(hitCount * interval + 1f); // อมตะระหว่างใช้อัลติ

        int damagePerHit = GetLevelScaledDamage() * 10; // สูตร: 10 * Multiplier

        for (int i = 0; i < hitCount; i++)
        {
            // 1. เคลื่อนที่ไปข้างหน้าเล็กน้อย
            float dir = player.animHandler.GetComponentInChildren<SpriteRenderer>().flipX ? 1 : -1;
            player.transform.Translate(Vector2.right * dir * 1f);

            // 2. หาศัตรูในระยะ
            Collider2D[] enemies = Physics2D.OverlapBoxAll(player.transform.position, hitBoxSize, 0, enemyLayer);

            foreach (var e in enemies)
            {
                // Snap ศัตรูมาตรงหน้า
                e.transform.position = Vector2.MoveTowards(e.transform.position, player.transform.position + (Vector3.right * dir), 10f);

                // ทำดาเมจ
                Enemy enemyScript = e.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    // Hit สุดท้ายให้ Stun 2 วิ
                    float stun = (i == hitCount - 1) ? 2f : 0.1f;
                    // ใช้ KnockbackVector (x, y)
                    enemyScript.TakeDamage(damagePerHit); // *อาจต้องแก้ Enemy.cs ให้รับ parameter Stun ถ้ายังไม่มี
                }
            }

            // เล่น Effect หรือเสียงที่นี่
            yield return new WaitForSeconds(interval);
        }

        player.movement.CanMove = true;
    }
}
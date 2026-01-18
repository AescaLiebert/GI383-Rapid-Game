using UnityEngine;

public class Sword : Weapon
{
    [Header("Sword Settings")]
    public Transform attackPoint; // จุดอ้างอิงวงกลม (จุดล่องหน)
    public Vector2 attackArea = new Vector2(1f, 0.5f);
    public LayerMask enemyLayers; // เลเยอร์ของศัตรู (เพื่อไม่ให้ฟันโดนพวกเดียวกัน)
       
    

    protected override void PerformAttack()
    {
        {
            // 1. เล่น Animation ฟัน (ถ้ามี)
            // animator.SetTrigger("Attack");

            // 2. ตรวจจับศัตรูทั้งหมดที่อยู่ในวงกลม
            // OverlapCircleAll(ตำแหน่ง, รัศมี, เลเยอร์ที่จะตรวจ)
            // เพิ่ม attackPoint.eulerAngles.z เป็นตัวที่ 3
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackArea, attackPoint.eulerAngles.z, enemyLayers);

            Debug.Log("Attack");
            // 3. วนลูปสั่งลดเลือดศัตรูทุกตัวที่โดน
            foreach (Collider2D enemy in hitEnemies)
            {
                Debug.Log("ฟันโดน " + enemy.name);

                // ถ้าศัตรูมีสคริปต์ Character หรือ Enemy ให้เรียกฟังก์ชันเจ็บ
                // enemy.GetComponent<Character>()?.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        // บรรทัดนี้จะช่วยให้สี่เหลี่ยมหมุนตามดาบของเราได้
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);

        // วาดสี่เหลี่ยม (Cube) ที่ตำแหน่ง 0,0 (เพราะเราย้าย matrix มาที่ attackPoint แล้ว)
        Gizmos.DrawWireCube(Vector3.zero, attackArea);
    }
}

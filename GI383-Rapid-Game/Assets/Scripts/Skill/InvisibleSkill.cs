using UnityEngine;
using System.Collections;

public class InvisibleSkill : SkillBase
{
    public float duration = 4f;

    protected override void Activate()
    {
        StartCoroutine(InvisibleRoutine());
    }

    private IEnumerator InvisibleRoutine()
    {
       
        player.stats.SetInvincible(duration);
        player.IsInvisible = true;

        
        var sr = player.animHandler.GetComponentInChildren<SpriteRenderer>();
        Color originalColor = sr.color;
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f); // จางลง

        

        yield return new WaitForSeconds(duration);

        
        sr.color = originalColor;
        player.IsInvisible = false;
    }
}
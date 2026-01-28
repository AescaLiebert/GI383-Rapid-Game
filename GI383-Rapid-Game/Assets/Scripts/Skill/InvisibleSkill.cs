using UnityEngine;
using System.Collections;

public class InvisibleSkill : SkillBase
{
    public float duration = 4f;
    [SerializeField] private string invisibleSound = "Player_Invisible";

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

        if (SoundManager.Instance != null && !string.IsNullOrEmpty(invisibleSound))
        {
            SoundManager.Instance.PlaySound(invisibleSound, player.transform.position);
        }


        yield return new WaitForSeconds(duration);

        
        sr.color = originalColor;
        player.IsInvisible = false;
    }
}
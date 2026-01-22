using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public HealthBarUI healthBarUI;
    public DeathSequenceController deathSequenceController;
    public GameObject deathPanel; // Fallback if no sequence controller

    void Start()
    {
        // Auto-find references if not assigned
        if (player == null) player = FindFirstObjectByType<Player>();
        if (healthBarUI == null) healthBarUI = FindFirstObjectByType<HealthBarUI>();
        // deathSequenceController might be optional or scene specific
        
        if (player != null && player.stats != null)
        {
            // Subscribe to events
            player.stats.OnHealthChanged += HandleHealthChanged;
            player.stats.OnDeath += HandleDeath;
            
            // Initialize UI
            if (healthBarUI != null)
            {
                // Ensure stats are ready or just use current
                // PlayerStats initializes in Start, so we might need to sync.
                // If GameManager runs after Player, this is fine. 
                // Using currentHP is safest.
                // We also need MaxHP for the UI to know hearts count?
                // HealthBarUI currently gets MaxHP from Player in its Start.
                // We should let HealthBarUI keep finding Player if it wants, OR we explicitly init it.
                // For now, HealthBarUI's UpdateHealth(int) is sufficient.
                HandleHealthChanged(player.stats.currentHP);
            }
        }
    }

    void OnDestroy()
    {
        if (player != null && player.stats != null)
        {
            player.stats.OnHealthChanged -= HandleHealthChanged;
            player.stats.OnDeath -= HandleDeath;
        }
    }

    private void HandleHealthChanged(int currentHP)
    {
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(currentHP);
        }
    }

    private void HandleDeath()
    {
        Debug.Log("GameManager: Player Died.");
        
        if (deathSequenceController != null)
        {
            deathSequenceController.StartDeathSequence();
        }
        else if (deathPanel != null)
        {
             deathPanel.SetActive(true);
             Time.timeScale = 0f;
        }
    }
}

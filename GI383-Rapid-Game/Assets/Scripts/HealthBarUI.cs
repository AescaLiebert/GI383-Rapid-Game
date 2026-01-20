using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Heart Sprites")]
    [Tooltip("Sprite for a full heart (2 HP)")]
    public Sprite fullHeartSprite;
    
    [Tooltip("Sprite for a half heart (1 HP)")]
    public Sprite halfHeartSprite;
    
    [Tooltip("Sprite for an empty heart (0 HP)")]
    public Sprite emptyHeartSprite;

    [Header("Heart UI Elements")]
    [Tooltip("Array of 5 heart Image components in the UI")]
    public Image[] heartImages;

    [Header("Player Reference")]
    [Tooltip("Reference to the Player component to get max HP")]
    public Player player;

    private int maxHP; // Cached max HP from Player

    private void Start()
    {
        // Get max HP from Player
        if (player != null)
        {
            maxHP = player.HP;
        }
        else
        {
            Debug.LogError("HealthBarUI: Player reference is not assigned! Cannot determine max HP.");
            maxHP = 10; // Fallback default
        }

        // Validate setup
        if (heartImages == null || heartImages.Length != 5)
        {
            Debug.LogError("HealthBarUI: heartImages array must contain exactly 5 Image components!");
            return;
        }

        if (fullHeartSprite == null || halfHeartSprite == null || emptyHeartSprite == null)
        {
            Debug.LogWarning("HealthBarUI: One or more heart sprites are not assigned. Please assign them in the Inspector.");
        }
    }

    /// <summary>
    /// Updates the health bar display based on current HP
    /// </summary>
    /// <param name="currentHP">Current player HP (0-10)</param>
    public void UpdateHealth(int currentHP)
    {
        // Clamp HP to valid range
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        // Each heart represents 2 HP
        // We need to determine the state of each of the 5 hearts
        for (int i = 0; i < heartImages.Length; i++)
        {
            // Calculate the HP range this heart represents
            // Heart 0: HP 10-9 (index 0 represents rightmost/highest HP)
            // Heart 1: HP 8-7
            // Heart 2: HP 6-5
            // Heart 3: HP 4-3
            // Heart 4: HP 2-1
            
            int heartIndex = i;
            int minHPForThisHeart = maxHP - (heartIndex * 2) - 2; // Minimum HP for this heart to show anything
            int maxHPForThisHeart = maxHP - (heartIndex * 2);     // Maximum HP this heart represents

            if (currentHP >= maxHPForThisHeart)
            {
                // Full heart: currentHP is at or above the max for this heart
                heartImages[i].sprite = fullHeartSprite;
            }
            else if (currentHP == maxHPForThisHeart - 1)
            {
                // Half heart: currentHP is exactly 1 less than max for this heart
                heartImages[i].sprite = halfHeartSprite;
            }
            else
            {
                // Empty heart: currentHP is below this heart's range
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }

    /// <summary>
    /// Alternative method with detailed logging for debugging
    /// </summary>
    public void UpdateHealthWithDebug(int currentHP)
    {
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        Debug.Log($"HealthBarUI: Updating health display for {currentHP} HP");

        for (int i = 0; i < heartImages.Length; i++)
        {
            int heartIndex = i;
            int minHPForThisHeart = maxHP - (heartIndex * 2) - 2;
            int maxHPForThisHeart = maxHP - (heartIndex * 2);

            string heartState = "";
            if (currentHP >= maxHPForThisHeart)
            {
                heartImages[i].sprite = fullHeartSprite;
                heartState = "FULL";
            }
            else if (currentHP == maxHPForThisHeart - 1)
            {
                heartImages[i].sprite = halfHeartSprite;
                heartState = "HALF";
            }
            else
            {
                heartImages[i].sprite = emptyHeartSprite;
                heartState = "EMPTY";
            }

            Debug.Log($"  Heart {i}: Range [{minHPForThisHeart + 1}-{maxHPForThisHeart}] HP -> {heartState}");
        }
    }
}

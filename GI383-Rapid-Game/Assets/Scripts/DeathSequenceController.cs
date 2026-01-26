using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class DeathSequenceController : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("The panel containing the restart button")]
    public GameObject deathPanel; 
    
    [Tooltip("Image used for full screen flickering effect")]
    public Image flickerImage;    
    
    [Tooltip("The Video Player component for Game Over video")]
    public VideoPlayer gameOverVideo;
    
    [Tooltip("The RawImage component to display the video on")]
    public RawImage videoDisplayImage; 

    [Header("CRT Input Settings")]
    [Tooltip("The RawImage that displays the final CRT output (User Click Area).")]
    public RawImage crtScreenRawImage;
    [Tooltip("The Camera that renders the UI to the RenderTexture.")]
    public Camera crtOutputCamera; 

    [Header("Settings")]
    public float flickerDuration = 0.5f;
    public float flickerSpeed = 0.05f;
    public float delayBeforeButton = 10.0f; // Time to wait after video starts before showing restart button
    
    [Header("Death Animation Settings")]
    [Tooltip("Time to wait for player death animation before freezing time.")]
    public float deathAnimationDuration = 1.5f;

    [Header("Glitch Settings")]
    public int glitchBarCount = 20;
    public Color[] glitchColors = new Color[] 
    { 
        Color.black, Color.black, new Color(1f, 0f, 0.4f), new Color(0.6f, 0f, 1f), Color.red 
    };
    private List<Image> glitchBars = new List<Image>();

    public void StartDeathSequence()
    {
        Debug.Log("DeathSequenceController: Starting death sequence...");
        this.gameObject.SetActive(true);
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        // 0. Wait for Player Death Animation
        // Ensure time is running so animation plays
        Time.timeScale = 1f; 
        Debug.Log($"DeathSequence: Waiting {deathAnimationDuration}s for death animation...");
        yield return new WaitForSeconds(deathAnimationDuration);

        // 1. TIMESTOP & Silence
        Time.timeScale = 0f; // Stop the game action immediately
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopAllSounds();
        }

        // 2. Glitch Freeze Effect (Replaces Desperate Flickering)
        if (flickerImage != null)
        {
            flickerImage.gameObject.SetActive(true);
            
            // Initialize glitch bars if needed
            if (glitchBars.Count == 0) CreateGlitchBars();

            float timer = 0;
            
            while(timer < flickerDuration)
            {
                UpdateGlitchEffect();
                yield return new WaitForSecondsRealtime(flickerSpeed);
                timer += flickerSpeed;
            }
            
            // Clean up glitch visibility
            foreach(var bar in glitchBars) bar.gameObject.SetActive(false);
            flickerImage.color = Color.black; 
            flickerImage.gameObject.SetActive(false); 
        }

        // 2. Play Video (With Auto Render Texture Setup)
        if (videoDisplayImage != null && gameOverVideo != null)
        {
            videoDisplayImage.gameObject.SetActive(true);
            
            // Prepare Video
            gameOverVideo.Stop();
            gameOverVideo.renderMode = VideoRenderMode.RenderTexture;
            gameOverVideo.isLooping = false; // Ensure it doesn't loop

            // Create a temporary Render Texture if one doesn't exist or isn't assigned
            if (gameOverVideo.targetTexture == null)
            {
                RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
                renderTexture.name = "VideoRenderTexture";
                gameOverVideo.targetTexture = renderTexture;
            }

            // Assign the Render Texture to the Raw Image
            videoDisplayImage.texture = gameOverVideo.targetTexture;
            // Set Color to white to ensure texture is visible
            videoDisplayImage.color = Color.white;
            videoDisplayImage.raycastTarget = false; // Fix: Ensure Video overlay doesn't block Death Panel clicks

            gameOverVideo.Play();
            
            // Wait while video plays
            yield return new WaitForSecondsRealtime(delayBeforeButton);
        }
        /*else
        {
            // If no video, just wait a bit
            yield return new WaitForSecondsRealtime(1.0f);
        }*/

        // 3. Show Restart Button (Death Panel)
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);

            // Fix: Setup Input for Screen Space - Camera rendered to Texture
            // We need to swap the GraphicRaycaster for our custom RenderTextureRaycaster
            SetupRenderTextureRaycaster(deathPanel);
        }
    }

    private void SetupRenderTextureRaycaster(GameObject panel)
    {
        Canvas canvas = panel.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // 1. Disable default Graphic Raycaster (it fails on RTs)
        GraphicRaycaster defaultRaycaster = canvas.GetComponent<GraphicRaycaster>();
        if (defaultRaycaster != null && !(defaultRaycaster is Game.UI.RenderTextureRaycaster))
        {
            defaultRaycaster.enabled = false;
        }

        // 2. Add or Get our Custom Raycaster
        Game.UI.RenderTextureRaycaster customRaycaster = canvas.GetComponent<Game.UI.RenderTextureRaycaster>();
        if (customRaycaster == null)
        {
            customRaycaster = canvas.gameObject.AddComponent<Game.UI.RenderTextureRaycaster>();
        }

        // 3. Configure dependencies
        // Priority 1: Manual Assignment
        if (crtOutputCamera != null && crtScreenRawImage != null)
        {
            customRaycaster.renderTextureCamera = crtOutputCamera;
            customRaycaster.screenRawImage = crtScreenRawImage;
            return;
        }

        // Priority 2: Auto-Find CRTCameraSetup (Fallback)
        CRTCameraSetup crtSetup = FindFirstObjectByType<CRTCameraSetup>();
        if (crtSetup != null)
        {
             customRaycaster.renderTextureCamera = crtSetup.targetCamera;
             customRaycaster.screenRawImage = crtSetup.GetComponent<RawImage>();
        }
        else
        {
            Debug.LogWarning("DeathSequence: Could not find CRT Setup (Manual or Auto). Input might fail. Please assign 'Crt Screen Raw Image' and 'Crt Output Camera' in the Inspector.");
        }
    }

    public void RestartGame()
    {
        // Clean up render texture if we created one (optional, but good practice)
        if (gameOverVideo != null && gameOverVideo.targetTexture != null)
        {
            gameOverVideo.targetTexture.Release();
        }

        Time.timeScale = 1f; // Ensure time is running again
        SceneManager.LoadScene("MainMenu");
    }

    private void CreateGlitchBars()
    {
        if (flickerImage == null) return;
        
        for (int i = 0; i < glitchBarCount; i++)
        {
            GameObject barObj = new GameObject($"GlitchBar_{i}");
            barObj.transform.SetParent(flickerImage.transform, false);
            Image barImg = barObj.AddComponent<Image>();
            barImg.raycastTarget = false;
            barImg.color = Color.black;
            barObj.SetActive(false);
            glitchBars.Add(barImg);
        }
    }

    private void UpdateGlitchEffect()
    {
        // Random Background Flicker
        if (Random.value > 0.7f)
             flickerImage.color = glitchColors[Random.Range(0, glitchColors.Length)];
        else
             flickerImage.color = new Color(0, 0, 0, Random.Range(0f, 0.5f)); 

        // Update Bars
        foreach (var bar in glitchBars)
        {
             if (Random.value > 0.6f) 
             {
                 bar.gameObject.SetActive(true);
                 RectTransform rect = bar.rectTransform;
                 
                 float yPos = Random.value;
                 float height = Random.Range(0.01f, 0.2f);
                 float xPos = Random.Range(0f, 0.9f);
                 float width = Random.Range(0.1f, 1f);

                 rect.anchorMin = new Vector2(xPos, yPos);
                 rect.anchorMax = new Vector2(xPos + width, yPos + height);
                 rect.offsetMin = Vector2.zero;
                 rect.offsetMax = Vector2.zero;

                 bar.color = glitchColors[Random.Range(0, glitchColors.Length)];
             }
             else
             {
                 bar.gameObject.SetActive(false);
             }
        }
    }
}

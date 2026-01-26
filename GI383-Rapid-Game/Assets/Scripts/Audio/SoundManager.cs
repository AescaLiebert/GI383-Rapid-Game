using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private SoundLibrary soundLibrary;
    //[SerializeField] private AudioSource sfxSourcePrefab; // Optional: Prefab for better control, or we generate them

    [SerializeField] private AudioSource bgmSource; // Dedicated AudioSource for BGM
    [SerializeField] private AudioSource sfxLoopSource; // Dedicated AudioSource for Looping SFX (e.g. Clock Tick)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (soundLibrary != null)
        {
            soundLibrary.Initialize();
        }

        // Initialize BGM Source if not assigned
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM_Source");
            bgmObj.transform.SetParent(this.transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        // Initialize SFX Loop Source if not assigned
        if (sfxLoopSource == null)
        {
            GameObject loopObj = new GameObject("SFX_Loop_Source");
            loopObj.transform.SetParent(this.transform);
            sfxLoopSource = loopObj.AddComponent<AudioSource>();
            sfxLoopSource.loop = true;
            sfxLoopSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// Plays background music. Switches track if different.
    /// </summary>
    public void PlayBGM(string soundName)
    {
        if (soundLibrary == null) return;

        SoundData? data = soundLibrary.GetSound(soundName);
        if (data == null) return;

        SoundData sound = data.Value;
        if (sound.clips == null || sound.clips.Count == 0) return;

        AudioClip clip = sound.clips[0]; // BGM usually just one clip

        // If already playing this clip, do nothing
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = sound.volume;
        bgmSource.pitch = sound.pitch; // Usually 1 for BGM
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// Plays a looping SFX (like a clock ticking). Prevents overlap by using a dedicated source.
    /// </summary>
    public void PlaySFXLoop(string soundName)
    {
        if (soundLibrary == null) return;

        SoundData? data = soundLibrary.GetSound(soundName);
        if (data == null) return;

        SoundData sound = data.Value;
        if (sound.clips == null || sound.clips.Count == 0) return;

        AudioClip clip = sound.clips[0];

        // If already playing this exact clip, just ensure it's playing and return (don't restart)
        if (sfxLoopSource.clip == clip && sfxLoopSource.isPlaying) return;

        sfxLoopSource.clip = clip;
        sfxLoopSource.volume = sound.volume;
        sfxLoopSource.pitch = sound.pitch;
        sfxLoopSource.Play();
    }

    public void StopSFXLoop()
    {
        if (sfxLoopSource != null)
        {
            sfxLoopSource.Stop();
            sfxLoopSource.clip = null; // Clear clip so next Play triggers fresh
        }
    }

    /// <summary>
    /// Plays a sound at the camera's position (2D non-spatial).
    /// </summary>
    public void PlaySound(string soundName)
    {
        // For 2D non-spatial sounds, we can just play at the listener's position
        // or use a dedicated 2D AudioSource on the manager itself.
        // For simplicity in this request, we'll treat it as "At positions" where position is camera/listener.
        if (Camera.main != null)
        {
            PlaySound(soundName, Camera.main.transform.position);
        }
    }

    /// <summary>
    /// Plays a sound at a specific 2D position.
    /// </summary>
    public void PlaySound(string soundName, Vector2 position)
    {
        if (soundLibrary == null)
        {
            Debug.LogWarning("SoundManager: SoundLibrary is missing!");
            return;
        }

        SoundData? data = soundLibrary.GetSound(soundName);
        if (data == null) return;
        
        SoundData sound = data.Value;

        if (sound.clips == null || sound.clips.Count == 0)
        {
             Debug.LogWarning($"SoundManager: Sound '{soundName}' has no clips assigned!");
             return;
        }

        // Pick a random clip from the list
        AudioClip clipToPlay = sound.clips[Random.Range(0, sound.clips.Count)];
        if (clipToPlay == null)
        {
             Debug.LogWarning($"SoundManager: One of the clips in '{soundName}' is missing (null)!");
             return;
        }

        // Create a temporary GameObject for the sound
        GameObject soundObj = new GameObject("TempAudio_" + soundName);
        soundObj.transform.position = position;

        AudioSource source = soundObj.AddComponent<AudioSource>();
        source.clip = clipToPlay;
        source.volume = sound.volume;
        
        // Randomize pitch
        if (sound.pitchVariance > 0f)
        {
            source.pitch = sound.pitch + Random.Range(-sound.pitchVariance, sound.pitchVariance);
        }
        else
        {
            source.pitch = sound.pitch;
        }

        // Play and Destroy
        source.Play();
        Destroy(soundObj, clipToPlay.length / source.pitch); // Adjust destroy time for pitch
    }

    /// <summary>
    /// Stops all currently playing sounds (BGM, Loops, and One-Shots).
    /// </summary>
    public void StopAllSounds()
    {
        // Stop BGM
        StopBGM();

        // Stop SFX Loop
        StopSFXLoop();

        // Find and destroy all temporary audio objects
        // (They are named "TempAudio_" + soundName)
        
        // Option 1: Find by name pattern (slower but specific)
        // Option 2: Find all AudioSources in the scene and stop them (broad but effective)
        // Given the requirement "Mean all Sound stop play", Option 2 is safer for "Silence".
        
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var source in allAudioSources)
        {
            if (source != null)
            {
                source.Stop();
                // Optionally destroy if it's a temp object
                if (source.gameObject.name.StartsWith("TempAudio_"))
                {
                    Destroy(source.gameObject);
                }
            }
        }
    }
}

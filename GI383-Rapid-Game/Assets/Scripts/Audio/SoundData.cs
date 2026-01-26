using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct SoundData
{
    public string soundName;
    public List<AudioClip> clips;
    [Range(0f, 1f)]
    public float volume;
    [Range(0.1f, 3f)]
    public float pitch;
    [Range(0f, 1f)]
    public float pitchVariance;

    // Constructor for default values
    public SoundData(string name, AudioClip audioClip)
    {
        soundName = name;
        clips = new List<AudioClip> { audioClip };
        volume = 1f;
        pitch = 1f;
        pitchVariance = 0f;
    }
}

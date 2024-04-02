using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PlaySF : MonoBehaviour
{
    //public static PlaySF instance; // Singleton instance

    public List<AudioClip> soundClips; // List of sound clips
    private Dictionary<string, AudioClip> soundClipDict; // Dictionary to map sound clip names to audio clips

    private AudioSource ass;

    void Awake()
    {
        // Initialize the dictionary with sound clip names and audio clips
        soundClipDict = new Dictionary<string, AudioClip>();
        foreach (AudioClip clip in soundClips)
        {
            soundClipDict.Add(clip.name, clip);
        }

        ass = gameObject.GetComponent<AudioSource>();
    }

    // Function to play a sound clip by index
    public void PlaySoundByIndex(int index)
    {
        if (index >= 0 && index < soundClips.Count)
        {
            ass.PlayOneShot(soundClips[index]);
        }
        else
        {
            Debug.LogWarning("Invalid sound clip index.");
        }
    }

    // Function to play a sound clip by name
    public void PlaySoundByName(string name)
    {
        if (soundClipDict.ContainsKey(name))
        {
            ass.PlayOneShot(soundClipDict[name]);
        }
        else
        {
            Debug.LogWarning("Sound clip with name " + name + " not found.");
        }
    }
}

using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume",
            Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);
    }

    public void SetSoundVolume(float volume)
    {
        audioMixer.SetFloat("SoundVolume",
            Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);
    }
}
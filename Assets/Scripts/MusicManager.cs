using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;

    private void Start()
    {
        if (musicSource == null) return;

        musicSource.loop = true;
        if (musicSource.clip != null)
            musicSource.Play();
    }

    public void SetVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(volume);
    }

    public void ToggleMusic()
    {
        if (musicSource != null)
            musicSource.mute = !musicSource.mute;
    }
}

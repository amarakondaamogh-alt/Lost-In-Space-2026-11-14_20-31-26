using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource audioSource;

    public AudioClip jumpSound;
    public AudioClip bounceSound;
    public AudioClip crystalSound;
    public AudioClip switchSound;
    public AudioClip damageSound;
    public AudioClip winSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayJump()
    {
        PlaySound(jumpSound);
    }

    public void PlayBounce()
    {
        PlaySound(bounceSound);
    }

    public void PlayCrystal()
    {
        PlaySound(crystalSound);
    }

    public void PlaySwitch()
    {
        PlaySound(switchSound);
    }

    public void PlayDamage()
    {
        PlaySound(damageSound);
    }

    public void PlayWin()
    {
        PlaySound(winSound);
    }
}
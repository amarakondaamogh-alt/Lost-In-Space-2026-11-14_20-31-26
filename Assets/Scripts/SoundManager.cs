using UnityEngine;

// Plays the game's sound effects. Any clip you leave empty is generated in code
// (see ProceduralAudio), so the game always has sound. Assign your own clips to override.
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

    [Range(0f, 1f)] public float volume = 0.7f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        if (jumpSound == null) jumpSound = ProceduralAudio.Jump();
        if (bounceSound == null) bounceSound = ProceduralAudio.Bounce();
        if (crystalSound == null) crystalSound = ProceduralAudio.Crystal();
        if (switchSound == null) switchSound = ProceduralAudio.Switch();
        if (damageSound == null) damageSound = ProceduralAudio.Damage();
        if (winSound == null) winSound = ProceduralAudio.Win();
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayJump() { PlaySound(jumpSound); }
    public void PlayBounce() { PlaySound(bounceSound); }
    public void PlayCrystal() { PlaySound(crystalSound); }
    public void PlaySwitch() { PlaySound(switchSound); }
    public void PlayDamage() { PlaySound(damageSound); }
    public void PlayWin() { PlaySound(winSound); }
}

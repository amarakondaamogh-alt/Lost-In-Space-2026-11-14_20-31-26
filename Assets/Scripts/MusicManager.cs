using UnityEngine;

// Background music. One copy is created automatically when the game starts and keeps
// playing across all scenes. Assign your own clip on an AudioSource to replace the
// built-in generated tune. Press M to mute / unmute.
public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;
    [Range(0f, 1f)] public float volume = 0.5f;

    private static MusicManager instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (instance != null || FindAnyObjectByType<MusicManager>() != null) return;

        var go = new GameObject("Music Manager");
        go.AddComponent<MusicManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (instance != this) return;

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = volume;

        // Your song: Assets/Audio/Music/Resources/SpaceCadet.mp3. Falls back to the built-in tune if it's missing.
        if (musicSource.clip == null)
            musicSource.clip = Resources.Load<AudioClip>("SpaceCadet");
        if (musicSource.clip == null)
            musicSource.clip = ProceduralAudio.Music();

        musicSource.Play();
    }

    private void Update()
    {
        if (GameInput.MutePressed())
            ToggleMusic();
    }

    public void SetVolume(float v)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(v);
    }

    public void ToggleMusic()
    {
        if (musicSource != null)
            musicSource.mute = !musicSource.mute;
    }
}

using UnityEngine;

// Persistent audio manager. Auto-spawns once from Resources, survives scene loads,
// and a singleton guard keeps only one so music plays continuously across levels.
// Music and SFX MUST use two separate AudioSources.
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("Music")]
    [SerializeField] AudioClip music;
    [SerializeField] AudioSource musicSource;        // dedicated music source (Loop on)

    [Header("SFX")]
    [SerializeField] AudioSource sfxSource;          // SEPARATE source from music (Loop off)
    [SerializeField] AudioClip shootClip;
    [SerializeField] AudioClip explosionClip;
    [SerializeField] AudioClip tankDestroyedClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject prefab = Resources.Load<GameObject>("GameAudio");
        if (prefab != null) Instantiate(prefab);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource != null && music != null)
        {
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayShoot()         => PlaySfx(shootClip);
    public void PlayExplosion()     => PlaySfx(explosionClip);
    public void PlayTankDestroyed() => PlaySfx(tankDestroyedClip);

    void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip);
    }

    // Mute is independent of play state, so firing SFX can't disturb it.
    public void SetMusicEnabled(bool on)
    {
        if (musicSource != null) musicSource.mute = !on;
    }

    public bool IsMusicOn => musicSource != null && !musicSource.mute;
}
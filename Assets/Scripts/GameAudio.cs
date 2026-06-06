using UnityEngine;

// Persistent audio manager. It auto-spawns once at startup from a prefab in a
// Resources folder, survives every scene load, and a singleton guard ensures only
// one ever exists — so music plays continuously and never restarts between levels.
// Other scripts call GameAudio.Instance?.PlayShoot() etc.
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("Music")]
    [SerializeField] AudioClip music;
    [SerializeField] AudioSource musicSource;        // Loop on, Play On Awake off

    [Header("SFX")]
    [SerializeField] AudioSource sfxSource;          // Loop off
    [SerializeField] AudioClip shootClip;
    [SerializeField] AudioClip explosionClip;
    [SerializeField] AudioClip tankDestroyedClip;

    // Spawn the manager before the first scene loads, regardless of entry scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject prefab = Resources.Load<GameObject>("GameAudio");
        if (prefab != null) Instantiate(prefab);
    }

    void Awake()
    {
        // Singleton: keep the first instance, destroy any later duplicate.
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

    // Wired to the title screen's music toggle later.
    public void SetMusicEnabled(bool on)
    {
        if (musicSource == null) return;
        if (on && !musicSource.isPlaying) musicSource.Play();
        else if (!on) musicSource.Pause();
    }

    public bool IsMusicOn => musicSource != null && musicSource.isPlaying;
}
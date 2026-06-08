using UnityEngine;

// Persistent audio manager. Auto-spawns once from Resources, survives scene loads,
// and a singleton guard keeps only one. Music and SFX use two separate AudioSources.
//
// Volume model: musicVolume / sfxVolume are the master levels (applied to the
// sources at runtime, overriding their component sliders). Each SFX also has its
// own per-clip scale that multiplies on top of the master, so one sound can sit
// lower than the rest without dragging everything down.
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("Music")]
    [SerializeField] AudioClip music;
    [SerializeField] AudioSource musicSource;            // dedicated music source (Loop on)
    [Range(0f, 1f)][SerializeField] float musicVolume = 0.5f;

    [Header("SFX (master)")]
    [SerializeField] AudioSource sfxSource;              // SEPARATE source from music (Loop off)
    [Range(0f, 1f)][SerializeField] float sfxVolume = 0.5f;

    [Header("SFX clips + per-clip scale")]
    [SerializeField] AudioClip shootClip;
    [Range(0f, 1f)][SerializeField] float shootVolume = 1f;
    [SerializeField] AudioClip explosionClip;            // the Impact sound
    [Range(0f, 1f)][SerializeField] float explosionVolume = 0.5f;
    [SerializeField] AudioClip tankDestroyedClip;
    [Range(0f, 1f)][SerializeField] float tankDestroyedVolume = 1f;

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

        if (sfxSource != null) sfxSource.volume = sfxVolume;

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
            if (music != null)
            {
                musicSource.clip = music;
                musicSource.loop = true;
                musicSource.Play();
            }
        }
    }

    public void PlayShoot()         => PlaySfx(shootClip, shootVolume);
    public void PlayExplosion()     => PlaySfx(explosionClip, explosionVolume);
    public void PlayTankDestroyed() => PlaySfx(tankDestroyedClip, tankDestroyedVolume);

    // Effective loudness = sfxSource.volume (master) * volumeScale (this clip).
    void PlaySfx(AudioClip clip, float volumeScale)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip, volumeScale);
    }

    // Mute is independent of play state, so firing SFX can't disturb it.
    public void SetMusicEnabled(bool on)
    {
        if (musicSource != null) musicSource.mute = !on;
    }

    public bool IsMusicOn => musicSource != null && !musicSource.mute;
}
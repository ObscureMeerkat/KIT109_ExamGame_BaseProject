using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent wind option. Auto-spawns from Resources like the audio manager and
// survives scene loads. When wind is enabled (the title toggle), it rolls a new
// random sideways force before each shot, applies it to projectiles in flight, and
// shows it in the HUD. A fresh value is rolled once the previous shot has cleared,
// so the in-flight shot keeps the wind the player aimed for.
public class WindManager : MonoBehaviour
{
    public static WindManager Instance { get; private set; }

    [SerializeField] float maxWind = 8f;          // max magnitude of the sideways force
    [SerializeField] float areaRadius = 60f;      // covers the play area (centred on origin)
    [SerializeField] LayerMask projectileMask;    // set to the Projectile layer

    float currentWind;
    bool needNewWind = true;
    HUD hud;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject prefab = Resources.Load<GameObject>("WindManager");
        if (prefab != null) Instantiate(prefab);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        needNewWind = true;     // roll a fresh wind for the new level's first shot
        currentWind = 0f;
        hud = null;             // re-find the new scene's HUD
    }

    void Update()
    {
        if (!GameSettings.WindEnabled)
        {
            if (hud == null) hud = FindAnyObjectByType<HUD>();
            hud?.HideWind();
            return;
        }

        // Roll a new wind once nothing is in flight; mark for a re-roll while a shot flies.
        if (Projectile.ActiveCount == 0 && needNewWind)
        {
            RollWind();
            needNewWind = false;
        }
        else if (Projectile.ActiveCount > 0)
        {
            needNewWind = true;
        }
    }

    void FixedUpdate()
    {
        if (!GameSettings.WindEnabled || Mathf.Abs(currentWind) < 0.01f) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(Vector2.zero, areaRadius, projectileMask);
        foreach (Collider2D c in hits)
        {
            Rigidbody2D rb = c.attachedRigidbody;
            if (rb != null) rb.AddForce(Vector2.right * currentWind);
        }
    }

    void RollWind()
    {
        currentWind = Random.Range(-maxWind, maxWind);
        if (hud == null) hud = FindAnyObjectByType<HUD>();
        hud?.SetWind(currentWind);
    }
}
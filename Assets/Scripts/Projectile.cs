using UnityEngine;
using UnityEngine.InputSystem;

// Goes on the Projectile prefab. On its first collision it spawns an explosion
// and damages enemies within the blast radius, then destroys itself. Self-destructs
// after maxLifetime, bounces off bounceLayers, and fires Exploded(point, normal) so
// add-ons like ClusterBomb can react. explodeOnImpact can be turned off (e.g. for a
// timed airburst) and Detonate() triggered manually instead.
//
// Lindsay mode: when GameSettings.LindsayMode is on, pressing Space mid-flight kills
// the shell's momentum so it drops straight down, and the resulting blast is bigger.
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionRadius = 1.0f;
    [SerializeField] LayerMask damageMask;
    [SerializeField] int damage = 1;
    [SerializeField] bool explodeOnImpact = true;

    [Header("Lifetime")]
    [SerializeField] float maxLifetime = 6f;

    [Header("Bouncing")]
    [SerializeField] LayerMask bounceLayers;

    [Header("Lindsay mode (press Space mid-flight to drop)")]
    [SerializeField] bool allowLindsayDrop = true;
    [SerializeField] float lindsayBlastMultiplier = 2f;   // scales blast radius + explosion visual

    public System.Action<Vector2, Vector2> Exploded;   // (impact point, surface normal)

    Rigidbody2D rb;
    bool hasExploded;
    bool lindsayDropped;
    float age;
    Vector2 lastNormal = Vector2.up;

    public static int ActiveCount { get; private set; }
    public static void ResetActiveCount() => ActiveCount = 0;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    void OnEnable()  { ActiveCount++; }
    void OnDisable() { ActiveCount = Mathf.Max(0, ActiveCount - 1); }

    public void SetExplodeOnImpact(bool value) => explodeOnImpact = value;

    // Explode now, at the current position (used by timed bursts).
    public void Detonate() { if (!hasExploded) Explode(transform.position); }

    void Update()
    {
        if (hasExploded) return;

        // Lindsay mode: stop the shell mid-air so it falls straight down.
        if (allowLindsayDrop && !lindsayDropped && GameSettings.LindsayMode
            && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            lindsayDropped = true;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                if (rb.gravityScale <= 0f) rb.gravityScale = 1f;   // make sure it actually drops
            }
        }

        if (maxLifetime <= 0f) return;
        age += Time.deltaTime;
        if (age >= maxLifetime) Explode(transform.position);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded || !explodeOnImpact) return;
        if (IsInMask(collision.gameObject.layer, bounceLayers)) return;

        ContactPoint2D contact = collision.GetContact(0);
        lastNormal = contact.normal;
        Explode(contact.point);
    }

    void Explode(Vector2 point)
    {
        hasExploded = true;

        float blastScale = lindsayDropped ? lindsayBlastMultiplier : 1f;

        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, point, Quaternion.identity);
            if (lindsayDropped) fx.transform.localScale *= blastScale;
        }

        GameAudio.Instance?.PlayExplosion();

        Collider2D[] hits = Physics2D.OverlapCircleAll(point, explosionRadius * blastScale, damageMask);
        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();
            if (health != null) health.TakeDamage(damage);
        }

        Exploded?.Invoke(point, lastNormal);
        Destroy(gameObject);
    }

    static bool IsInMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
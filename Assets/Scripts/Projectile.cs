using UnityEngine;

// Goes on the Projectile prefab. On its first collision it spawns an explosion
// and damages every enemy within the blast radius, then destroys itself.
// A single radius check covers BOTH a direct hit and splash damage.
// It self-destructs after maxLifetime seconds (prevents soft-locks / off-screen shots),
// and it bounces off anything on the bounceLayers instead of exploding.
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionRadius = 1.0f;
    [SerializeField] LayerMask damageMask;   // set this to the Enemy layer in the Inspector
    [SerializeField] int damage = 1;

    [Header("Lifetime")]
    [SerializeField] float maxLifetime = 6f;   // auto-explode after this many seconds; <= 0 = never

    [Header("Bouncing")]
    [SerializeField] LayerMask bounceLayers;   // layers to bounce off instead of exploding (e.g. Barrier)

    bool hasExploded;
    float age;

    // --- In-flight tracking (used by LevelManager's loss check) ---
    public static int ActiveCount { get; private set; }
    public static void ResetActiveCount() => ActiveCount = 0;

    void OnEnable()  { ActiveCount++; }
    void OnDisable() { ActiveCount = Mathf.Max(0, ActiveCount - 1); }

    void Update()
    {
        if (hasExploded || maxLifetime <= 0f) return;
        age += Time.deltaTime;
        if (age >= maxLifetime) Explode(transform.position);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;

        // Bounce off barriers (physics handles the bounce) instead of exploding.
        if (IsInMask(collision.gameObject.layer, bounceLayers)) return;

        Vector2 point = collision.GetContact(0).point;
        Explode(point);
    }

    void Explode(Vector2 point)
    {
        hasExploded = true;

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, point, Quaternion.identity);

        // Damage every enemy whose collider overlaps the blast radius.
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, explosionRadius, damageMask);
        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();
            if (health != null) health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    static bool IsInMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
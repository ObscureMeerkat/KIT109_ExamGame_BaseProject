using UnityEngine;

// Goes on the Projectile prefab. On its first collision it spawns an explosion
// and damages enemies within the blast radius, then destroys itself. Self-destructs
// after maxLifetime, bounces off bounceLayers, and fires Exploded(point, normal) so
// add-ons like ClusterBomb can react. explodeOnImpact can be turned off (e.g. for a
// timed airburst) and Detonate() triggered manually instead.
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

    public System.Action<Vector2, Vector2> Exploded;   // (impact point, surface normal)

    bool hasExploded;
    float age;
    Vector2 lastNormal = Vector2.up;

    public static int ActiveCount { get; private set; }
    public static void ResetActiveCount() => ActiveCount = 0;

    void OnEnable()  { ActiveCount++; }
    void OnDisable() { ActiveCount = Mathf.Max(0, ActiveCount - 1); }

    public void SetExplodeOnImpact(bool value) => explodeOnImpact = value;

    // Explode now, at the current position (used by timed bursts).
    public void Detonate() { if (!hasExploded) Explode(transform.position); }

    void Update()
    {
        if (hasExploded || maxLifetime <= 0f) return;
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

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, point, Quaternion.identity);

        GameAudio.Instance?.PlayExplosion();

        Collider2D[] hits = Physics2D.OverlapCircleAll(point, explosionRadius, damageMask);
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
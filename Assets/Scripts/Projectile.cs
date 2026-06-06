using UnityEngine;

// Goes on the Projectile prefab. On its first collision it spawns an explosion
// and damages every enemy within the blast radius, then destroys itself.
// Self-destructs after maxLifetime seconds, and bounces off bounceLayers.
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionRadius = 1.0f;
    [SerializeField] LayerMask damageMask;
    [SerializeField] int damage = 1;

    [Header("Lifetime")]
    [SerializeField] float maxLifetime = 6f;

    [Header("Bouncing")]
    [SerializeField] LayerMask bounceLayers;

    bool hasExploded;
    float age;

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
        if (IsInMask(collision.gameObject.layer, bounceLayers)) return;
        Vector2 point = collision.GetContact(0).point;
        Explode(point);
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

        Destroy(gameObject);
    }

    static bool IsInMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
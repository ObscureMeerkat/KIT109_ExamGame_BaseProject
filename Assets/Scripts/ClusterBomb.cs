using UnityEngine;

// Add alongside Projectile on a cluster-bomb variant.
//   Fuse Time = 0  -> burst on impact; fragments spread away from the hit surface
//                     (within maxSpreadAngle of the contact normal) and spawn a little
//                     off the surface so they don't instantly re-explode.
//   Fuse Time > 0  -> timed airburst after that many seconds; fragments spread in all
//                     directions (no surface involved).
[RequireComponent(typeof(Projectile))]
public class ClusterBomb : MonoBehaviour
{
    [SerializeField] GameObject fragmentPrefab;
    [SerializeField] int fragmentCount = 5;

    [Header("Burst timing")]
    [SerializeField] float fuseTime = 0f;            // 0 = on impact; >0 = airburst delay

    [Header("Spread")]
    [SerializeField] float maxSpreadAngle = 80f;     // impact mode: keep <= 90 from the normal
    [SerializeField] float spawnOffset = 0.4f;       // impact mode: push fragments off the surface
    [SerializeField] float minSpeed = 4f;
    [SerializeField] float maxSpeed = 9f;

    Projectile projectile;

    void Awake()
    {
        projectile = GetComponent<Projectile>();
        projectile.Exploded += SpawnFragments;

        if (fuseTime > 0f)
        {
            projectile.SetExplodeOnImpact(false);
            Invoke(nameof(Detonate), fuseTime);
        }
    }

    void Detonate() => projectile.Detonate();

    void SpawnFragments(Vector2 point, Vector2 normal)
    {
        if (fragmentPrefab == null) return;

        bool airburst = fuseTime > 0f;
        Vector2 axis   = airburst ? Vector2.up : normal.normalized;
        float spread   = airburst ? 180f : maxSpreadAngle;                  // 180 each way = full circle
        Vector2 origin = airburst ? point : point + normal.normalized * spawnOffset;

        for (int i = 0; i < fragmentCount; i++)
        {
            float angle = Random.Range(-spread, spread);
            Vector2 dir = Rotate(axis, angle);
            float speed = Random.Range(minSpeed, maxSpeed);

            GameObject frag = Instantiate(fragmentPrefab, origin, Quaternion.identity);
            Rigidbody2D rb = frag.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * speed;
        }
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad), s = Mathf.Sin(rad);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }
}
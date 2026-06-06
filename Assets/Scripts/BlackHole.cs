using UnityEngine;

// Pulls nearby projectiles toward its centre. Place one in the Black Hole level.
// The pull is strongest at the centre and fades to zero at the edge of pullRadius.
// Projectiles can't get permanently stuck orbiting, because each one self-destructs
// via its own lifetime timer (the level's anti-soft-lock requirement).
public class BlackHole : MonoBehaviour
{
    [SerializeField] float pullRadius = 6f;
    [SerializeField] float pullStrength = 30f;
    [SerializeField] LayerMask projectileMask;   // set this to the Projectile layer

    void FixedUpdate()
    {
        Collider2D[] inRange = Physics2D.OverlapCircleAll(transform.position, pullRadius, projectileMask);
        foreach (Collider2D col in inRange)
        {
            Rigidbody2D rb = col.attachedRigidbody;
            if (rb == null) continue;

            Vector2 toCentre = (Vector2)transform.position - rb.position;
            float distance = toCentre.magnitude;
            if (distance < 0.01f) continue;

            // Stronger near the centre, zero at the edge.
            float strength = pullStrength * Mathf.Clamp01(1f - distance / pullRadius);
            rb.AddForce(toCentre.normalized * strength);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}
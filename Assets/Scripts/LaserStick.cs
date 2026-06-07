using UnityEngine;

// Add alongside Projectile on the laser-web projectile variant (set the Projectile's
// Explode On Impact off and Max Lifetime 0). On first contact the shot freezes where
// it is, joins the LaserWeb as a node, and stops counting as an in-flight projectile.
// It never explodes or damages on contact.
[RequireComponent(typeof(Rigidbody2D))]
public class LaserStick : MonoBehaviour
{
    Rigidbody2D rb;
    bool stuck;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (stuck) return;
        stuck = true;

        // Freeze in place.
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Stop being an in-flight projectile: disabling Projectile decrements the
        // active count and prevents any explode/lifetime behaviour.
        Projectile proj = GetComponent<Projectile>();
        if (proj != null) proj.enabled = false;

        // Join the web.
        LaserWeb web = FindAnyObjectByType<LaserWeb>();
        if (web != null) web.RegisterNode(transform);
    }
}
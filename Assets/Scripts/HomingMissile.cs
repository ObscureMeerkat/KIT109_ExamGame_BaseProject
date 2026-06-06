using UnityEngine;

// Add this alongside Projectile on a homing-projectile variant.
// The shot flies ballistically for `ballisticTime` seconds, then homes toward
// the enemy: it turns gradually (capped turn rate), holds a constant speed no
// greater than its launch speed, and changes colour to show the switch.
// Gravity is disabled once homing begins so it tracks cleanly.
[RequireComponent(typeof(Rigidbody2D))]
public class HomingMissile : MonoBehaviour
{
    [SerializeField] float ballisticTime = 1f;
    [SerializeField] float turnRate = 180f;          // degrees per second
    [SerializeField] string enemyTag = "Enemy";
    [SerializeField] Color homingColor = Color.red;  // visual cue for the switch

    Rigidbody2D rb;
    SpriteRenderer sprite;
    Transform target;

    float initialSpeed;
    float age;
    bool captured;
    bool homing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        GameObject e = GameObject.FindGameObjectWithTag(enemyTag);
        if (e != null) target = e.transform;
    }

    void FixedUpdate()
    {
        // Capture the launch speed on the first physics step after firing.
        if (!captured)
        {
            initialSpeed = rb.linearVelocity.magnitude;
            if (initialSpeed > 0.01f) captured = true;
            return;
        }

        age += Time.fixedDeltaTime;
        if (age < ballisticTime) return;   // still ballistic — leave physics alone

        if (!homing) StartHoming();
        if (target == null) return;        // enemy already gone: keep flying straight

        // Rotate the current heading gradually toward the target, at a fixed speed.
        Vector2 toTarget = ((Vector2)target.position - rb.position).normalized;
        Vector2 currentDir = rb.linearVelocity.sqrMagnitude > 0.0001f
            ? rb.linearVelocity.normalized
            : toTarget;
        float maxStep = turnRate * Mathf.Deg2Rad * Time.fixedDeltaTime;
        Vector2 newDir = Vector3.RotateTowards(currentDir, toTarget, maxStep, 0f);
        rb.linearVelocity = newDir * initialSpeed;   // constant; never above launch speed
    }

    void StartHoming()
    {
        homing = true;
        rb.gravityScale = 0f;
        if (sprite != null) sprite.color = homingColor;
    }
}
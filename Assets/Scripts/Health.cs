using UnityEngine;

// Generic hit-points component. Put it on anything a projectile can destroy.
// Optional damage gates on the same object are checked before damage applies:
//   - MinionGuard makes the body invincible while its minions live.
//   - Shield absorbs a fixed number of hits before breaking.
public class Health : MonoBehaviour
{
    [SerializeField] int hitPoints = 1;

    Shield shield;
    MinionGuard guard;

    public bool IsDead { get; private set; }

    void Awake()
    {
        shield = GetComponent<Shield>();
        guard = GetComponent<MinionGuard>();
    }

    public void TakeDamage(int amount = 1)
    {
        if (IsDead) return;

        // Invincible while guarded by living minions.
        if (guard != null && guard.IsInvincible) return;

        // A shield (if present and still up) absorbs the hit instead of the body.
        if (shield != null && shield.AbsorbHit()) return;

        hitPoints -= amount;
        if (hitPoints <= 0) Die();
    }

    void Die()
    {
        IsDead = true;
        Destroy(gameObject);
    }
}
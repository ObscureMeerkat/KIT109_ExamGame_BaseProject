using UnityEngine;

// Generic hit-points component. Optional damage gates on the same object are
// checked before damage applies: MinionGuard (invincible while minions live)
// and Shield (absorbs a fixed number of hits).
//
// Tick isPlayer on the player tank so its death ends the level as a loss
// (used by the mirror level and by too-close self-misfires).
public class Health : MonoBehaviour
{
    [SerializeField] int hitPoints = 1;
    [SerializeField] bool isPlayer = false;

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
        if (guard != null && guard.IsInvincible) return;
        if (shield != null && shield.AbsorbHit()) return;

        hitPoints -= amount;
        if (hitPoints <= 0) Die();
    }

    void Die()
    {
        IsDead = true;
        GameAudio.Instance?.PlayTankDestroyed();
        if (isPlayer) LevelManager.Instance?.OnPlayerDied();
        Destroy(gameObject);
    }
}
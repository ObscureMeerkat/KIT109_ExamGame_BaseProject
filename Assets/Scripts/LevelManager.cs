using UnityEngine;

// One LevelManager per level scene. Tracks ammo and decides win/loss.
//   Win  = the enemy is gone.
//   Loss = ammo is spent, nothing is still in flight, and the enemy survives.
// It finds the enemy (by tag) and the HUD at runtime, so dropping the prefabs
// into a new level scene needs no per-scene wiring.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Setup")]
    [SerializeField] int startingAmmo = 10;
    [SerializeField] string enemyTag = "Enemy";   // tag of the win-target enemy

    GameObject enemy;
    HUD hud;
    int ammoRemaining;
    bool hadEnemy;
    bool gameOver;

    // PlayerShooter checks this before firing.
    public bool CanShoot => !gameOver && ammoRemaining > 0;

    void Awake()
    {
        Instance = this;
        ammoRemaining = startingAmmo;
        Projectile.ResetActiveCount();

        enemy = GameObject.FindGameObjectWithTag(enemyTag);
        hadEnemy = enemy != null;
        if (!hadEnemy)
            Debug.LogWarning($"LevelManager: no live object tagged '{enemyTag}' was found in the scene.");

        hud = FindAnyObjectByType<HUD>();
        if (hud != null) hud.SetAmmo(ammoRemaining);
    }

    void Update()
    {
        if (gameOver) return;

        // Win once an enemy that existed at the start has been destroyed.
        if (hadEnemy && enemy == null)
        {
            EndLevel(true);
            return;
        }

        // Lose once ammo is gone AND no projectiles remain in the air.
        if (ammoRemaining <= 0 && Projectile.ActiveCount == 0)
            EndLevel(false);
    }

    // Called by PlayerShooter after a shot successfully launches.
    public void ConsumeAmmo()
    {
        ammoRemaining = Mathf.Max(0, ammoRemaining - 1);
        if (hud != null) hud.SetAmmo(ammoRemaining);
    }

    void EndLevel(bool won)
    {
        gameOver = true;
        if (hud != null) hud.ShowResult(won ? "Level complete!" : "Out of ammo!");
        // Polished animated congrats / game-over screens with stats come later.
    }
}
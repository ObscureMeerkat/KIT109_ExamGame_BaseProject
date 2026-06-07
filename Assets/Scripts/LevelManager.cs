using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// One LevelManager per level scene. State machine: Intro -> Playing -> GameOver.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    enum State { Intro, Playing, GameOver }

    [Header("Level info (shown at start)")]
    [SerializeField] int levelNumber = 1;
    [SerializeField] string levelTitle = "Default";
    [TextArea][SerializeField] string levelDescription = "Destroy the enemy tank.";
    [SerializeField] float introDuration = 2f;

    [Header("End of level")]
    [SerializeField] float endDuration = 3f;
    [SerializeField] float lossGrace = 0.5f;   // delay before a loss, so a late kill (e.g. laser web) can land

    [Header("Setup")]
    [SerializeField] int startingAmmo = 10;
    [SerializeField] string enemyTag = "Enemy";

    GameObject enemy;
    HUD hud;
    int ammoRemaining;
    int shotsFired;
    float levelTime;
    float lossTimer;
    bool hadEnemy;
    State state = State.Intro;

    public bool CanShoot    => state == State.Playing && ammoRemaining > 0;
    public bool InputLocked => state != State.Playing;

    void Awake()
    {
        Instance = this;
        ammoRemaining = startingAmmo;
        Projectile.ResetActiveCount();
        if (levelNumber == 1) GameStats.Reset();

        enemy = GameObject.FindGameObjectWithTag(enemyTag);
        hadEnemy = enemy != null;
        if (!hadEnemy)
            Debug.LogWarning($"LevelManager: no live object tagged '{enemyTag}' was found.");

        hud = FindAnyObjectByType<HUD>();
        if (hud != null) hud.SetAmmo(ammoRemaining);
    }

    void Start() => StartCoroutine(IntroRoutine());

    IEnumerator IntroRoutine()
    {
        state = State.Intro;
        if (hud != null) hud.ShowStartText($"Level {levelNumber}: {levelTitle}\n{levelDescription}");
        yield return new WaitForSeconds(introDuration);
        if (hud != null) hud.HideStartText();
        state = State.Playing;
    }

    void Update()
    {
        if (state != State.Playing) return;
        levelTime += Time.deltaTime;

        if (hadEnemy && enemy == null) { Win(); return; }

        if (ammoRemaining <= 0 && Projectile.ActiveCount == 0)
        {
            lossTimer += Time.deltaTime;
            if (lossTimer >= lossGrace) Lose();
        }
        else
        {
            lossTimer = 0f;
        }
    }

    public void ConsumeAmmo()
    {
        ammoRemaining = Mathf.Max(0, ammoRemaining - 1);
        shotsFired++;
        if (hud != null) hud.SetAmmo(ammoRemaining);
    }

    void Win()
    {
        state = State.GameOver;
        GameStats.TotalShots += shotsFired;
        GameStats.TotalTime  += levelTime;

        bool isFinal = SceneManager.GetActiveScene().buildIndex + 1 >= SceneManager.sceneCountInBuildSettings;
        if (isFinal)
        {
            if (hud != null) hud.ShowEndOfGame(GameStats.TotalShots, GameStats.TotalTime);
        }
        else
        {
            StartCoroutine(WinThenAdvance());
        }
    }

    IEnumerator WinThenAdvance()
    {
        if (hud != null) hud.ShowEndOfLevel(shotsFired, levelTime);
        yield return new WaitForSeconds(endDuration);

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(next);
    }

    void Lose()
    {
        state = State.GameOver;
        if (hud != null) hud.ShowResult("Out of ammo!\nPress R to retry or Q for menu.");
    }
}
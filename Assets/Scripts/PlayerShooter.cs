using UnityEngine;
using UnityEngine.InputSystem;

// Handles aiming, charging, and launching projectiles for the player tank.
// Attach this to the PlayerTank GameObject.
public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] LineRenderer aimLine;
    [SerializeField] Camera aimCamera;

    [Header("Shot tuning")]
    [SerializeField] float minLaunchSpeed = 4f;
    [SerializeField] float maxLaunchSpeed = 16f;
    [SerializeField] float maxChargeTime = 1.5f;
    [SerializeField] float maxAimLineLength = 4f;

    bool isCharging;
    float chargeTime;

    void Awake()
    {
        if (aimCamera == null) aimCamera = Camera.main;
        if (aimLine != null) aimLine.enabled = false;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = aimCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        Vector2 aimDir = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isCharging = true;
            chargeTime = 0f;
            if (aimLine != null) aimLine.enabled = true;
        }

        if (isCharging && Mouse.current.leftButton.isPressed)
        {
            chargeTime = Mathf.Min(chargeTime + Time.deltaTime, maxChargeTime);
            UpdateAimLine(aimDir);
        }

        if (isCharging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Launch(aimDir);
            isCharging = false;
            if (aimLine != null) aimLine.enabled = false;
        }
    }

    float ChargeFraction() => maxChargeTime <= 0f ? 1f : chargeTime / maxChargeTime;

    void UpdateAimLine(Vector2 aimDir)
    {
        if (aimLine == null) return;
        float length = Mathf.Lerp(0.5f, maxAimLineLength, ChargeFraction());
        Vector3 start = firePoint.position;
        Vector3 end = start + (Vector3)(aimDir * length);
        aimLine.positionCount = 2;
        aimLine.SetPosition(0, start);
        aimLine.SetPosition(1, end);
    }

    void Launch(Vector2 aimDir)
    {
        if (LevelManager.Instance != null && !LevelManager.Instance.CanShoot) return;

        float speed = Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, ChargeFraction());
        GameObject shot = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = shot.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = aimDir * speed;

        GameAudio.Instance?.PlayShoot();

        if (LevelManager.Instance != null) LevelManager.Instance.ConsumeAmmo();
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

// Handles aiming, charging, and launching projectiles for the player tank.
// Attach this to the PlayerTank GameObject.
public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform firePoint;          // muzzle: where shots spawn and where aim originates
    [SerializeField] GameObject projectilePrefab;   // the Projectile prefab
    [SerializeField] LineRenderer aimLine;          // shows direction + strength while charging
    [SerializeField] Camera aimCamera;              // leave empty to auto-use Camera.main

    [Header("Shot tuning")]
    [SerializeField] float minLaunchSpeed = 4f;     // launch speed from a quick tap
    [SerializeField] float maxLaunchSpeed = 16f;    // launch speed at full charge
    [SerializeField] float maxChargeTime = 1.5f;    // seconds of holding to reach full charge
    [SerializeField] float maxAimLineLength = 4f;   // aim line length (world units) at full charge

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

        // Aim direction: from the muzzle toward the mouse cursor, in world space.
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = aimCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        Vector2 aimDir = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        // Begin charging when the button goes down.
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isCharging = true;
            chargeTime = 0f;
            if (aimLine != null) aimLine.enabled = true;
        }

        // While held: accumulate charge (capped) and refresh the aim line.
        if (isCharging && Mouse.current.leftButton.isPressed)
        {
            chargeTime = Mathf.Min(chargeTime + Time.deltaTime, maxChargeTime);
            UpdateAimLine(aimDir);
        }

        // Release to fire.
        if (isCharging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Launch(aimDir);
            isCharging = false;
            if (aimLine != null) aimLine.enabled = false;
        }
    }

    // 0 at a tap, 1 at full charge.
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
        // Ammo gate: don't fire if out of ammo or the level has ended.
        if (LevelManager.Instance != null && !LevelManager.Instance.CanShoot) return;

        float speed = Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, ChargeFraction());
        GameObject shot = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = shot.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = aimDir * speed;   // Unity 6: linearVelocity (was velocity)

        // Spend one ammo for the successful shot.
        if (LevelManager.Instance != null) LevelManager.Instance.ConsumeAmmo();
    }
}
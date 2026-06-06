using UnityEngine;
using Unity.Cinemachine;

// Put this on the CinemachineCamera in the Cinemachine level.
// It follows the live projectile while one is in flight, and falls back to the
// player tank when there isn't one.
[RequireComponent(typeof(CinemachineCamera))]
public class CameraFollowProjectile : MonoBehaviour
{
    [SerializeField] Transform fallbackTarget;   // the player tank

    CinemachineCamera cam;

    void Awake() { cam = GetComponent<CinemachineCamera>(); }

    void LateUpdate()
    {
        Projectile shot = FindFirstObjectByType<Projectile>();
        Transform target = shot != null ? shot.transform : fallbackTarget;
        if (cam.Follow != target) cam.Follow = target;
    }
}
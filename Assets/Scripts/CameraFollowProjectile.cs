using UnityEngine;
using Unity.Cinemachine;

// Put this on the CinemachineCamera in a Cinemachine level.
// Follows the live projectile while one is in flight, falling back to the player
// tank otherwise. Optionally (Zoom On Play) holds a wide framing during the start
// panel, then eases in to a closer framing once play begins.
[RequireComponent(typeof(CinemachineCamera))]
public class CameraFollowProjectile : MonoBehaviour
{
    [SerializeField] Transform fallbackTarget;   // the player tank

    [Header("Zoom (optional)")]
    [Tooltip("Hold a wide view during the start panel, then zoom in when play starts. Leave off for plain follow (e.g. Level 8).")]
    [SerializeField] bool zoomOnPlay = false;
    [SerializeField] float introSize = 8f;       // wide arena framing during the intro text
    [SerializeField] float playSize = 4f;        // zoomed-in framing once playing
    [SerializeField] float zoomSpeed = 3f;       // how fast it eases between the two

    CinemachineCamera cam;

    void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
        if (zoomOnPlay) SetOrthoSize(introSize);   // start wide
    }

    void LateUpdate()
    {
        Projectile shot = FindFirstObjectByType<Projectile>();
        Transform target = shot != null ? shot.transform : fallbackTarget;
        if (cam.Follow != target) cam.Follow = target;

        if (!zoomOnPlay) return;

        // InputLocked is true during the intro text and at game over, false while playing.
        bool playing = LevelManager.Instance != null && !LevelManager.Instance.InputLocked;
        float targetSize = playing ? playSize : introSize;

        float current = cam.Lens.OrthographicSize;
        SetOrthoSize(Mathf.Lerp(current, targetSize, Time.deltaTime * zoomSpeed));
    }

    void SetOrthoSize(float size)
    {
        LensSettings lens = cam.Lens;
        lens.OrthographicSize = size;
        cam.Lens = lens;
    }
}
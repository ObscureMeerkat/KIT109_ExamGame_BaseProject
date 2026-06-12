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

    [Header("Debug")]
    [Tooltip("On-screen readout of the live camera state, for diagnosing confiner/follow issues.")]
    [SerializeField] bool debugReadout = false;

    CinemachineCamera cam;
    CinemachineConfiner2D confiner;

    void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
        confiner = GetComponent<CinemachineConfiner2D>();
        if (zoomOnPlay) SetOrthoSize(introSize);   // start wide

        // Start on the player from frame one instead of easing in from wherever
        // the camera was saved in the scene. The confiner still clamps this.
        if (fallbackTarget != null)
        {
            cam.Follow = fallbackTarget;
            cam.ForceCameraPosition(fallbackTarget.position, transform.rotation);
        }
    }

    // Runs in Update (not LateUpdate) so the size is applied before the Cinemachine
    // Brain composes the frame in LateUpdate — otherwise the zoom can lag the camera
    // position by a frame, which reads as jagged stutter.
    void Update()
    {
        Projectile shot = FindFirstObjectByType<Projectile>();
        Transform target = shot != null ? shot.transform : fallbackTarget;
        if (cam.Follow != target) cam.Follow = target;

        if (!zoomOnPlay) return;

        // InputLocked is true during the intro text and at game over, false while playing.
        bool playing = LevelManager.Instance != null && !LevelManager.Instance.InputLocked;
        float targetSize = playing ? playSize : introSize;

        // Frame-rate-independent ease toward the target size, snapping when close
        // so the zoom actually finishes instead of creeping forever.
        float t = 1f - Mathf.Exp(-zoomSpeed * Time.deltaTime);
        float next = Mathf.Lerp(cam.Lens.OrthographicSize, targetSize, t);
        if (Mathf.Abs(next - targetSize) < 0.01f) next = targetSize;
        SetOrthoSize(next);
    }

    void SetOrthoSize(float size)
    {
        LensSettings lens = cam.Lens;
        if (Mathf.Abs(lens.OrthographicSize - size) < 0.0001f)
            return;
        lens.OrthographicSize = size;
        cam.Lens = lens;

        // Cinemachine's Confiner2D computes its clamp region once and doesn't notice
        // lens-size changes, so a zooming camera stays confined to a stale window
        // size. Force the recompute (cheap for simple box bounds).
        if (confiner != null)
            confiner.InvalidateBoundingShapeCache();
    }

    void OnGUI()
    {
        if (!debugReadout) return;

        Camera main = Camera.main;
        string text =
            $"vcam pos: {transform.position}\n" +
            $"main cam pos: {(main != null ? main.transform.position : Vector3.zero)}\n" +
            $"vcam ortho: {cam.Lens.OrthographicSize:0.00}   main ortho: {(main != null ? main.orthographicSize : 0f):0.00}   aspect: {(main != null ? main.aspect : 0f):0.00}\n" +
            $"follow: {(cam.Follow != null ? cam.Follow.name : "NULL")} @ {(cam.Follow != null ? cam.Follow.position : Vector3.zero)}\n" +
            $"confiner: {(confiner != null ? (confiner.BoundingShape2D != null ? confiner.BoundingShape2D.name : "NO SHAPE") : "NOT ATTACHED")}" +
            $"   displacement: {(confiner != null ? confiner.GetCameraDisplacementDistance(cam) : 0f):0.00}";

        GUI.color = Color.black;
        GUI.Label(new Rect(11, 41, 800, 120), text);
        GUI.color = Color.yellow;
        GUI.Label(new Rect(10, 40, 800, 120), text);
    }
}
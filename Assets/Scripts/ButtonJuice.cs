using UnityEngine;
using UnityEngine.EventSystems;

// Drop on any UI button or toggle: grows slightly on hover, squashes on press.
// Pure scale juice — works alongside the Button's color tint transition.
public class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float hoverScale = 1.06f;
    [SerializeField] float pressScale = 0.94f;
    [SerializeField] float speed = 12f;          // how snappily it eases between scales

    Vector3 baseScale = Vector3.one;   // the control's natural scale; juice multiplies it
    float current = 1f;
    float target = 1f;
    bool hovering;

    public void OnPointerEnter(PointerEventData e) { hovering = true; target = hoverScale; }
    public void OnPointerExit(PointerEventData e)  { hovering = false; target = 1f; }
    public void OnPointerDown(PointerEventData e)  { target = pressScale; }
    public void OnPointerUp(PointerEventData e)    { target = hovering ? hoverScale : 1f; }

    void Awake() { baseScale = transform.localScale; }

    void OnDisable()
    {
        current = 1f;
        target = 1f;
        transform.localScale = baseScale;
    }

    void Update()
    {
        float t = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);
        current = Mathf.Lerp(current, target, t);
        transform.localScale = baseScale * current;
    }
}

using UnityEngine;

// Sits on the enemy alongside Health. Absorbs a fixed number of hits before the
// enemy can be damaged, and changes its visual for each remaining charge level.
public class Shield : MonoBehaviour
{
    [SerializeField] int maxCharges = 3;
    [SerializeField] SpriteRenderer shieldRenderer;   // the ring sprite around the enemy
    [SerializeField] Color[] chargeColors;            // one colour per remaining-charge level

    int charges;

    void Awake()
    {
        charges = maxCharges;
        UpdateVisual();
    }

    // Returns true if the hit was absorbed (shield still up); false if the shield
    // is already down, in which case the hit should pass through to Health.
    public bool AbsorbHit()
    {
        if (charges <= 0) return false;
        charges--;
        UpdateVisual();
        return true;
    }

    void UpdateVisual()
    {
        if (shieldRenderer == null) return;

        if (charges <= 0)
        {
            shieldRenderer.enabled = false;
            return;
        }

        shieldRenderer.enabled = true;
        // chargeColors[0] = look at 1 charge, [1] = at 2 charges, etc.
        if (chargeColors != null && chargeColors.Length >= charges)
            shieldRenderer.color = chargeColors[charges - 1];
    }
}
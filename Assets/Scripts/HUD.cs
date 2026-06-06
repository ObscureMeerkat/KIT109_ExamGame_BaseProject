using UnityEngine;
using TMPro;

// Lives on the in-game UI Canvas. The LevelManager finds this at runtime and
// drives it, so the text fields are wired once inside the prefab and never need
// re-wiring per scene.
public class HUD : MonoBehaviour
{
    [SerializeField] TMP_Text ammoText;
    [SerializeField] TMP_Text resultText;

    void Awake()
    {
        if (resultText != null) resultText.gameObject.SetActive(false);
    }

    public void SetAmmo(int ammo)
    {
        if (ammoText != null) ammoText.text = "Ammo: " + ammo;
    }

    public void ShowResult(string message)
    {
        if (resultText == null) return;
        resultText.gameObject.SetActive(true);
        resultText.text = message;
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

// Lives on the in-game UI Canvas. The LevelManager (and WindManager) find this at
// runtime and drive it: ammo, loss text, animated start/end-of-level messages, the
// end-of-game screen, and the wind readout.
public class HUD : MonoBehaviour
{
    [Header("In-game")]
    [SerializeField] TMP_Text ammoText;
    [SerializeField] TMP_Text resultText;

    [Header("Start-of-level")]
    [SerializeField] CanvasGroup startPanel;
    [SerializeField] TMP_Text startText;

    [Header("End-of-level")]
    [SerializeField] CanvasGroup endPanel;
    [SerializeField] TMP_Text endCongratsText;
    [SerializeField] TMP_Text endStatsText;
    [SerializeField] string[] congratsMessages = { "Nice shooting!", "Target down!", "Direct hit!" };

    [Header("End-of-game")]
    [SerializeField] CanvasGroup endGamePanel;
    [SerializeField] TMP_Text endGameText;

    [Header("Wind")]
    [SerializeField] TMP_Text windText;

    [SerializeField] float fadeTime = 0.3f;

    void Awake()
    {
        if (resultText != null) resultText.gameObject.SetActive(false);
        if (windText != null) windText.gameObject.SetActive(false);
        SetAlpha(startPanel, 0f);
        SetAlpha(endPanel, 0f);
        SetAlpha(endGamePanel, 0f);
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

    public void ShowStartText(string message)
    {
        if (startText != null) startText.text = message;
        Animate(startPanel, true);
    }

    public void HideStartText() => Animate(startPanel, false);

    public void ShowEndOfLevel(int shots, float time)
    {
        if (endCongratsText != null && congratsMessages.Length > 0)
            endCongratsText.text = congratsMessages[Random.Range(0, congratsMessages.Length)];
        if (endStatsText != null)
            endStatsText.text = $"Shots: {shots}\nTime: {time:F1}s";
        Animate(endPanel, true);
    }

    public void ShowEndOfGame(int totalShots, float totalTime)
    {
        if (endGameText != null)
            endGameText.text = $"All levels complete!\nTotal shots: {totalShots}\nTotal time: {totalTime:F1}s";
        Animate(endGamePanel, true);
    }

    public void ReturnToMenu() => SceneManager.LoadScene(0);

    public void SetWind(float wind)
    {
        if (windText == null) return;
        windText.gameObject.SetActive(true);
        string arrow = wind >= 0 ? "\u2192" : "\u2190";   // right / left arrow
        windText.text = $"Wind: {arrow} {Mathf.Abs(wind):F1}";
    }

    public void HideWind()
    {
        if (windText != null) windText.gameObject.SetActive(false);
    }

    void SetAlpha(CanvasGroup g, float a) { if (g != null) g.alpha = a; }

    void Animate(CanvasGroup g, bool show)
    {
        if (g != null) StartCoroutine(Fade(g, show));
    }

    IEnumerator Fade(CanvasGroup g, bool show)
    {
        float from = g.alpha;
        float to = show ? 1f : 0f;
        float startScale = show ? 0.85f : 1f;
        float endScale   = show ? 1f : 0.85f;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeTime);
            g.alpha = Mathf.Lerp(from, to, k);
            g.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, k);
            yield return null;
        }
        g.alpha = to;
        g.transform.localScale = Vector3.one;
    }
}
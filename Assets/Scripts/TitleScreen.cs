using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Drives the title screen. The level-select buttons are placed by hand in the
// editor as children of levelButtonContainer (so they're visible and arrangeable);
// at runtime this labels them 1..N in order and wires each to load its level.
// Wire Start and the toggle events to the public methods in the Inspector.
public class TitleScreen : MonoBehaviour
{
    [Header("Level select")]
    [SerializeField] Transform levelButtonContainer;  // parent of the level buttons (has a layout group)
    [SerializeField] int firstLevelSceneIndex = 1;     // Title is 0, Level 1 is scene 1

    void Start()
    {
        if (levelButtonContainer == null) return;

        int level = 1;
        foreach (Transform child in levelButtonContainer)
        {
            Button b = child.GetComponent<Button>();
            if (b == null) continue;

            int sceneIndex = firstLevelSceneIndex + (level - 1);   // fresh per iteration
            TMP_Text label = b.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = level.ToString();
            b.onClick.AddListener(() => LoadScene(sceneIndex));
            level++;
        }
    }

    public void StartGame() => LoadScene(firstLevelSceneIndex);

    public void LoadScene(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(sceneIndex);
    }

    // Wired to the toggles' OnValueChanged (dynamic bool).
    public void SetLindsayMode(bool on) => GameSettings.LindsayMode = on;
    public void SetWind(bool on)        => GameSettings.WindEnabled = on;
    public void SetMusic(bool on)       => GameAudio.Instance?.SetMusicEnabled(on);
}
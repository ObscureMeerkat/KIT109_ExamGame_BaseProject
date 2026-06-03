using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneSwitcherKeys : MonoBehaviour
{
    int sceneOffset = 1; //we have 1 extra scene (title), if adding other scenes at the beginning of the scene list, tweak this number as needed

    public void LoadScene(int num)
    {
        if (num < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(num);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (var i = Key.Digit1; i <= Key.Digit0; i++)  // this order might seem weird, but 0 > 1 for digit keys
        {
            if (Keyboard.current[i].wasPressedThisFrame)
            {
                var sceneIndex = i - Key.Digit1;
                sceneIndex += sceneOffset;

                LoadScene(sceneIndex);
                return;
            }
        }
        for (var i = Key.Numpad0; i <= Key.Numpad9; i++)
        {
            if (Keyboard.current[i].wasPressedThisFrame)
            {
                if (i == Key.Numpad0)
                    i += 10; //hax

                var sceneIndex = i - Key.Numpad1;
                sceneIndex += sceneOffset;

                LoadScene(sceneIndex);
                return;
            }
        }
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            LoadScene(0);
            return;
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }
    }

}

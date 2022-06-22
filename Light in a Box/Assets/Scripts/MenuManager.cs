using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static bool gameIsPaused = false;

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject LevelsMenuUI;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        gameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        gameIsPaused = true;
    }

    public void OpenLevelSelection()
    {
        pauseMenuUI.SetActive(false);
        LevelsMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    public void Back()
    {
        LevelsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

}

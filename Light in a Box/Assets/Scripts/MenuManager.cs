using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static bool gameIsPaused = false;

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject LevelsMenuUI;
    [SerializeField] private GameObject HUD;

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
        HUD.SetActive(true);
        gameIsPaused = false;
    }

    public void Pause()
    {
        HUD.SetActive(false);
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

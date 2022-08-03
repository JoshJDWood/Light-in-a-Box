using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static bool gameIsPaused = false;

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject LevelsMenuUI;
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject solvedHUD;

    private DragController dragController;
    private AudioManager audioManager;

    private void Start()
    {
        dragController = FindObjectOfType<DragController>();
        audioManager = FindObjectOfType<AudioManager>();
    }

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
        audioManager.Play("resume");
        pauseMenuUI.SetActive(false);
        HUD.SetActive(true);
        gameIsPaused = false;
    }

    public void ResetResume()
    {
        audioManager.Play("resume");
        pauseMenuUI.SetActive(false);
        LevelsMenuUI.SetActive(false);
        solvedHUD.SetActive(false);
        HUD.SetActive(true);
        gameIsPaused = false;
    }

    public void Pause()
    {
        audioManager.Play("pause");
        dragController.DropForPause();
        pauseMenuUI.SetActive(true);
        HUD.SetActive(false);
        gameIsPaused = true;
    }

    public void OpenLevelSelection()
    {
        audioManager.Play("swapMenu");
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
        audioManager.Play("swapMenu");
        LevelsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static bool gameIsPaused = false;

    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject LevelsMenuUI;
    [SerializeField] private GameObject SettingsMenuUI;
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject solvedHUD;
    [SerializeField] private GameObject checkButton;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button hardButton;

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
        LevelsMenuUI.SetActive(false);
        SettingsMenuUI.SetActive(false);
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

    public void OpenSettings()
    {
        audioManager.Play("swapMenu");
        pauseMenuUI.SetActive(false);
        SettingsMenuUI.SetActive(true);
    }

    public void HardMode(bool hardMode)
    {
        dragController.hardMode = hardMode;
        checkButton.SetActive(hardMode);
        if(hardMode)
        {
            ColorBlock newColors = hardButton.colors;
            newColors.normalColor = new Color(0, 0, 0, 1);
            hardButton.colors = newColors;

            newColors.normalColor = new Color(0, 0, 0, 0);
            easyButton.colors = newColors;
        }
        else
        {
            ColorBlock newColors = easyButton.colors;
            newColors.normalColor = new Color(0, 0, 0, 1);
            easyButton.colors = newColors;

            newColors.normalColor = new Color(0, 0, 0, 0);
            hardButton.colors = newColors;
        }
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
        SettingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

}

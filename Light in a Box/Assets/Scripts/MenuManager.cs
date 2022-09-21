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
    [SerializeField] private Button levelButtonPrefab;
    [SerializeField] private Text hintsRemainingText;
    [SerializeField] private string saveFileName;

    [SerializeField] private DragController dragController;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GridManager gridManager;

    private void Awake()
    {
        LoadSaveData();
        formatLevelButtons();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
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
        if (gridManager.currentPuzzleIndex != 0) //to prevent unpausing when entering the tutorial
            gameIsPaused = false;
    }

    public void Pause()
    {
        audioManager.Play("pause");
        dragController.DropForPause();
        LevelsMenuUI.SetActive(false);
        SettingsMenuUI.SetActive(false);
        solvedHUD.SetActive(false);
        HUD.SetActive(false);
        pauseMenuUI.SetActive(true);        
        gameIsPaused = true;
    }

    public void OpenLevelSelection()
    {
        audioManager.Play("swapMenu");
        pauseMenuUI.SetActive(false);
        SettingsMenuUI.SetActive(false);
        solvedHUD.SetActive(false);
        HUD.SetActive(false);
        LevelsMenuUI.SetActive(true);
    }

    public void OpenSettings()
    {
        audioManager.Play("swapMenu");
        pauseMenuUI.SetActive(false);
        LevelsMenuUI.SetActive(false);
        solvedHUD.SetActive(false);
        HUD.SetActive(false);
        SettingsMenuUI.SetActive(true);
    }

    public void HardMode(bool hardMode)
    {
        dragController.hardMode = hardMode;
        checkButton.SetActive(hardMode);
        if (hardMode)
        {
            hardButton.interactable = false;            
            easyButton.interactable = true;
        }
        else
        {
            hardButton.interactable = true;
            easyButton.interactable = false;
        }
        if (gridManager.currentPuzzleIndex != -1)
        {
            gridManager.SpawnNewPuzzle(gridManager.currentPuzzleIndex);
        }
    }

    private void formatLevelButtons()
    {
        int[] solvedValues = gridManager.GetPuzzleSolvedValues();
        int x = 0, y = 0;
        for (int i = 0; i < solvedValues.Length; i++)
        {
            int j = i;
            Button levelButton = Instantiate(levelButtonPrefab, LevelsMenuUI.gameObject.transform);
            if (i == 0)
            {
                levelButton.name = "Level Button " + (i); //for the tutorial level
                levelButton.GetComponentInChildren<Text>().text = "T";
            }
            else
            {
                levelButton.name = "Level Button " + (i); //so that levels start at number 1 not 0
                levelButton.GetComponentInChildren<Text>().text = "" + (i);
            }
            levelButton.transform.Translate(new Vector2(150 * x, -110 * y));
            levelButton.onClick.AddListener(() => { gridManager.SpawnNewPuzzle(j); });
            levelButton.onClick.AddListener(() => { Resume(); });

            Text solvedText = levelButton.gameObject.transform.Find("Solved Text").GetComponent<Text>();
            solvedText.text = "" + solvedValues[i];
            if (solvedValues[i] == SaveManager.unsolvedVal)
            {
                solvedText.gameObject.SetActive(false);
            }
            else
            {
                levelButton.image.color = Color.white;
                if (solvedValues[i] == SaveManager.solvedEasy)
                    solvedText.gameObject.SetActive(false);
            }

            x = (x + 1) % 5;
            if (x == 0)
            {
                y++;
            }
        }
    }

    public void LoadSaveData()
    {
        SaveManager.SaveData data = SaveManager.LoadFile(saveFileName);
        if (data != null)
        {
            HardMode(data.hardMode);
            dragController.hintsRemaining = data.hintsRemaining;
            hintsRemainingText.text = "" + dragController.hintsRemaining;
            gridManager.SetPuzzleSolvedValues(data.solvedValues);
        }
    }

    public void UpdateSaveScores(int solvedValue)
    {
        if (gridManager.NewBestScore(solvedValue))
        {
            SaveManager.SaveFile(gridManager.GetPuzzleSolvedValues(), dragController.hardMode, dragController.hintsRemaining, saveFileName);
            Transform buttonTransform = LevelsMenuUI.gameObject.transform.Find("Level Button " + (gridManager.currentPuzzleIndex));
            Text solvedText = buttonTransform.Find("Solved Text").GetComponent<Text>();
            solvedText.text = "" + solvedValue;
            if (solvedValue == SaveManager.unsolvedVal)
            {
                solvedText.gameObject.SetActive(false);
            }
            else
            {
                buttonTransform.GetComponent<Button>().image.color = Color.white;
                if (solvedValue == SaveManager.solvedEasy)
                    solvedText.gameObject.SetActive(false);
                else
                    solvedText.gameObject.SetActive(true);
            }
        }
    }

    public void ResetScores()
    {
        gridManager.ResetPuzzleScoreValues();

        for (int i = 1; i < LevelsMenuUI.transform.childCount; i++) //starts from 1 to miss back button
        {
            Destroy(LevelsMenuUI.transform.GetChild(i).gameObject);
        }

        formatLevelButtons();
        SaveManager.SaveFile(gridManager.GetPuzzleSolvedValues(), dragController.hardMode, dragController.hintsRemaining, saveFileName);
    }
    public void Back()
    {
        audioManager.Play("swapMenu");
        LevelsMenuUI.SetActive(false);
        SettingsMenuUI.SetActive(false);
        solvedHUD.SetActive(false);
        HUD.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
        SaveManager.SaveFile(gridManager.GetPuzzleSolvedValues(), dragController.hardMode, dragController.hintsRemaining, saveFileName);
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}

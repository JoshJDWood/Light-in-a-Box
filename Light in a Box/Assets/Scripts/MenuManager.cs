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
    [SerializeField] private string saveFileName;

    private DragController dragController;
    private AudioManager audioManager;
    private GridManager gridManager;

    private void Awake()
    {
        dragController = FindObjectOfType<DragController>();
        audioManager = FindObjectOfType<AudioManager>();
        gridManager = FindObjectOfType<GridManager>();

        LoadSaveData();
        formatLevelButtons();
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

    private void formatLevelButtons()
    {
        int[] solvedValues = gridManager.GetPuzzleSolvedValues();
        int x = 0, y = 0;
        for (int i = 0; i < solvedValues.Length; i++)
        {
            int j = i;
            Button levelButton = Instantiate(levelButtonPrefab, LevelsMenuUI.gameObject.transform);
            levelButton.name = "Level Button " + (i + 1); //so that levels start at number 1 not 0
            levelButton.GetComponentInChildren<Text>().text = "" + (i + 1);
            levelButton.transform.Translate(new Vector2(150 * x, -150 * y));
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
            gridManager.SetPuzzleSolvedValues(data.solvedValues);
        }
    }

    public void UpdateSaveScores(int solvedValue)
    {
        if (gridManager.NewBestScore(solvedValue))
        {
            SaveManager.SaveFile(gridManager.GetPuzzleSolvedValues(), dragController.hardMode, saveFileName);
            Transform buttonTransform = LevelsMenuUI.gameObject.transform.Find("Level Button " + (gridManager.currentPuzzleIndex + 1));
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
    public void Back()
    {
        audioManager.Play("swapMenu");
        LevelsMenuUI.SetActive(false);
        SettingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
        SaveManager.SaveFile(gridManager.GetPuzzleSolvedValues(), dragController.hardMode, saveFileName);
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}

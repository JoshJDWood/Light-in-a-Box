using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static bool gameIsPaused = true;

    [SerializeField] private Camera cam;
    [SerializeField] private List<GameObject> MenuScreens = new List<GameObject>();
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject LevelsMenuUI;
    [SerializeField] private GameObject SettingsMenuUI;
    [SerializeField] private GameObject HUD;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject solvedHUD;
    [SerializeField] private GameObject checkButton;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button levelButtonPrefab;
    [SerializeField] private Text hintsRemainingText;
    [SerializeField] private GameObject adFailedPopup;
    [SerializeField] private RewardedAdsButton rewardedAdsButton;
    [SerializeField] private AdsInitializer adsInitializer;

    public string saveFileName;
    private bool tutorialCompleted = false;

    [SerializeField] private DragController dragController;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TutorialManager tutorialManager;

    private void Awake()
    {
        LoadSaveData(); //removed for webgl where there is no save file
        //gridManager.ResetPuzzleScoreValues(); //added in place of loading save file for webgl version
        formatLevelButtons();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
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
        adFailedPopup.SetActive(false);
        if (gridManager.currentPuzzleIndex == 0 && tutorialManager.promptIndex == 0) //to prevent unpausing when entering the tutorial
            gameIsPaused = true;
        else
            gameIsPaused = false;
    }

    public void StartGame()
    {
        titleScreen.SetActive(false);
        if (!tutorialCompleted)
            gridManager.SpawnNewPuzzle(0);
        Resume();
    }

    public void Pause()
    {
        audioManager.Play("pause");
        dragController.DropForPause();
        foreach (GameObject screen in MenuScreens)
            screen.SetActive(false);
        solvedHUD.SetActive(false);
        HUD.SetActive(false);
        pauseMenuUI.SetActive(true);        
        gameIsPaused = true;
    }

    public void OpenMenuScreen(int i)
    {
        audioManager.Play("swapMenu");
        solvedHUD.SetActive(false);
        HUD.SetActive(false);
        pauseMenuUI.SetActive(true);
        foreach (GameObject screen in MenuScreens)
            screen.SetActive(false);
        MenuScreens[i].SetActive(true);        
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
        if (gridManager.currentPuzzleIndex > 0)
        {
            gridManager.SpawnNewPuzzle(gridManager.currentPuzzleIndex);
        }
    }

    private void formatLevelButtons()
    {
        int[] solvedValues = gridManager.GetPuzzleSolvedValues();
        float scaleFactor = (float)Screen.height / 1080; //scaling for different resolutions
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
            levelButton.transform.Translate(new Vector2(scaleFactor * 150 * (x - 2),scaleFactor * -150 * (y - 2)));
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

    public void ShowAdFailedPopup()
    {
        adFailedPopup.SetActive(true);
        Text failedText = adFailedPopup.GetComponentInChildren<Text>();
        if(!adsInitializer.IsInitialized())
        {
            rewardedAdsButton.adError = "Ads Inintialization Failed.";
        }
        else if (rewardedAdsButton.adError == "")
        {
            rewardedAdsButton.adError = "Unknown Error.";
        }
        failedText.text = "Ad failed. Error type: " + rewardedAdsButton.adError + " Please try again later";

        if (adsInitializer.IsInitialized())
        {
            rewardedAdsButton.LoadAd(); //try to load new ad
        }
        else
        {
            adsInitializer.InitializeAds(); // retry initialization
        }
        gameIsPaused = true;
    }


    public void LoadSaveData()
    {
        SaveManager.SaveData data = SaveManager.LoadFile(saveFileName);
        if (data != null)
        {
            HardMode(data.hardMode);
            dragController.hintsRemaining = data.hintsRemaining;
            if (dragController.hintsRemaining == 0)
            {
                hintsRemainingText.text = "Watch Ad";
            }
            else
            {
                hintsRemainingText.text = "" + dragController.hintsRemaining;
            }
            gridManager.SetPuzzleSolvedValues(data.solvedValues);
            if (data.solvedValues[0] != SaveManager.unsolvedVal)
                tutorialCompleted = true;
            gridManager.SetOutlineMode(data.outlineMode, false);
            audioManager.SetAudioSettings(data.audioSettings);
        }
        else
        {
            gridManager.ResetPuzzleScoreValues();
        }
    }

    public void UpdateSaveScores(int solvedValue)
    {
        if (gridManager.NewBestScore(solvedValue))
        {
            SaveManager.SaveFile(gridManager.GetPuzzleSolvedValues(), dragController.hardMode, (int)gridManager.currentOutlineMode,
                audioManager.GetAudioSettings(), dragController.hintsRemaining, saveFileName);
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
        SaveManager.SaveFile(gridManager.GetPuzzleSolvedValues(), dragController.hardMode, (int)gridManager.currentOutlineMode, 
            audioManager.GetAudioSettings(), dragController.hintsRemaining, saveFileName);
    }

    public void QuitGame()
    {
        SaveManager.SaveFile(gridManager.GetPuzzleSolvedValues(), dragController.hardMode, (int)gridManager.currentOutlineMode, 
            audioManager.GetAudioSettings(), dragController.hintsRemaining, saveFileName);
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject outerWall;
    [SerializeField] private GameObject outerWallCorner;
    [SerializeField] private Text hintsRemainingText;
    [SerializeField] private List<GameObject> blockPrefabs;
    [SerializeField] private List<Puzzle> puzzlePrefabs;
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private Button tutorialNextPromptButton;
    [SerializeField] private GameObject solvedHUD;

    private List<Tile> tiles = new List<Tile>();
    private List<Draggable> blocks = new List<Draggable>();
    private List<GameObject> hintsOnDisplay = new List<GameObject>();
    private Puzzle puzzle;
    public int currentPuzzleIndex = -1;

    [SerializeField] private CastBeam lightSource;
    [SerializeField] private DragController dragController;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private MenuManager menuManager;

    private float wallThickness = 0.1f; //actually half wall thinkness
    private int outerWallCount = 0;
    private int outerWallCornerCount = 0;
    private float spawnX = -2.5f;
    private float spawnY = 0.5f;
    private float spawnGap = 1.1f;

    [SerializeField] private Transform cam;

    void Start()
    {
        GenerateGrid(width, height);
        foreach (Draggable block in FindObjectsOfType<Draggable>())
        {
            blocks.Add(block);
        }
        puzzle = FindObjectOfType<Puzzle>();
    }

    public void SpawnNewPuzzle(int i)
    {
        tutorialCanvas.SetActive(false);        

        DeleteOldPuzzle();
        currentPuzzleIndex = i;
        this.puzzle = Instantiate(puzzlePrefabs[i]);
        GenerateGrid(puzzle.width, puzzle.height);
        lightSource.transform.position = puzzle.lightPos;
        float x = 0, y = 0;
        foreach(BlockData bD in puzzle.solution)
        {
            if (bD.id != 0)
            {
                GameObject newBlock = Instantiate(blockPrefabs[bD.id], new Vector2(spawnX + x * spawnGap, spawnY + ((float)puzzle.height / 2) - y * spawnGap),
                    Quaternion.Euler(new Vector3(0, 0, 90 * bD.r)));
                newBlock.GetComponent<Draggable>().UpdateCR();
                blocks.Add(newBlock.GetComponent<Draggable>());

                x = (x + 1) % 2;
                if ( x == 0)
                {
                    y++;
                }
            }
        }

        if (i == 0)
        {
            dragController.hardMode = false;
            tutorialManager.ResetTutorialIndex();
            tutorialCanvas.SetActive(true);
            MenuManager.gameIsPaused = true; 
            puzzle.gameObject.SetActive(false);
        }

        if (!dragController.hardMode)
            StartCoroutine(dragController.RelightSequence(false));
        else
            lightSource.LightOff();
    }

    public void DeleteOldPuzzle()
    {
        if (puzzle != null)
        {
            Destroy(puzzle.gameObject);
        }
        for (int i = 0; i < outerWallCount; i++)
        {
            Destroy(GameObject.Find("Outer Wall " + i));
        }
        outerWallCount = 0;
        for (int i = 0; i < outerWallCornerCount; i++)
        {
            Destroy(GameObject.Find("Outer Wall Corner " + i));
        }
        outerWallCornerCount = 0;
        foreach (Tile t in tiles)
        {
            Destroy(t.gameObject);
        }
        tiles.Clear();
        foreach (Draggable b in blocks)
        {
            Destroy(b.gameObject);
        }
        blocks.Clear();
        hintsOnDisplay.Clear();
    }

    public void SpawnNextPuzzle()
    {
        if (currentPuzzleIndex < puzzlePrefabs.Count - 1)
        {
            SpawnNewPuzzle(currentPuzzleIndex + 1);
        }
        else
        {
            StartCoroutine(OpenLevelSelectionDelayed());
        }

        IEnumerator OpenLevelSelectionDelayed()
        {
            yield return new WaitForFixedUpdate();
            menuManager.OpenLevelSelection();
        }
    }

    void GenerateGrid(int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Tile spawnedTile = Instantiate(tilePrefab, new Vector2(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x + width * y}";
                tiles.Add(spawnedTile);
            }
        }

        for(int i = 0; i < width; i++)
        {
            SpawnOuterWall(new Vector2( i, -0.5f - wallThickness), 0);
            SpawnOuterWall(new Vector2( i, height - 0.5f + wallThickness), 0);
        }
        for(int i = 0; i < height; i++)
        {
            SpawnOuterWall(new Vector2(-0.5f - wallThickness, i), 90);
            SpawnOuterWall(new Vector2(width - 0.5f + wallThickness, i), 90);
        }

        SpawnOuterWallCorner(new Vector2(-0.5f - wallThickness, -0.5f - wallThickness), 0);
        SpawnOuterWallCorner(new Vector2(-0.5f - wallThickness, -0.5f + wallThickness + height), 0);
        SpawnOuterWallCorner(new Vector2(-0.5f + wallThickness + width, - 0.5f - wallThickness), 0);
        SpawnOuterWallCorner(new Vector2(-0.5f + wallThickness + width, -0.5f + wallThickness + height), 0);

        if (currentPuzzleIndex != 0)
            cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10);
        else
            cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.9f, -10);
    }

    void SpawnOuterWall(Vector2 pos, int rotateAmount)
    {
        GameObject outerWallspawn = Instantiate(outerWall, pos, Quaternion.Euler(new Vector3(0, 0, rotateAmount)));
        outerWallspawn.name = $"Outer Wall {outerWallCount}";
        outerWallCount++;
    }

    void SpawnOuterWallCorner(Vector2 pos, int rotateAmount)
    {
        GameObject outerWallspawn = Instantiate(outerWallCorner, pos, Quaternion.Euler(new Vector3(0, 0, rotateAmount)));
        outerWallspawn.name = $"Outer Wall Corner {outerWallCornerCount}";
        outerWallCornerCount++;
    }

    public int[] GetPuzzleSolvedValues()
    {
        int[] solvedValues = new int[puzzlePrefabs.Count];
        for (int i = 0; i < puzzlePrefabs.Count; i++)
        {
            solvedValues[i] = puzzlePrefabs[i].solvedValue;
        }
        return solvedValues;
    }


    public void SetPuzzleSolvedValues(int[] solvedValues)
    {
        for (int i = 0; i < solvedValues.Length; i++)
        {
            puzzlePrefabs[i].solvedValue = solvedValues[i];
        }
    }

    public bool NewBestScore(int solvedValue)
    {
        if (solvedValue < puzzlePrefabs[currentPuzzleIndex].solvedValue)
        {
            puzzlePrefabs[currentPuzzleIndex].solvedValue = solvedValue;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckSolution()
    {
        if (puzzle == null)
        {
            return false;
        }
        else if (currentPuzzleIndex == 0)
        {
            TutorialController(0);
            
            if (tutorialManager.promptIndex != 7)
                return false;
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].heldConfig != puzzle.solution[i])
            {
                Debug.Log("Answer incorrect");
                return false;
            }
        }
        Debug.Log("Correct Answer, Well Done!");
        return true;        
    }

    public void TutorialController(int skipValue)
    {


        if (tutorialManager.promptIndex == 0)
        {
            foreach (Draggable block in blocks)
            {
                GameObject currentHint = block.gameObject.transform.GetChild(0).gameObject;
                currentHint.SetActive(true);
                currentHint.GetComponent<SpriteRenderer>().color = Color.yellow;
            }
        }
        else if (tutorialManager.promptIndex == 1)
        {
            foreach (Draggable block in blocks)
            {
                GameObject currentHint = block.gameObject.transform.GetChild(0).gameObject;
                currentHint.SetActive(false);
                blocks[1].gameObject.SetActive(false);
                MenuManager.gameIsPaused = false;
                tutorialNextPromptButton.interactable = false;
            }
        }
        else if (tutorialManager.promptIndex == 2 && tiles[1].heldConfig == new BlockData(6, 3))
        {
            audioManager.Play("win");
            StartCoroutine(TutorialPhase3());
        }
        else if (tutorialManager.promptIndex == 3 && tiles[4].heldConfig == new BlockData(1, 2))
        {
            audioManager.Play("win");
            StartCoroutine(TutorialPhase4());
        }
        else if (tutorialManager.promptIndex == 4 && tiles[1].heldConfig == new BlockData(6, 3) && tiles[3].heldConfig == new BlockData(1, 2))
        {
            tutorialManager.promptIndex = 5;
            tutorialNextPromptButton.interactable = true;
        }
        else if (tutorialManager.promptIndex == 5)
        {
            foreach (Draggable block in blocks)
            {
                GameObject currentHint = block.gameObject.transform.GetChild(0).gameObject;
                currentHint.SetActive(false);
            }
        }
        else if (tutorialManager.promptIndex == 6)
        {
            solvedHUD.SetActive(true);
            menuManager.UpdateSaveScores(SaveManager.solvedEasy);
            return;
        }

        tutorialManager.promptIndex += skipValue;
        tutorialManager.UpdateDisplayedPrompt();

        IEnumerator TutorialPhase3()
        {
            yield return new WaitForFixedUpdate();
            tutorialManager.tutorialPrompts[2].transform.GetChild(0).gameObject.SetActive(false);
            yield return new WaitForSeconds(1.5f);
            tutorialManager.promptIndex = 3;
            tutorialManager.UpdateDisplayedPrompt();
            blocks[1].gameObject.SetActive(true);
        }

        IEnumerator TutorialPhase4()
        {
            yield return new WaitForFixedUpdate();
            tutorialManager.tutorialPrompts[3].transform.GetChild(0).gameObject.SetActive(false);
            yield return new WaitForSeconds(1.5f);
            tutorialManager.promptIndex = 4;
            tutorialManager.UpdateDisplayedPrompt();
            puzzle.gameObject.SetActive(true);
            foreach (Tile t in tiles)
            {
                t.ExitTile();
            }
            int x = 0, y = 0;
            foreach (Draggable block in blocks)
            {
                block.transform.position = new Vector2(spawnX + x * spawnGap, spawnY + (puzzle.height / 2) - y * spawnGap);
                x++;
            }
            StartCoroutine(dragController.RelightSequence(false));
        }
    }

    public void GiveHint()
    {
        if (puzzle == null)
        {
            return;
        }
    
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].heldBlock == null)
            {
                continue;
            }

            GameObject currentHint = tiles[i].heldBlock.gameObject.transform.GetChild(0).gameObject;
            hintsOnDisplay.Add(currentHint);
            currentHint.SetActive(true);

            if (tiles[i].heldConfig == puzzle.solution[i])            
                currentHint.GetComponent<SpriteRenderer>().color = Color.cyan;            
            else                            
                currentHint.GetComponent<SpriteRenderer>().color = Color.red;            
        }

        dragController.hintsRemaining--;
        hintsRemainingText.text = "" + dragController.hintsRemaining;
    }

    public void RemoveHint()
    {
        foreach (GameObject hint in hintsOnDisplay)
            hint.SetActive(false);

        hintsOnDisplay.Clear();
    }

    public void SeeTiles()
    {
        foreach (Tile idx in tiles)
        {
            idx.gameObject.layer = Layer.Default;
        }
    }

    public void IgnoreTiles()
    {
        foreach (Tile idx in tiles)
        {
            idx.gameObject.layer = Layer.Ignore;
        }
    }

    public void SeeBlocks()
    {
        foreach (Draggable idx in blocks)
        {
            idx.gameObject.layer = Layer.Default;
        }
    }

    public void IgnoreBlocks()
    {
        foreach (Draggable idx in blocks)
        {
            idx.gameObject.layer = Layer.Ignore;
        }
    }
}

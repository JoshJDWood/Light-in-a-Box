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

    private List<Tile> tiles = new List<Tile>();
    private List<Draggable> blocks = new List<Draggable>();
    private List<GameObject> hintsOnDisplay = new List<GameObject>();
    private Puzzle puzzle;
    public int currentPuzzleIndex;
    private CastBeam lightSource;
    private DragController dragController;
    private TutorialManager tutorialManager;
    private AudioManager audioManager;

    private float wallThickness = 0.1f; //actually half wall thinkness
    private int outerWallCount = 0;
    private int outerWallCornerCount = 0;
    private float spawnX = -2.5f;
    private float spawnY = 1.5f;
    private float spawnGap = 1.1f;

    [SerializeField] private Transform cam;

    void Start()
    {
        GenerateGrid(width, height);
        lightSource = FindObjectOfType<CastBeam>();
        dragController = FindObjectOfType<DragController>();
        tutorialManager = FindObjectOfType<TutorialManager>();
        audioManager = FindObjectOfType<AudioManager>();
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
                GameObject newBlock = Instantiate(blockPrefabs[bD.id], new Vector2(spawnX + x * spawnGap, spawnY - y * spawnGap), Quaternion.Euler(new Vector3(0, 0, 90 * bD.r)));
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
            blocks[1].gameObject.SetActive(false);
            puzzle.gameObject.SetActive(false);
        }

        if (!dragController.hardMode)
            StartCoroutine(RelightSequence());
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

    IEnumerator RelightSequence()
    {
        yield return new WaitForFixedUpdate();
        lightSource.Relight();
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


        cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10);
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
            if (tutorialManager.promptIndex == 0 && tiles[1].heldConfig == new BlockData(6,3))
            {
                audioManager.Play("win");
                StartCoroutine(TutorialPhase1());
            }
            else if (tutorialManager.promptIndex == 1 && tiles[4].heldConfig == new BlockData(1, 2))
            {
                audioManager.Play("win");
                StartCoroutine(TutorialPhase2());
            }
            else if (tutorialManager.promptIndex == 2 && tiles[1].heldConfig == new BlockData(6, 3) && tiles[3].heldConfig == new BlockData(1, 2))
            {
                tutorialManager.promptIndex = 3;
                tutorialManager.UpdateDisplayedPrompt();
            }

            if (tutorialManager.promptIndex != 3)
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

        IEnumerator TutorialPhase1()
        {
            yield return new WaitForSeconds(1.5f);
            tutorialManager.promptIndex = 1;
            tutorialManager.UpdateDisplayedPrompt();
            blocks[1].gameObject.SetActive(true);
        }

        IEnumerator TutorialPhase2()
        {
            yield return new WaitForSeconds(1.5f);
            tutorialManager.promptIndex = 2;
            tutorialManager.UpdateDisplayedPrompt();
            puzzle.gameObject.SetActive(true);
            foreach (Tile t in tiles)
            {
                t.ExitTile();
            }
            int x = 0, y = 0;
            foreach (Draggable block in blocks)
            {
                block.transform.position = new Vector2(spawnX + x * spawnGap, spawnY - y * spawnGap);
                x++;
            }
            StartCoroutine(dragController.RelightSequence());
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject outerWall;
    [SerializeField] private GameObject outerWallCorner;
    [SerializeField] private List<GameObject> blockPrefabs;

    private List<Tile> tiles = new List<Tile>();
    private List<Draggable> blocks = new List<Draggable>();
    private Puzzle puzzle;
    private CastBeam lightSource;

    private float wallThickness = 0.1f; //actually half wall thinkness
    private int outerWallCount = 0;
    private int outerWallCornerCount = 0;

    [SerializeField] private Transform cam;

    void Start()
    {
        GenerateGrid(width, height);
        lightSource = FindObjectOfType<CastBeam>();
        foreach(Draggable block in FindObjectsOfType<Draggable>())
        {
            blocks.Add(block);
        }
        puzzle = FindObjectOfType<Puzzle>();
    }

    public void SpawnNewPuzzle(Puzzle puzzle)
    {
        DeleteOldPuzzle();
        GenerateGrid(puzzle.width, puzzle.height);
        lightSource.transform.position = puzzle.lightPos;
        float x = 0, y = 0;
        foreach(BlockData bD in puzzle.solution)
        {
            if (bD.id != 0)
            {
                GameObject newBlock = Instantiate(blockPrefabs[bD.id], new Vector2(-2.5f + x, 1.5f - y), Quaternion.Euler(new Vector3(0, 0, 90 * bD.r)));
                newBlock.GetComponent<Draggable>().UpdateCR();
                blocks.Add(newBlock.GetComponent<Draggable>());

                x = (x + 1) % 2;
                if ( x == 0)
                {
                    y++;
                }
            }
        }
        this.puzzle = Instantiate(puzzle);
        StartCoroutine(RelightSequence());
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
            Destroy(GameObject.Find("Outer Wall Corner" + i));
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
        outerWallspawn.name = $"Outer Wall Corner {outerWallCount}";
        outerWallCornerCount++;
    }

    public bool CheckSolution()
    {
        if (puzzle == null)
        {
            return false;
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].heldConfig.id != puzzle.solution[i].id || tiles[i].heldConfig.r != puzzle.solution[i].r)
            {
                Debug.Log("Answer incorrect");
                return false;
            }
        }
        Debug.Log("Correct Answer, Well Done!");
        return true;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject outerWall;
    [SerializeField] private List<GameObject> blockPrefabs;

    private List<Tile> tiles = new List<Tile>();
    private List<Draggable> blocks = new List<Draggable>();
    private Puzzle puzzle;
    private CastBeam lightSource;

    private float wallThickness = 4f;

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
        for (int i = 1; i < 5; i++)
        {
            Destroy(GameObject.Find("Outer Wall " + i));
        }
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

        SpawnOuterWall(new Vector2(-0.5f + (float)width / 2, -0.5f - wallThickness / 40), new Vector3(width, wallThickness), 0, 1);
        SpawnOuterWall(new Vector2(-0.5f - wallThickness / 40, -0.5f + (float)height / 2), new Vector3(height, wallThickness), 90, 2);
        SpawnOuterWall(new Vector2(-0.5f + (float)width / 2, height - 0.5f + wallThickness / 40), new Vector3(width, wallThickness), 0, 3);
        SpawnOuterWall(new Vector2(width - 0.5f + wallThickness / 40, -0.5f + (float)height / 2), new Vector3(height, wallThickness), 90, 4);

        cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10);
    }

    void SpawnOuterWall(Vector2 pos, Vector3 scale, int rotateAmount, int id)
    {
        GameObject outerWallspawn = Instantiate(outerWall, pos, Quaternion.Euler(new Vector3(0, 0, rotateAmount)));
        outerWallspawn.name = $"Outer Wall {id}";
        outerWallspawn.transform.localScale = scale;
        outerWallspawn.gameObject.layer = Layer.Default;
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

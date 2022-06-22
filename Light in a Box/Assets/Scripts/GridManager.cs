using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    private List<Tile> tiles = new List<Tile>();
    private Draggable[] blocks;

    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject outerWall;
    private float wallThickness = 4f;

    [SerializeField] private Transform cam;

    void Start()
    {
        GenerateGrid();
        blocks = FindObjectsOfType<Draggable>();        
    }

    void GenerateGrid()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Tile spawnedTile = Instantiate(tilePrefab, new Vector2(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
                tiles.Add(spawnedTile);
            }
        }

        SpawnOuterWall(new Vector2(-0.5f + (float)width / 2, -0.5f - wallThickness / 40), new Vector3(width, wallThickness), 0);
        SpawnOuterWall(new Vector2(-0.5f - wallThickness / 40, -0.5f + (float)height / 2), new Vector3(height, wallThickness ), 90);
        SpawnOuterWall(new Vector2(-0.5f + (float)width / 2, height -0.5f + wallThickness / 40), new Vector3(width, wallThickness), 0);
        SpawnOuterWall(new Vector2(width -0.5f + wallThickness / 40, -0.5f + (float)height / 2), new Vector3(height, wallThickness), 90);

        cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10);
    }

    void SpawnOuterWall(Vector2 pos, Vector3 scale, int rotateAmount)
    {
        GameObject outerWallspawn = Instantiate(outerWall, pos, Quaternion.Euler(new Vector3(0, 0, rotateAmount)));
        outerWallspawn.transform.localScale = scale;        
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

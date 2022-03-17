using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;

    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject outerWall;
    private float wallThickness = 0.2f;

    [SerializeField] private Transform cam;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Tile spawnedTile = Instantiate(tilePrefab, new Vector2(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
            }
        }

        SpawnOuterWall(new Vector2(-0.5f + (float)width / 2, -0.5f - wallThickness / 2), new Vector3(width, wallThickness));
        SpawnOuterWall(new Vector2(-0.5f - wallThickness / 2, -0.5f + (float)height / 2), new Vector3(wallThickness, height));
        SpawnOuterWall(new Vector2(-0.5f + (float)width / 2, height -0.5f + wallThickness / 2), new Vector3(width, wallThickness));
        SpawnOuterWall(new Vector2(width -0.5f + wallThickness / 2, -0.5f + (float)height / 2), new Vector3(wallThickness, height));

        cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10);
    }

    void SpawnOuterWall(Vector2 pos, Vector3 scale)
    {
        GameObject outerWallspawn = Instantiate(outerWall, pos, Quaternion.identity);
        outerWallspawn.transform.localScale = scale;        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    public int width;
    public int height;
    public Vector2 lightPos;

    public int solvedValue = 0;

    public List<BlockData> solution = new List<BlockData>();
}

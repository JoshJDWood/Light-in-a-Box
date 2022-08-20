using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    public int width;
    public int height;
    public Vector2 lightPos;

    public int solvedValue = SaveManager.unsolvedVal; //lowest number of: 1000 if unsolved, 999 if solved on easy or # of guesses to solve on hard.

    public List<BlockData> solution = new List<BlockData>();
}

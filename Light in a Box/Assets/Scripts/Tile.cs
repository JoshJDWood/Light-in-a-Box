using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int heldId = 0;
    public int heldRotation = 0;
    public Draggable heldBlock = null;

    private void Update()
    {
        if (heldBlock != null && heldBlock.isDragging)
        {
            ExitTile();
        }
    }

    public void EnterTile(int id, int rotation, Draggable draggable)
    {
        heldId = id;
        heldRotation = rotation;
        heldBlock = draggable;
    }

    public void ExitTile()
    {
        heldId = 0;
        heldRotation = 0;
        heldBlock = null;
    }
}

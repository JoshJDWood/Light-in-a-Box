using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public BlockData heldConfig = new BlockData( 0, 0);
    public Draggable heldBlock = null;

    private void Update()
    {
        if (heldBlock != null && heldBlock.isDragging)
        {
            ExitTile();
        }
    }

    public void EnterTile(Draggable draggable)
    {
        heldConfig = draggable.config;
        heldBlock = draggable;
    }

    public void ExitTile()
    {
        heldConfig.id = 0;
        heldConfig.r = 0;
        heldBlock = null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    public BlockData config;    
    public int oRS;
    public int orderInLayer = 1;
    public bool isDragging;
    public Tile inTile = null;


    public void UpdateSortingOrder(int order)
    {
        gameObject.GetComponent<SpriteRenderer>().sortingOrder = order - 1;
        foreach (Transform child in transform)
        {
            child.GetComponent<SpriteRenderer>().sortingOrder = order;
        }
        orderInLayer = order;
    }    

    public void UpdateCR()
    {
        config.r = (int)(transform.rotation.eulerAngles.z / 90) % oRS;
    }

    public void SeeWalls()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.layer = Layer.Default;
        }
    }

    public void IgnoreWalls()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.layer = Layer.Ignore;
        }
    }
}

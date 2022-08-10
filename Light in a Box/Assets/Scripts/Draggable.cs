using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    public BlockData config;    
    public int oRS;
    public bool isDragging;

    private void Awake()
    {
        DroppedSortingOrder();
    }

    public void DraggingSortingOrder()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<SpriteRenderer>().sortingOrder = 24;
        }
    }

    public void DroppedSortingOrder()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<SpriteRenderer>().sortingOrder = config.id;
        }        
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

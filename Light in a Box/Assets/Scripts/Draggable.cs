using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    public int id;
    public int oRS;
    public int currentRotation = 0;
    public bool isDragging;

    public void UpdateCR()
    {
        currentRotation = (int)(transform.rotation.eulerAngles.z / 90) % oRS;
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

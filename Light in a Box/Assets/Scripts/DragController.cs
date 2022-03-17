using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private bool isDragActive = false;

    private Vector2 screenPos;
    private Vector3 worldPos;
    private Draggable lastDragged;
    private Vector2 dragOffset;

    void Awake()
    {
        cam = Camera.main;
    }


    void Update()
    {
        if(isDragActive && Input.GetMouseButtonUp(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider.CompareTag("ValidTile"))
            {
                Debug.Log("hit had tag of " + hit.collider.tag);
                lastDragged.transform.position = hit.transform.position;
                Drop();
            }
            else
            {
                Debug.Log("invadid hit" + hit.transform.gameObject.name);
                Drop();
            }
            
            return;
        }
        
        if (Input.GetMouseButton(0))
        {
            screenPos = Input.mousePosition;
        }
        else
        {
            return;
        }

        worldPos = cam.ScreenToWorldPoint(screenPos);

        if (isDragActive)
        {
            Drag();
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null)
            {
                Draggable draggable = hit.transform.gameObject.GetComponent<Draggable>();
                if (draggable != null)
                {
                    lastDragged = draggable;
                    dragOffset = lastDragged.transform.position - worldPos;
                    InitDrag();
                }
            }
        }
    }

    void InitDrag()
    {
        UpdateDragStatus(true);
    }

    void Drag()
    {
        lastDragged.transform.position = (Vector2)worldPos + dragOffset;
    }

    void Drop()
    {
        UpdateDragStatus(false);
    }

    void UpdateDragStatus(bool isDragging)
    {
        isDragActive = lastDragged.isDragging = isDragging;
        lastDragged.gameObject.layer = isDragging ? Layer.Invisable : Layer.Default;
    }
}

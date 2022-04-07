using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    private GridManager gridManager;
    private CastBeam lightSource;
    private bool isDragActive = false;

    private Vector2 screenPos;
    private Vector3 worldPos;
    private Draggable lastDragged;
    private Vector2 dragOffset;

    void Awake()
    {
        cam = Camera.main;
        gridManager = FindObjectOfType<GridManager>();
        lightSource = FindObjectOfType<CastBeam>();
    }

    private void Start()
    {
        StartCoroutine(RelightSequence());
    }


    void Update()
    {
        if(isDragActive && Input.GetMouseButtonUp(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("ValidTile"))
            {
                Debug.Log("hit had tag of " + hit.collider.tag);
                lastDragged.transform.position = hit.transform.position;
                Drop();
                StartCoroutine(RelightSequence());
            }
            else
            {
                if (hit.collider != null)
                {
                    Debug.Log("drop and hit the " + hit.transform.gameObject.name);
                }
                else
                {
                    Debug.Log("drop hit nothing");
                }
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
                Debug.Log("pick up and hit the " + hit.transform.gameObject.name);
            }
            else
            {
                Debug.Log("pick up hit nothing");
            }
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
        gridManager.SeeTiles();
        UpdateDragStatus(true);
    }

    void Drag()
    {
        lastDragged.transform.position = (Vector2)worldPos + dragOffset;
    }

    void Drop()
    {
        gridManager.IgnoreTiles();
        UpdateDragStatus(false);
    }

    void UpdateDragStatus(bool isDragging)
    {
        isDragActive = lastDragged.isDragging = isDragging;
        lastDragged.gameObject.layer = isDragging ? Layer.Ignore : Layer.Default;
    }

    IEnumerator RelightSequence()
    {
        yield return new WaitForFixedUpdate();
        gridManager.IgnoreBlocks();
        yield return new WaitForFixedUpdate();
        lightSource.Relight();
        gridManager.SeeBlocks();
        Debug.Log("in coroutine");
    }
}

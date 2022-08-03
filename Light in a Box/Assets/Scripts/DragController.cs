using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject solvedHUD;
    private GridManager gridManager;
    private AudioManager audioManager;
    private CastBeam lightSource;
    public bool isDragActive = false;

    private Vector2 screenPos;
    private Vector3 worldPos;
    private Draggable lastDragged;
    private Vector2 dragOffset;

    void Awake()
    {
        cam = Camera.main;
        gridManager = FindObjectOfType<GridManager>();
        audioManager = FindObjectOfType<AudioManager>();
        lightSource = FindObjectOfType<CastBeam>();
    }

    private void Start()
    {
        StartCoroutine(RelightSequence());
    }


    void Update()
    {
        if(MenuManager.gameIsPaused)
        {            
            return;
        }

        //testing rotation of blocks
        if(isDragActive)
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                lastDragged.transform.Rotate(0, 0, 90);
                lastDragged.UpdateCR();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                lastDragged.transform.Rotate(0, 0, -90);
                lastDragged.UpdateCR();
            }
        }

        if(isDragActive && Input.GetMouseButtonUp(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null && hit.collider.CompareTag("ValidTile"))
            {
                Debug.Log("hit had tag of " + hit.collider.tag);
                lastDragged.transform.position = hit.transform.position;
                Drop();
                hit.transform.gameObject.GetComponent<Tile>().EnterTile(lastDragged);
                solvedHUD.SetActive(gridManager.CheckSolution());               
                lastDragged.SeeWalls();
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
                solvedHUD.SetActive(gridManager.CheckSolution());
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
        audioManager.Play("pickUp");
        lastDragged.IgnoreWalls();
        UpdateDragStatus(true);
        StartCoroutine(RelightSequencePickUp());        
    }

    void Drag()
    {
        lastDragged.transform.position = (Vector2)worldPos + dragOffset;
    }

    void Drop()
    {
        audioManager.Play("drop" + UnityEngine.Random.Range(1, 4));
        gridManager.IgnoreTiles();
        UpdateDragStatus(false);
    }

    public void DropForPause()
    {
        if (isDragActive)
        {
            StartCoroutine(DropOnDelay());
        }
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
        //yield return new WaitForFixedUpdate();
        lightSource.Relight();
        gridManager.SeeBlocks();            
    }

    IEnumerator RelightSequencePickUp()
    {
        yield return new WaitForFixedUpdate();
        gridManager.IgnoreBlocks();
        lightSource.Relight();
        gridManager.SeeBlocks();
        UpdateDragStatus(true);
        gridManager.SeeTiles();
    }

    IEnumerator DropOnDelay()
    {
        yield return new WaitForFixedUpdate();
        Drop();
    }
}

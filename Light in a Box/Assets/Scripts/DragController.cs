using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject solvedHUD;
    private GridManager gridManager;
    private AudioManager audioManager;
    private MenuManager menuManager;
    private CastBeam lightSource;

    public bool isDragActive = false;
    public bool hardMode = false;
    public int hintsRemaining = 100;
    private int guesses = 0;

    private Vector2 screenPos;
    private Vector3 worldPos;
    private Draggable lastDragged;
    private int draggedIndex = 2;
    private Vector2 dragOffset;
    private List<RaycastHit2D> hits = new List<RaycastHit2D>();

    private Vector2 dragSize = new Vector2(1.2f, 1.2f);
    private Vector2 defaultSize = new Vector2(1f, 1f);
    private Coroutine grow;

    void Awake()
    {
        cam = Camera.main;
        gridManager = FindObjectOfType<GridManager>();
        audioManager = FindObjectOfType<AudioManager>();
        menuManager = FindObjectOfType<MenuManager>();
        lightSource = FindObjectOfType<CastBeam>();
    }

    private void Start()
    {
        if (!hardMode)
            StartCoroutine(RelightSequence());
    }


    void Update()
    {
        if (MenuManager.gameIsPaused)
        {
            return;
        }

        //testing rotation of blocks
        if (isDragActive)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
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

        if (Input.GetKeyDown(KeyCode.Return) && hardMode)
        {
            CheckSolutionHardMode();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            GiveHint();
        }

        if (isDragActive && Input.GetMouseButtonUp(0))
        {
            int hitsLength = Physics2D.Raycast(worldPos, Vector2.zero, new ContactFilter2D().NoFilter(), hits, 5.0f);
            for (int i = 0; i < hitsLength; i++)
            {
                if (hits[i].transform.gameObject.GetComponent<Draggable>() == lastDragged)
                {
                    hits.RemoveAt(i);
                    hitsLength--;
                }
            }
            if (hitsLength == 1 && hits[0].collider.CompareTag("ValidTile"))
            {
                Debug.Log("hit had tag of " + hits[0].collider.tag);
                lastDragged.transform.position = hits[0].transform.position;
                Drop();
                hits[0].transform.gameObject.GetComponent<Tile>().EnterTile(lastDragged);
                lastDragged.SeeWalls();
                if (!hardMode)
                {
                    if (gridManager.CheckSolution())
                    {
                        solvedHUD.SetActive(true);
                        menuManager.UpdateSaveScores(SaveManager.solvedEasy);
                    }
                    else
                        solvedHUD.SetActive(false);
                    
                    StartCoroutine(RelightSequence());
                }
            }
            else
            {
                if (hitsLength > 0)
                {
                    Debug.Log("number of hits " + hitsLength);
                    for (int i = 0; i < hitsLength; i++)
                        Debug.Log("drop and hit the " + hits[i].transform.gameObject.name);
                }
                else
                {
                    Debug.Log("drop hit nothing");
                }
                Drop();
                solvedHUD.SetActive(false);
            }
            hits.Clear();
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
            int hitsLength = Physics2D.Raycast(worldPos, Vector2.zero, new ContactFilter2D().NoFilter(), hits, 5.0f);
            Draggable draggable = null;
            int topDraggable = 0;
            for (int i = 0; i < hitsLength; i++)
            {
                Draggable tempDraggable = hits[i].transform.gameObject.GetComponent<Draggable>();
                if (tempDraggable != null && tempDraggable.orderInLayer > topDraggable)
                {
                    draggable = tempDraggable;
                    topDraggable = tempDraggable.orderInLayer;
                }
            }
            if (hitsLength > 0)
            {
                Debug.Log("number of hits " + hitsLength);
                for (int i = 0; i < hitsLength; i++)
                    Debug.Log("drop and hit the " + hits[i].transform.gameObject.name);
            }
            else
            {
                Debug.Log("pick up hit nothing");
            }

            if (draggable != null)
            {
                lastDragged = draggable;
                dragOffset = lastDragged.transform.position - worldPos;
                InitDrag();
            }

        }
    }

    void InitDrag()
    {
        audioManager.Play("pickUp");
        gridManager.RemoveHint();
        grow = StartCoroutine(ResizeDraggable(defaultSize, dragSize, 0.01f));
        lastDragged.UpdateSortingOrder(draggedIndex);
        draggedIndex++;
        lastDragged.IgnoreWalls();
        UpdateDragStatus(true);
        if (!hardMode)
            StartCoroutine(RelightSequencePickUp());
        else
        {
            lightSource.LightOff();
            gridManager.SeeTiles();
        }
    }

    void Drag()
    {
        lastDragged.transform.position = (Vector2)worldPos + dragOffset;
    }

    void Drop()
    {
        audioManager.Play("drop" + UnityEngine.Random.Range(1, 4));
        StopCoroutine(grow);
        lastDragged.transform.localScale = defaultSize;
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

    public void CheckSolutionHardMode()
    {
        if (!isDragActive)
        {
            guesses++;
            if (gridManager.CheckSolution())
            {
                solvedHUD.SetActive(true);
                menuManager.UpdateSaveScores(guesses);
                guesses = 0;
            }
            StartCoroutine(RelightSequence());
        }
    }

    public void GiveHint()
    {
        if (hintsRemaining > 0 && !isDragActive)
        {
            gridManager.GiveHint();
        }
    }

    //*****//for resizing while dragging//*****//
    IEnumerator ResizeDraggable(Vector2 startSize, Vector2 endSize, float rate)
    {
        for (float i = 0; i <= 1; i += 0.1f)
        {
            lastDragged.transform.localScale = Vector2.Lerp(startSize, endSize, i);
            yield return new WaitForSeconds(rate);
        }
    }

    IEnumerator RelightSequence()
    {
        yield return new WaitForFixedUpdate();
        gridManager.IgnoreBlocks();
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

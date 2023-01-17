using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject solvedHUD;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private CastBeam lightSource;

    public bool isDragActive = false;
    public bool hardMode = false;
    public int hintsRemaining = 10;
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
    }

    private void Start()
    {
        if (!hardMode)
            StartCoroutine(RelightSequence(false));
    }


    void Update()
    {
        if (MenuManager.gameIsPaused)
        {
            return;
        }

        //testing rotation of blocks
        //if (isDragActive)
        //{
        //    if (Input.GetKeyDown(KeyCode.LeftArrow))
        //    {
        //        lastDragged.transform.Rotate(0, 0, 90);
        //        lastDragged.UpdateCR();
        //    }
        //    else if (Input.GetKeyDown(KeyCode.RightArrow))
        //    {
        //        lastDragged.transform.Rotate(0, 0, -90);
        //        lastDragged.UpdateCR();
        //    }
        //}

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
            int hitsLength = Physics2D.Raycast(lastDragged.transform.position, Vector2.zero, new ContactFilter2D().NoFilter(), hits, 5.0f);
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
                        StartCoroutine(DelayShowSolvedHUD());
                        audioManager.Play("win");
                        menuManager.UpdateSaveScores(SaveManager.solvedEasy);
                        MenuManager.gameIsPaused = true;
                        StartCoroutine(RelightSequence(true));
                    }
                    else
                    {
                        solvedHUD.SetActive(false);
                        StartCoroutine(RelightSequence(false));
                    }
                    
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
        lastDragged.gameObject.GetComponent<SpriteRenderer>().sortingOrder = -2;
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
                StartCoroutine(DelayShowSolvedHUD());
                audioManager.Play("win");
                menuManager.UpdateSaveScores(guesses);
                guesses = 0;
                MenuManager.gameIsPaused = true;
                StartCoroutine(RelightSequence(true));
            }
            else
            {
                StartCoroutine(RelightSequence(false));
            }
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

    public IEnumerator RelightSequence(bool beatPuzzle)
    {
        yield return new WaitForFixedUpdate();
        gridManager.IgnoreBlocks();
        lightSource.Relight(beatPuzzle);
        gridManager.SeeBlocks();
    }

    IEnumerator RelightSequencePickUp()
    {
        yield return new WaitForFixedUpdate();
        gridManager.IgnoreBlocks();
        lightSource.Relight(false);
        gridManager.SeeBlocks();
        UpdateDragStatus(true);
        gridManager.SeeTiles();
    }

    IEnumerator DelayShowSolvedHUD()
    {
        yield return new WaitForSeconds(2f);
        solvedHUD.SetActive(true);
    }

    IEnumerator DropOnDelay()
    {
        yield return new WaitForFixedUpdate();
        Drop();
    }
}

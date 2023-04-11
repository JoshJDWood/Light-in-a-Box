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
    [SerializeField] private RewardedAdsButton rewardedAdsButton;

    public bool isDragActive = false;
    public bool hardMode = false;
    public int hintsRemaining = 3;
    private int guesses = 0;

    private Vector2 screenPos;
    private Vector3 worldPos;
    private Draggable lastDragged;
    private Vector2 lastPickupPos;
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

        //if (Input.GetKeyDown(KeyCode.Return) && hardMode)
        //{
        //    CheckSolutionHardMode();
        //}
        //
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    GiveHint();
        //}

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
                //Debug.Log("hit had tag of " + hits[0].collider.tag);
                lastDragged.transform.position = hits[0].transform.position;
                Drop(hits[0].transform.gameObject.GetComponent<Tile>(), UnityEngine.Random.Range(1, 3));
                if (!hardMode)
                {
                    CheckSolution();
                }
            }
            else
            {
                if (hitsLength > 0)
                {
                    //Debug.Log("number of hits " + hitsLength);
                    //for (int i = 0; i < hitsLength; i++)
                    //    Debug.Log("drop and hit the " + hits[i].transform.gameObject.name);
                    lastDragged.transform.position = lastPickupPos; //if dropped on something other that an empty tile will jump back to old position
                    Drop(lastDragged.inTile, 3);//allows to jump back into tile its just left
                    if (!hardMode && lastDragged.inTile != null)
                    {
                        CheckSolution();
                    }
                }
                else
                {
                    //Debug.Log("drop hit nothing");
                    Drop(null, UnityEngine.Random.Range(1, 3));
                }                
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
                //Debug.Log("number of hits " + hitsLength);
                //for (int i = 0; i < hitsLength; i++)
                //    Debug.Log("drop and hit the " + hits[i].transform.gameObject.name);
            }
            else
            {
                //Debug.Log("pick up hit nothing");
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
        lastPickupPos = lastDragged.transform.position;
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

    void Drop(Tile inTile, int dropSound)
    {
        audioManager.Play("drop" + dropSound);
        StopCoroutine(grow);
        lastDragged.transform.localScale = defaultSize;
        lastDragged.gameObject.GetComponent<SpriteRenderer>().sortingOrder = -2;
        gridManager.IgnoreTiles();
        UpdateDragStatus(false);
        lastDragged.inTile = inTile;
        if (inTile != null)
        {
            lastDragged.SeeWalls();
            inTile.EnterTile(lastDragged);
        }
    }

    public void DropForPause()
    {
        if (isDragActive)
        {
            StartCoroutine(DropOnDelay());
        }
    }

    private void CheckSolution()
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

    void UpdateDragStatus(bool isDragging)
    {
        isDragActive = lastDragged.isDragging = isDragging;
        lastDragged.gameObject.layer = isDragging ? Layer.Ignore : Layer.Default;
    }

    public void ResetGuesses()
    {
        guesses = 0;
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
        if (isDragActive || (gridManager.currentPuzzleIndex == -1 && hintsRemaining > 0))
        {
            return;
        }

        if (hintsRemaining > 0)
        {
            gridManager.GiveHint();
        }
        else
        {
            if (rewardedAdsButton.GetAdIsLoaded())
            {
                rewardedAdsButton.ShowAd();
            }
            else
            {
                menuManager.ShowAdFailedPopup();
            }
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
        menuManager.ShowSolvedHUD(false);
    }

    IEnumerator DropOnDelay()
    {
        yield return new WaitForFixedUpdate();
        Drop(null, UnityEngine.Random.Range(1, 3));
    }
}

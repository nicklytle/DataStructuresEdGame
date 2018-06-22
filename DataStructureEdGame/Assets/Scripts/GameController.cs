using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public int debugLinkControlVersion;

    public WorldGenerationBehavior worldGenerator;
    // a referernce to all objective entities in the level
    private List<ObjectiveBlockBehavior> objectiveBlocks;
    // a reference to all platform entities in the level.
    private List<PlatformBehavior> platformEntities;

    // a queue of platforms that may be added in this level
    public List<PlatformBehavior> platformsToAdd;
    // whether the player is currently in an the Add Platform mode.
    public bool addingPlatforms = false;

    // to help track for double clicking
    public DateTime lastTimeClickedMillis;

    // the Prefab for line renderer stuff.
    public Transform linePreFab;
    // References to the hover arrow parts showing a preview of the arrow.
    public Transform hoverArrowLine;
    public Transform hoverArrowHead;

    public Texture2D pointerCursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;

    // different win conditions for the level.
    public enum WinCondition
    {
        None,
        SortListAscending,
        SortListDescending
    }

    // The win condition of this level.
    public GameController.WinCondition winConditon;

    // References to important objects in the scene. 
    public Transform playerRef;
    public Transform helicopterRobotRef;
    public LinkBlockBehavior selectedLink; // what Link block the player is adding a connection to, if any
    
    // Linked list properties
    public LinkBlockBehavior startingLink; // what is this level's starting link block?
    public PlatformBehavior hoverPlatformRef; // the platform the mouse is hovering over for version 1.
    public LinkBlockBehavior hoverLinkRef; // the link block the mouse is hovering over. 

    // references to important UI elements.
    public Text statusTextUI;
    public Image objectiveHudPanelUI;
    public Text objectiveTextUI;

    void Start()
    {
        selectedLink = null; 
        setStatusText("");
        // ensure the starting link has the proper property
        if (startingLink != null)
        {
            startingLink.isStartingLink = true;
        }
        platformEntities = new List<PlatformBehavior>();
        objectiveBlocks = new List<ObjectiveBlockBehavior>();
    }

    void Update()
    {
        if (playerRef != null)
        {
            // always set the camera on top of the player.
            transform.position = new Vector3(playerRef.position.x, playerRef.position.y, transform.position.z);
        }

        // add platform system
        if (addingPlatforms && Input.GetMouseButtonDown(0))
        {
            if (platformsToAdd.Count > 0)
            {
                Debug.Log("Adding platform now..");
                Debug.Log(platformsToAdd.Count);
                PlatformBehavior toBeAdded = platformsToAdd[0];
                if (toBeAdded != null)
                {
                    platformsToAdd.Remove(toBeAdded);
                    Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector3 positionMcPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    positionMcPosition.z = 0;
                    Debug.Log(positionMcPosition);
                    toBeAdded.transform.position = positionMcPosition;
                    toBeAdded.gameObject.SetActive(true);
                }
            }
        }

        if (debugLinkControlVersion == 0)
        {  // Link -> Platform controls
            if (selectedLink != null && !Input.GetMouseButton(0)) // mouse was released 
            {
                if (hoverPlatformRef != null)
                {
                    selectedLink.setConnectingPlatform(hoverPlatformRef);
                }
                // deselect when you release the mouse button.
                setHoverPlatformReference(null);
                setSelectedLink(null);
            }
        }
        else if (debugLinkControlVersion == 1)
        {

            if (Input.GetMouseButtonDown(0))
            {
                if (selectedLink == null && hoverLinkRef != null)
                {
                    setSelectedLink(hoverLinkRef);
                } else if (selectedLink != null && hoverLinkRef != null && selectedLink == hoverLinkRef)
                {
                    Debug.Log("DOUBLE CLICK");
                    setSelectedLink(null);
                    hoverLinkRef.removeLinkConnection();
                    setStatusText("Removed link");
                    updateObjectiveHUDAndBlocks(); // update any objective blocks
                    updatePlatformEntities();
                } else if (selectedLink != null && hoverLinkRef == null)
                {
                    Debug.Log("Deselect");
                    setSelectedLink(null); // deselect adding link to deselect
                    setStatusText("Deselected link block");
                    updateObjectiveHUDAndBlocks(); // update any objective blocks
                    updatePlatformEntities();
                }
            } else if (!Input.GetMouseButton(0))
            {
                if (selectedLink != null && hoverLinkRef != null && hoverLinkRef != selectedLink)
                {
                    // establish connection 
                    Debug.Log("release mouse with hoverLink and selectedLink not null");
                    if (hoverLinkRef.parentPlatform == null || selectedLink.parentPlatform != hoverLinkRef.parentPlatform)
                    {
                        Debug.Log("Establishing connection");
                        // this means there is a valid connection!
                        // before establishing the connection for the addingLink, remove any links current there.
                        if (selectedLink.isConnectedToPlatform())
                        {
                            selectedLink.removeLinkConnection();
                        }
                        selectedLink.setConnectingPlatform(hoverLinkRef.connectingPlatform);
                        removeHoverArrow();
                        setStatusText("Established a connection.");
                    }
                    setSelectedLink(null);
                    updateObjectiveHUDAndBlocks(); // update any objective blocks
                    updatePlatformEntities();
                } else if (selectedLink != null && hoverLinkRef != selectedLink)
                {
                    Debug.Log("Deselect");
                    setSelectedLink(null); // deselect adding link to deselect
                    setStatusText("Deselected link block");
                    updateObjectiveHUDAndBlocks(); // update any objective blocks
                    updatePlatformEntities();
                }
            }
        }


        if (Input.GetMouseButtonDown(0))
        {
            lastTimeClickedMillis = System.DateTime.Now;
        }
    }


    bool isWinConditonSatisfied()
    {
        if (winConditon == WinCondition.None)
        {
            return true; // always beat it when there is no condition.
        }

        switch (winConditon)
        {
            case WinCondition.SortListAscending:
            case WinCondition.SortListDescending:
                int sizeOfList = 0;
                if (startingLink == null)
                    return false;

                // count how many platforms are in the level.
                int numberOfTotalPlatformsInLevel = 0;
                foreach (PlatformBehavior pb in platformEntities)
                {
                    if (pb.isInLevel)
                    {
                        numberOfTotalPlatformsInLevel++;
                    }
                }
                //Debug.Log("Number of platforms: " + numberOfTotalPlatformsInLevel);

                if (startingLink.connectingPlatform == null)
                {
                    return (numberOfTotalPlatformsInLevel == 0); // if there are no platforms in the level
                } else if (startingLink.connectingPlatform.childLink.GetComponent<LinkBlockBehavior>().connectingPlatform == null)
                {
                    return (numberOfTotalPlatformsInLevel == 1); // if there is only one platform in the level
                }
                sizeOfList = 1;
                PlatformBehavior temp = startingLink.connectingPlatform.GetComponent<PlatformBehavior>();
                while (temp != null)
                {
                    PlatformBehavior next = temp.childLink.GetComponent<LinkBlockBehavior>().connectingPlatform;
                    if (next == null)
                    {
                        break;
                        //return true;
                    }
                    else
                    {
                        if ((winConditon == WinCondition.SortListAscending && next.getValue() < temp.getValue()) ||
                            (winConditon == WinCondition.SortListDescending && next.getValue() > temp.getValue()))
                        {
                            //Debug.Log("Not sorted");
                            return false; // not sorted.
                        } // otherwise just keep on iterating.
                    }
                    temp = next;
                    sizeOfList++;
                }
                //Debug.Log("Size of the list: " + sizeOfList);
                return (sizeOfList == numberOfTotalPlatformsInLevel); // the list is sorted if all platforms in the level are in the list.
        }
        return false;
    }

    /**
     * Set what LinkBlock is being added by the Player.
     */
    public void setSelectedLink(LinkBlockBehavior aLink)
    {
        if (selectedLink != null)
        {
            selectedLink.setDisplaySelected(false, true);
        }
        selectedLink = aLink; // do other data processing?

        if (selectedLink != null)
        { 
            selectedLink.setDisplaySelected(true, true);
        }
    }


    public void setHoverLink(LinkBlockBehavior lb)
    {
        if (hoverLinkRef != null)
        {
            if (debugLinkControlVersion == 1)
            {
                removeHoverArrow();
                if (hoverLinkRef != selectedLink) { // don't remove the marker on the selected link
                    hoverLinkRef.setDisplaySelected(false);
                }
            }
        }

       /* if (debugLinkControlVersion == 1)
        {
            if (hoverLinkRef.connectingPlatform == null)
            {

            } else
            {
                if (!hoverLinkRef.connectingPlatform.isPhasedOut)
                {
                    // only properly set the hover link if it would make a valid link.
                    hoverLinkRef = lb;
                }
            }
        } else
        {*/
            hoverLinkRef = lb;
        //}


        if (hoverLinkRef != null && hoverLinkRef != selectedLink) // can't set the hover link to the selected link
        { 
            if (debugLinkControlVersion == 1) // hover it to say that you can interact with it. 
            {
                hoverLinkRef.setDisplaySelected(true); 
                // faded arrow to show the outcome
                removeHoverArrow();
                if (selectedLink != null && hoverLinkRef != null && selectedLink != hoverLinkRef &&
                    hoverLinkRef.connectingPlatform != null)
                {
                    hoverLinkRef.setDisplaySelected(true);
                    Color c = Color.gray;
                    c.a = 0.3f;
                    Transform[] hoverArrowParts = createArrowInstanceBetweenLinkPlatform(selectedLink, hoverLinkRef.connectingPlatform, c);
                    hoverArrowLine = hoverArrowParts[0];
                    hoverArrowHead = hoverArrowParts[1];

                    setStatusText("Release to set the first link equal to this one.");
                }
            }
        } 
    }

    /**
     * Set the game's status text
     */
    public void setStatusText(string t)
    {
        statusTextUI.text = t;
    }

    /**
     * Set what Platform is being connected to by the player. 'addingLink' must not be null.
     */ 
    public void setHoverPlatformReference(PlatformBehavior platform)
    {
        if (selectedLink != null /*&& platform != null && !platform.isPhasedOut*/ ) // you can only connect to a platform when there is a Link you have selected.
        {
            if (selectedLink.parentPlatform != null && selectedLink.parentPlatform == platform)
                return; // don't change anything if the platform is the parent of the adding link.
            
            if (hoverPlatformRef != null)
            {
                if (debugLinkControlVersion == 0) { 
                    removeHoverArrow();
                }
                hoverPlatformRef.setDisplaySelected(false);
            }
            hoverPlatformRef = platform;
            if (selectedLink != null && hoverPlatformRef != null) // only display it for this platform if you're also selecting a Link.
            {
                if (debugLinkControlVersion == 0)
                {
                    removeHoverArrow();
                    Color c = Color.gray;
                    c.a = 0.3f;
                    Transform[] hoverArrowParts = createArrowInstanceBetweenLinkPlatform(selectedLink, hoverPlatformRef, c);
                    hoverArrowLine = hoverArrowParts[0];
                    hoverArrowHead = hoverArrowParts[1];
                } 
                hoverPlatformRef.setDisplaySelected(true);
            }
        }
    }

    /**
     * This will update the objective block states based on the win condition.
     * It will also update the HUD for saying what the objective is and its state.
     */ 
    public void updateObjectiveHUDAndBlocks()
    {
        // Debug.Log("Checking win!");
        if (winConditon != WinCondition.None)
        {
            objectiveHudPanelUI.gameObject.SetActive(true);
            objectiveTextUI.gameObject.SetActive(true);

            bool isWinSatisfied = isWinConditonSatisfied();
            // update the hud
            if (isWinSatisfied)
            {
                objectiveHudPanelUI.color = new Color(0, 1, 0, (160.0f / 255.0f));
                objectiveTextUI.text = "Sort the list in ascending order\nThe List is sorted!";
            }
            else
            {
                objectiveHudPanelUI.color = new Color(1f, 0.02f, 0.02f, (160.0f / 255.0f));
                objectiveTextUI.text = "Sort the list in ascending order\nThe List is not sorted!";
            }
            // update the blocks
            for (int i = 0; i < objectiveBlocks.Count; i++)
            {
                objectiveBlocks[i].updatedFadedState(isWinSatisfied);
            }
        } else
        {
            objectiveHudPanelUI.gameObject.SetActive(false);
            objectiveTextUI.gameObject.SetActive(false);
        }
    }

    public void updatePlatformEntities()
    {
        for (int i = 0; i < platformEntities.Count; i++)
        {
            platformEntities[i].updatePlatformValuesAndSprite();
        }
        // update the links after updating the states of the platform
        for (int i = 0; i < platformEntities.Count; i++)
        {
            bool old_state = platformEntities[i].childLink.activeSelf;
            platformEntities[i].childLink.SetActive(true);
            platformEntities[i].childLink.GetComponent<LinkBlockBehavior>().UpdateLinkArrow();
            platformEntities[i].childLink.SetActive(old_state);
        }
    }

    /**
     * Remove the hover arrow if it is there
     */
    public void removeHoverArrow()
    {
        if (hoverArrowLine != null)
        {
            Destroy(hoverArrowLine.gameObject);
            Destroy(hoverArrowHead.gameObject);
            hoverArrowLine = null;
            hoverArrowHead = null;
        }
    }


    public void setLevelPlatformEntitiesList(List<PlatformBehavior> plist)
    {
        platformEntities.AddRange(plist);
    }

    public void setLevelObjectiveBlocksList(List<ObjectiveBlockBehavior> oblist)
    {
        objectiveBlocks.AddRange(oblist);
    }

    public void clearReferenceLists()
    {
        objectiveBlocks.Clear();
        platformEntities.Clear();
    }

    /**
     * Instantiate an arrow that goes from the given link block to the given platform using the given color.
     */
    public Transform[] createArrowInstanceBetweenLinkPlatform(LinkBlockBehavior lb, PlatformBehavior pb, Color color)
    {
        // determine the start and end points of the arrow.
        Bounds linkBounds = lb.GetComponent<SpriteRenderer>().bounds; // the bounds for this link block.;
        /*if (lb.parentPlatform != null) // If this link is a child link, then the parent platform's bounds is the link bounds for rendering
        {
            linkBounds = lb.parentPlatform.GetComponent<SpriteRenderer>().bounds;
        }
        */
        Bounds platBounds = pb.GetComponent<SpriteRenderer>().bounds;

        // find the closest points on both bounding boxes to the center point to make the arrow.
        Vector3 betweenPoint = new Vector3((linkBounds.center.x + platBounds.center.x) / 2,
            (linkBounds.center.y + platBounds.center.y) / 2, 0);
        Vector3 closestToLink = linkBounds.ClosestPoint(betweenPoint);
        closestToLink = new Vector3(closestToLink.x, closestToLink.y, 0);
        Vector3 closestToPlat = platBounds.ClosestPoint(betweenPoint);
        closestToPlat = new Vector3(closestToPlat.x, closestToPlat.y, 0); 
        return createArrowInstanceBetweenPoints(closestToLink, closestToPlat, color);
    }

    /**
     * Instantiate an arrow that goes from the first point to the second using the given color.
     */
    public Transform[] createArrowInstanceBetweenPoints(Vector3 pFrom, Vector3 pTo, Color color)
    {
        // index 0 is the line; index 1 is the head.
        Transform[] arrowParts = new Transform[2];
        arrowParts[0] = Instantiate(linePreFab, pFrom, Quaternion.identity);
        arrowParts[1] = Instantiate(linePreFab, pFrom, Quaternion.identity);
        LineRenderer lineRenderer = arrowParts[0].GetComponent<LineRenderer>();
        LineRenderer lineRendererHead = arrowParts[1].GetComponent<LineRenderer>();

        lineRenderer.enabled = true;
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRendererHead.enabled = true;
        lineRendererHead.startColor = color;
        lineRendererHead.endColor = color;
        lineRendererHead.startWidth = 0.5f;
        lineRendererHead.endWidth = 0f;

        Vector3 zOffset = new Vector3(0, 0, -10);
        Vector3[] linePos = new Vector3[2];
        linePos[0] = pFrom + zOffset;
        linePos[1] = pTo + zOffset;

        float headLength = 0.25f;
        Vector3 diffNorm = (pTo - pFrom).normalized;
        Vector3[] linePosHead = new Vector3[2];
        linePosHead[0] = pTo - (diffNorm * headLength) + zOffset;
        linePosHead[1] = pTo + zOffset;

        lineRenderer.SetPositions(linePos);
        lineRendererHead.SetPositions(linePosHead);
        return arrowParts;
    }
}

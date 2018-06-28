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
        // initial world generation
        worldGenerator.ManualStartGenerator();
    }

    void Update()
    {
        if (playerRef != null)
        {
            // always set the camera on top of the player.
            transform.position = new Vector3(playerRef.position.x, playerRef.position.y, transform.position.z);
        }

        // add platform system

        if (addingPlatforms)
        {
            // you are not clicking or holding down the mouse and there is no select link.  OR you have a select link and you are holding down the mouse button
            if ((!Input.GetMouseButton(0) && selectedLink == null) || (selectedLink != null && Input.GetMouseButton(0))) 
            {
                Debug.Log("cond a");
                if ((platformsToAdd.Count > 0) && (platformsToAdd[0] != null))
                {
                    Debug.Log("cond b");
                    // show a faded preview of the platform
                    PlatformBehavior platToDisplayAndAdd = platformsToAdd[0];
                    platToDisplayAndAdd.GetComponent<SpriteRenderer>().sprite = platToDisplayAndAdd.phasedOutSprite;

                    platToDisplayAndAdd.childLink.GetComponent<SpriteRenderer>().material = platToDisplayAndAdd.fadedChildMaterial;
                    platToDisplayAndAdd.childValueBlock.GetComponent<SpriteRenderer>().material = platToDisplayAndAdd.fadedChildMaterial;

                    int val = platformsToAdd[0].getValue();
                    platToDisplayAndAdd.setValueBlockText("" + val + ""); // can't see the value

                    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    pos.z = 0;
                    platToDisplayAndAdd.transform.position = pos;
                    pos.z = 0;
                    platToDisplayAndAdd.gameObject.SetActive(true);

                    // you have a select link and you are holding down the mouse button... make a faded arrow
                    if (selectedLink != null && Input.GetMouseButton(0)) { 
                        // make a faded hover arrow going to this guy
                        if (hoverArrowHead != null)
                        {
                            removeHoverArrow();
                        }
                        // make a faded hover arrow from the selected link to the proposed platform location
                        Color c = Color.gray;
                        c.a = 0.3f;
                        Transform[] hoverArrowParts = createArrowInstanceBetweenLinkPlatform(selectedLink, platToDisplayAndAdd, c);
                        hoverArrowLine = hoverArrowParts[0];
                        hoverArrowHead = hoverArrowParts[1];
                    }
                }
            } 
            // you have a select link and you have released the mouse button
            else if (selectedLink != null && Input.GetMouseButtonUp(0))
            {
                if (platformsToAdd.Count > 0)
                { 
                    PlatformBehavior toBeAdded = platformsToAdd[0];
                    if (toBeAdded != null)
                    {
                        platformsToAdd.Remove(toBeAdded);
                        Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        Vector3 positionMcPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        positionMcPosition.z = 0;
                        toBeAdded.transform.position = positionMcPosition;
                        toBeAdded.gameObject.SetActive(true);
                        selectedLink.setConnectingPlatform(toBeAdded);
                        addingPlatforms = false;
                        String timestamp1 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        Debug.Log("Platform is added from link " + selectedLink.logId + " at (" + positionMcPosition.x + ", " + positionMcPosition.y + ") at time :" + timestamp1);
                    }
                }
                //to remove that gray 'prediction' arrow now that you've added the platform
                if (hoverArrowHead != null)
                {
                    removeHoverArrow();
                }
            } else if (hoverLinkRef == null && selectedLink == null && Input.GetMouseButtonDown(0))
            {
                String timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Debug.Log("Clicked without clicking on a hover link and a select link at time: " + timestamp);
                if ((platformsToAdd.Count > 0) && (platformsToAdd[0] != null))
                {
                    platformsToAdd[0].gameObject.SetActive(false); // cancel placing the platform
                    addingPlatforms = false;
                }
            }
        } // end addingPlatforms block

        
        // handle just clicking the mouse button
        if (Input.GetMouseButtonDown(0))
        {
            // if you don't have any link selected and you're hovering over a link
            if (selectedLink == null && hoverLinkRef != null)
            {
                setSelectedLink(hoverLinkRef);

            }  // if you're selecting a link and also hovering over the select link and clicking
            else if (selectedLink != null && hoverLinkRef != null && selectedLink == hoverLinkRef)
            {
                String timestamp3 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Debug.Log("the link block " + selectedLink.logId + " double clicked had an existing link so now it's deleted at time: " + timestamp3);

                setSelectedLink(null);
                if (hoverLinkRef.connectingPlatform != null) { 
                    hoverLinkRef.removeLinkConnection();
                    setStatusText("Removed link");

                }
                updateObjectiveHUDAndBlocks(); // update any objective blocks
                updatePlatformEntities();
            } // if you just clicked and you have a link selected and you're not hovering over anything.
            else if (selectedLink != null && hoverLinkRef == null)
            {
                // Debug.Log("Deselect");

                String timestamp4 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Debug.Log("the link block " + selectedLink.logId + " was deselected at time: " + timestamp4);

                setSelectedLink(null); // deselect adding link to deselect
                setStatusText("Deselected link block");
                updateObjectiveHUDAndBlocks(); // update any objective blocks
                updatePlatformEntities();
            }
        } // if you are not clicking and not holding down the mouse button
        else if (!Input.GetMouseButton(0))
        {
            if (selectedLink != null && hoverLinkRef != null && hoverLinkRef != selectedLink)
            {
                // establish connection  
                if (hoverLinkRef.parentPlatform == null || selectedLink.parentPlatform != hoverLinkRef.parentPlatform)
                {
                    // Debug.Log("Establishing connection");
                    // this means there is a valid connection!
                    // before establishing the connection for the addingLink, remove any links current there.
                    if (selectedLink.isConnectedToPlatform())
                    {
                        selectedLink.removeLinkConnection();
                    }
                    if (hoverLinkRef.connectingPlatform != null && hoverLinkRef.connectingPlatform != selectedLink.parentPlatform)
                    {
                        selectedLink.setConnectingPlatform(hoverLinkRef.connectingPlatform);
                        setStatusText("Established a connection.");
                    }
                    removeHoverArrow();
                    setStatusText("Established a connection.");

                    String timestamp5 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    Debug.Log("Connection made: " + selectedLink.logId + " was clicked and dragged to " + hoverLinkRef.logId + " at time: " + timestamp5);
                }

                setSelectedLink(null);
                updateObjectiveHUDAndBlocks(); // update any objective blocks
                updatePlatformEntities();
            } else if (selectedLink != null && hoverLinkRef != selectedLink)
            {
                // Debug.Log("Deselect");
                setSelectedLink(null); // deselect adding link to deselect
                setStatusText("Deselected link block");
                updateObjectiveHUDAndBlocks(); // update any objective blocks
                updatePlatformEntities();
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
        LinkBlockBehavior oldHoverLink = hoverLinkRef;
        hoverLinkRef = lb;
        // update the old hover link that is no longer hovered over.
        if (oldHoverLink != null)
        {
            removeHoverArrow();
            if (oldHoverLink != selectedLink) { // don't remove the marker on the selected link
                oldHoverLink.setDisplaySelected(false);
            }
            // update the next connecting platform for when there was a bridge from this to the next link.
            if (oldHoverLink.connectingPlatform != null)
            {
                //Debug.Log("Remove the bridge");
                oldHoverLink.GetComponent<PolygonCollider2D>().points = new Vector2[0];  // remove the "bridge" collider
                oldHoverLink.GetComponent<LineRenderer>().SetPositions(new Vector3[0]);
                oldHoverLink.GetComponent<LineRenderer>().positionCount = 0;
                oldHoverLink.connectingPlatform.updatePlatformValuesAndSprite();
            }
        }
         
        // update the new hover link if there is one.
        if (hoverLinkRef != null && hoverLinkRef != selectedLink) // can't set the hover link to the selected link
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

                // This is when the mouse is hovering over a link for establishing a link.
                // update and create a "bridge" from this link to the next for next->next->.. option.
                if (hoverLinkRef.connectingPlatform != null)
                {
                    Debug.Log("Create the 'next' bridge");
                    // the line goes from the hover block to the other block.
                    // http://mathworld.wolfram.com/PerpendicularVector.html
                    Bounds otherBounds = hoverLinkRef.connectingPlatform.childLink.GetComponent<SpriteRenderer>().bounds; 
                    Bounds hoverBounds = hoverLinkRef.GetComponent<SpriteRenderer>().bounds;

                    // find the closest points on both bounding boxes to the center point to make the arrow.
                    Vector3 betweenPoint = transform.InverseTransformPoint(
                        new Vector3((otherBounds.center.x + hoverBounds.center.x) / 2, 
                                (otherBounds.center.y + hoverBounds.center.y) / 2, 0));
                    Vector3 p0 = otherBounds.center - hoverBounds.center; // (otherBounds.ClosestPoint(betweenPoint) - hoverBounds.center);  // convert from world to local points
                    Vector3 p1 = new Vector3(); // (hoverBounds.ClosestPoint(betweenPoint) - hoverBounds.center);
                    Vector3 diff = p0 - p1;
                    float scalePerpScale = 0.5f;
                    Vector3 perpDiff = (new Vector2(-diff.y, diff.x)).normalized;
                    Vector2[] bridgePoints = new Vector2[4];
                    bridgePoints[0] = p0 + (scalePerpScale * perpDiff);
                    bridgePoints[1] = p0 - (scalePerpScale * perpDiff);
                    bridgePoints[2] = p1 - (scalePerpScale * perpDiff);
                    bridgePoints[3] = p1 + (scalePerpScale * perpDiff);
                    // verify where the lines are located
                    Vector3 md = new Vector3(0, 0, 40);
                    Debug.DrawLine(transform.TransformPoint(p0) + md, transform.TransformPoint(p1) + md, Color.green, 3, false);

                    Vector3[] linePositions = new Vector3[2];
                    linePositions[0] = p0 + md;
                    linePositions[1] = p1 + md;
                    // make sure line is being drawn in the right spot 
                    Debug.Log(linePositions);
                    Debug.Log(linePositions[0]);
                    Debug.Log(linePositions[1]);

                    // TODO: Transform the collider to go from world space to local space;
                    hoverLinkRef.GetComponent<PolygonCollider2D>().points = bridgePoints;
                    hoverLinkRef.GetComponent<LineRenderer>().startColor = Color.cyan;
                    hoverLinkRef.GetComponent<LineRenderer>().endColor = Color.cyan;
                    hoverLinkRef.GetComponent<LineRenderer>().positionCount = linePositions.Length;
                    hoverLinkRef.GetComponent<LineRenderer>().SetPositions(linePositions);
                    hoverLinkRef.GetComponent<LineRenderer>().startWidth = scalePerpScale;
                    hoverLinkRef.GetComponent<LineRenderer>().endWidth = scalePerpScale;
                    Debug.Log(hoverLinkRef.GetComponent<LineRenderer>().positionCount);
                    hoverLinkRef.connectingPlatform.updatePlatformValuesAndSprite();
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
                hoverPlatformRef.setDisplaySelected(false);
            }
            hoverPlatformRef = platform;
            if (selectedLink != null && hoverPlatformRef != null) // only display it for this platform if you're also selecting a Link.
            { 
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

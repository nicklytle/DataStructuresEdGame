using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public int debugLinkControlVersion;
    public long debugFrameCount;

    public WorldGenerationBehavior worldGenerator;
    public InstructionScreensBehavior instructionScreenBehavior;
    // a referernce to all objective entities in the level
    private List<ObjectiveBlockBehavior> objectiveBlocks;
    // a reference to all platform entities in the level.
    private List<PlatformBehavior> platformEntities;

    // a queue of platforms that may be added in this level
    public List<PlatformBehavior> platformsToAdd;
    // whether the player is currently in an the Add Platform mode.
    public bool addingPlatforms = false;

    public bool enableLinkChaining = false;

    // to help track for double clicking
    public DateTime lastTimeClickedMillis;

    // the Prefab for line renderer stuff.
    public Transform linePreFab;
    // References to the hover arrow parts showing a preview of the arrow.
    public Transform hoverArrowLine;
    public Transform hoverArrowHead;

    public Texture2D cursorPointingTexture;
    public Texture2D cursorDraggingTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public int doubleClickDelay;

    // different win conditions for the level.
    public enum WinCondition
    {
        None,
        SortListAscending,
        SortListDescending,
        SortListDuplicatesNotAllBlocks,
    }

    // The win condition of this level.
    public WinCondition winConditon;
    // References to important objects in the scene. 
    public Transform playerRef;
    public Transform helicopterRobotRef;
    public LinkBlockBehavior selectedLink; // what Link block the player is adding a connection to, if any
    
    // Linked list properties
    public LinkBlockBehavior startingLink; // what is this level's starting link block?
    public PlatformBehavior hoverPlatformRef; // the platform the mouse is hovering over for version 1.
    public LinkBlockBehavior hoverLinkRef; // the link block the mouse is hovering over. 
    public LinkBlockBehavior previousNotNullHoverLinkRef;
    //public LinkBlockBehavior mouseOverLinkRef; // the link block the mouse is hovering over. 
    public List<LinkBlockBehavior> mouseOverLinkRefs; // the link block the mouse is hovering over. 

    // references to important UI elements.
    public Text statusTextUI;
    public Image objectiveHudPanelUI;
    public Text objectiveTextUI;
    // public Text codeTextUI;
    public CodePanelBehavior codePanelBehavior;

    public LoggingManager currentPlayerLogs;
    //TEMPORARY CHANGE THIS!! after we make logging work
    public int currentPlayerID = 0000;

    void Start()
    {
        debugFrameCount = 0;


        selectedLink = null;
        // ensure the starting link has the proper property
        if (startingLink != null)
        {
            startingLink.isStartingLink = true;
        }
        platformEntities = new List<PlatformBehavior>();
        objectiveBlocks = new List<ObjectiveBlockBehavior>();
        // initial world generation
        // worldGenerator.ManualStartGenerator();

    }

    void Update()
    {
        debugFrameCount++;

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
                if ((platformsToAdd.Count > 0) && (platformsToAdd[0] != null))
                {
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

                    // cancel placing with escape or right click
                    if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
                    {
                        addingPlatforms = false;
                        deselectSelectedLink();
                        removeHoverArrow();
                        platToDisplayAndAdd.gameObject.SetActive(false);
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
                        // configure the properties for the new platform
                        toBeAdded.isInLevel = true; // mark this as being in the level
                        Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        Vector3 positionMcPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        positionMcPosition.z = 0;
                        toBeAdded.transform.position = positionMcPosition;
                        toBeAdded.gameObject.SetActive(true);
                        selectedLink.setConnectingPlatform(toBeAdded); // connect the selected link to the new platform
                        // end adding platform 
                        addingPlatforms = false;
                        // append code and log
                        codePanelBehavior.appendCodeText(selectedLink.getCodeVariableString() + " = new Node();");
                        String timestamp1 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        //Debug.Log("Platform is added from link " + selectedLink.logId + " at (" + positionMcPosition.x + ", " + positionMcPosition.y + ") at time :" + timestamp1);
                        string actMsg = "Platform is added from link " + selectedLink.logId + " at (" + positionMcPosition.x + ", " + positionMcPosition.y + ")";
                        currentPlayerLogs.send_To_Server(currentPlayerID, actMsg, "connection", timestamp1);


                        updateObjectiveHUDAndBlocks();
                        updatePlatformEntities();

                    }
                }
                //to remove that gray 'prediction' arrow now that you've added the platform
                if (hoverArrowHead != null)
                {
                    removeHoverArrow();
                }
                deselectSelectedLink();
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


        // testing to see if you can see if there are more than one link blocks over the mouse
        mouseOverLinkRefs.Clear();
        Vector3 mousePointInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        foreach (Transform t in worldGenerator.levelEntities)
        {  

            // look at all elements which have link blocks
            if (t.GetComponent<LinkBlockBehavior>() != null) { 
                if (t.GetComponent<BoxCollider2D>().OverlapPoint(mousePointInWorld) || t.GetComponent<PolygonCollider2D>().OverlapPoint(mousePointInWorld))
                {
                    mouseOverLinkRefs.Add(t.GetComponent<LinkBlockBehavior>());
                }
            } else if(t.GetComponent<HelicopterRobotBehavior>() != null)
            {
                if (t.GetComponent<HelicopterRobotBehavior>().childLink.GetComponent<BoxCollider2D>().OverlapPoint(mousePointInWorld) ||
                    t.GetComponent<HelicopterRobotBehavior>().childLink.GetComponent<PolygonCollider2D>().OverlapPoint(mousePointInWorld))
                {
                    mouseOverLinkRefs.Add(t.GetComponent<HelicopterRobotBehavior>().childLink.GetComponent<LinkBlockBehavior>());
                }
            } else if (t.GetComponent<PlatformBehavior>() != null)
            {
                if (t.GetComponent<PlatformBehavior>().childLink.GetComponent<BoxCollider2D>().OverlapPoint(mousePointInWorld) ||
                       t.GetComponent<PlatformBehavior>().childLink.GetComponent<PolygonCollider2D>().OverlapPoint(mousePointInWorld))
                {
                    mouseOverLinkRefs.Add(t.GetComponent<PlatformBehavior>().childLink.GetComponent<LinkBlockBehavior>());
                }
            }
        } // end populating mouseOverLinkRefs.


        if (mouseOverLinkRefs.Count > 0)
        { 
            LinkBlockBehavior priorityLink = null;
            if (mouseOverLinkRefs.Count == 1)
            {
                priorityLink = mouseOverLinkRefs[0];
            } else
            {
                // pick the one with the center closest to the mouse. 
                LinkBlockBehavior minLb = null;
                foreach (LinkBlockBehavior lb in mouseOverLinkRefs)
                {
                    if (lb.GetComponent<BoxCollider2D>().OverlapPoint(mousePointInWorld) ||
                        (lb.parentPlatform != null && lb.parentPlatform.GetComponent<BoxCollider2D>().OverlapPoint(mousePointInWorld)))
                    {
                        minLb = lb;
                    }
                } 
                priorityLink = minLb;
            }

            if (hoverLinkRef != priorityLink && (priorityLink.parentPlatform == null || 
                (priorityLink.parentPlatform != null && !priorityLink.parentPlatform.isPlatHidden())))
            {
                setHoverLink(ref priorityLink);
                string hoverTag = hoverLinkRef != null ? hoverLinkRef.getLogID() : ""; 
                if (priorityLink != null && previousNotNullHoverLinkRef != null && previousNotNullHoverLinkRef.parentPlatform != null)
                {
                    previousNotNullHoverLinkRef.parentPlatform.updatePlatformValuesAndSprite(); // it should be returned to its "natural" state.
                }
            } 

        }
        else if (hoverLinkRef != null) // only set null if needed
        {
            removeHoverLink();
            previousNotNullHoverLinkRef = null; // does this fall apart?
            if (!Input.GetMouseButton(0)) { 
                setCursorToDefault(); // if you are not dragging, then default the cursor
            }
        }

        // handle just clicking the mouse button
        if (Input.GetMouseButtonDown(0))
        {
            // if you don't have any link selected and you're hovering over a link
            if (selectedLink == null && hoverLinkRef != null)
            {
                setSelectedLink(hoverLinkRef);
                String timestamp2 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Debug.Log(selectedLink.logId + " was clicked at time: " + timestamp2);
                setCursorToDragging();

            }  // if you're selecting a link and also hovering over the select link and clicking
            else if (selectedLink != null && hoverLinkRef != null && selectedLink == hoverLinkRef)
            {
                Debug.Log("millisecond diff: " + (DateTime.Now.Millisecond - lastTimeClickedMillis.Millisecond));
                if ((DateTime.Now.Millisecond - lastTimeClickedMillis.Millisecond) < doubleClickDelay) { 
                    codePanelBehavior.appendCodeText(selectedLink.getCodeVariableString() + " = null;"); // still counds as setting it as null if it is already null;
                    if (hoverLinkRef.connectingPlatform != null) { 
                        hoverLinkRef.removeLinkConnection();
                        //setStatusText("Removed link");
                        String timestamp3 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                        Debug.Log("the link block " + selectedLink.logId + " double clicked had an existing link so now it's deleted at time: " + timestamp3);
                    }
                }
                setSelectedLink(null);
                updateObjectiveHUDAndBlocks(); // update any objective blocks
                updatePlatformEntities();
                setCursorToPointer();
            } // if you just clicked and you have a link selected and you're not hovering over anything.
            else if (selectedLink != null && hoverLinkRef == null)
            {
                String timestamp4 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Debug.Log("the link block " + selectedLink.logId + " was deselected at time: " + timestamp4);
                deselectSelectedLink();
                setCursorToDefault(); 
            }
        } // if you are not clicking and not holding down the mouse button

        if (!Input.GetMouseButton(0)) // if the mouse button is NOT being held down.
        {
            if (selectedLink != null && hoverLinkRef != null && hoverLinkRef != selectedLink)
            {
                // establish connection  
                if (hoverLinkRef.parentPlatform == null || selectedLink.parentPlatform != hoverLinkRef.parentPlatform)
                {
                    // this means there is a valid connection!
                    // before establishing the connection for the addingLink, remove any links current there.
                    codePanelBehavior.appendCodeText(selectedLink.getCodeVariableString() + " = " + hoverLinkRef.getCodeVariableString() + ";");
                    if (selectedLink.isConnectedToPlatform())
                    {
                        selectedLink.removeLinkConnection();
                    }
                    if (hoverLinkRef.connectingPlatform != null && hoverLinkRef.connectingPlatform != selectedLink.parentPlatform)
                    {
                        selectedLink.setConnectingPlatform(hoverLinkRef.connectingPlatform);
                    } 
                    removeHoverArrow();
                    removeHoverLink();
                    setCursorToDefault();
                    //setStatusText("Established a connection.");
                    String timestamp5 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"); 
                    Debug.Log("Connection made: " + selectedLink.logId + " was clicked and dragged to " + (hoverLinkRef != null ? hoverLinkRef.logId : "null") + " at time: " + timestamp5);
                }
                previousNotNullHoverLinkRef = null; // no longer needed to track
                deselectSelectedLink();
            } else if (selectedLink != null && hoverLinkRef != selectedLink)
            {
                previousNotNullHoverLinkRef = null; // no longer needed to track
                deselectSelectedLink();
            } else if (selectedLink == null && hoverLinkRef != null)
            {
                // you can click on the hover link, so change the cursor to show that.
                setCursorToPointer();
            } else if (selectedLink != null && hoverLinkRef == selectedLink)
            {
                setCursorToPointer(); // click to delete
            }
        } 
         
        if (Input.GetMouseButton(0))
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
                Debug.Log(debugFrameCount + " | WIN CONDITION: Number of platforms in the level: " + numberOfTotalPlatformsInLevel);

                if (startingLink.connectingPlatform == null)
                {
                    return (numberOfTotalPlatformsInLevel == 0); // if there are no platforms in the level
                }
                else if (startingLink.connectingPlatform.childLink.GetComponent<LinkBlockBehavior>().connectingPlatform == null)
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
                    }
                    else
                    {
                        if ((winConditon == WinCondition.SortListAscending && next.getValue() < temp.getValue()) ||
                            (winConditon == WinCondition.SortListDescending && next.getValue() > temp.getValue()))
                        {
                            return false; // not sorted.
                        } // otherwise just keep on iterating.
                    }
                    temp = next;
                    sizeOfList++;
                }
                Debug.Log(debugFrameCount + " | WIN CONDITION: Size of the list: " + sizeOfList);
                return (sizeOfList == numberOfTotalPlatformsInLevel); // the list is sorted if all platforms in the level are in the list.

            case WinCondition.SortListDuplicatesNotAllBlocks:
                int listSize = 0; // number of elements connected to head list.
                Dictionary<int, int> unique = new Dictionary<int, int>();
                //this dictionary of all the levels at the start isn't helpful
                foreach (PlatformBehavior pb in platformEntities)
                {
                    if(!unique.ContainsKey(pb.getValue()))
                    {
                        unique.Add(pb.getValue(), 1);
                    }
                    
                    if (pb.isPlatformConnectingToStart())
                    {
                        listSize++;
                    }
                }
                
                if (startingLink.connectingPlatform == null)
                {
                    return (listSize == 0); // if there are no platforms in the level
                }
                else if (startingLink.connectingPlatform.childLink.GetComponent<LinkBlockBehavior>().connectingPlatform == null)
                {
                    return (listSize == 1); // if there is only one platform in the level
                }
                //start traversing the list and comparing the current and next platform's values
                PlatformBehavior tempDupl = startingLink.connectingPlatform.GetComponent<PlatformBehavior>();
                //check student's list to make a dictionary...
                Dictionary<int, int> PlayersList = new Dictionary<int, int>();
                while (tempDupl != null)
                {
                    if (PlayersList.ContainsKey(tempDupl.getValue()))
                    {
                        return false; 
                    }
                    else if (!PlayersList.ContainsKey(tempDupl.getValue()) && PlayersList.Count < listSize)
                    {
                        PlayersList.Add(tempDupl.getValue(), 1); 
                        if ((PlayersList.Count == unique.Count) && PlayersList.Count == listSize)
                        {
                            return true;
                        }
                    }
                    PlatformBehavior next = tempDupl.childLink.GetComponent<LinkBlockBehavior>().connectingPlatform;
                    if(next != null)
                    {
                        if(next.getValue() < tempDupl.getValue())
                        {
                            return false;
                        }
                    }
                    tempDupl = next;
                }
                return false;
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
            selectedLink.setDisplayMarker(false, true);
        }
        selectedLink = aLink; // do other data processing?

        if (selectedLink != null)
        { 
            selectedLink.setDisplayMarker(true, true);
        }
    }

    public void deselectSelectedLink()
    { 
        setCursorToDefault();
        setSelectedLink(null); // deselect adding link to deselect
        //setStatusText("Deselected link block"); 
        updateObjectiveHUDAndBlocks(); // update any objective blocks
        updatePlatformEntities();
    }

    // remove the current hover link and set the "bridge" collider to default again. 
    public void removeHoverLink()
    { 
        removeHoverArrow();
        if (hoverLinkRef != null) {  // only remove it if we actually need to remove it.
            if (hoverLinkRef != selectedLink)
            { // don't remove the marker on the selected link, but remove it otherwise.
                hoverLinkRef.setDisplayMarker(false);
            }
            // remove the "bridge" from the link we are no longer setting as the hover link.
            // so that its collision is now normal. 
            if (hoverLinkRef.connectingPlatform != null)
            {
                if (enableLinkChaining)
                {
                    hoverLinkRef.GetComponent<PolygonCollider2D>().points = new Vector2[0];  // remove the "bridge" collider
                    hoverLinkRef.GetComponent<LineRenderer>().SetPositions(new Vector3[0]);
                    hoverLinkRef.GetComponent<LineRenderer>().positionCount = 0;
                }
            }
            // update the old hover link...
            previousNotNullHoverLinkRef = hoverLinkRef; 
            hoverLinkRef = null; // no more hover link.

            // update the sprite of the connecting platform that we used to be revealing by the bridge
            /// but only if you are no longer mousing over any other links
            if (mouseOverLinkRefs.Count == 0) {
                if (previousNotNullHoverLinkRef.parentPlatform != null)
                {
                    previousNotNullHoverLinkRef.parentPlatform.updatePlatformValuesAndSprite(); 
                }
                if (previousNotNullHoverLinkRef.connectingPlatform != null) { 
                    previousNotNullHoverLinkRef.connectingPlatform.updatePlatformValuesAndSprite();
                }
            }
        }
    }

    // this value being passed in CAN'T be null.
    public void setHoverLink(ref LinkBlockBehavior lb)
    {
        removeHoverLink(); 
        hoverLinkRef = lb; 

        // update the new hover link if there is one.
        if (hoverLinkRef != null && hoverLinkRef != selectedLink) // can't set the hover link to the selected link
        {
            // make that hover link block be displayed as selected. 
            hoverLinkRef.setDisplayMarker(true);

            // conditions for "bridging": there is a select link and a hover link, and they are not equal
            if (selectedLink != null && hoverLinkRef != null && selectedLink != hoverLinkRef && hoverLinkRef.connectingPlatform != null)
            {
                hoverLinkRef.setDisplayMarker(true);
                // draw the faded arrow
                Color c = Color.gray;
                c.a = 0.3f;
                Transform[] hoverArrowParts = createArrowInstanceBetweenLinkPlatform(selectedLink, hoverLinkRef.connectingPlatform, c);
                hoverArrowLine = hoverArrowParts[0];
                hoverArrowHead = hoverArrowParts[1];
                //setStatusText("Release to set the first link equal to this one.");

                // This is when the mouse is hovering over a link for establishing a link.
                // update and create a "bridge" from this link to the next for next->next->.. option.
                if (hoverLinkRef.connectingPlatform != null)
                {
                    if (enableLinkChaining) { 
                        // Create a "bridge" from this link to the next link
                        Bounds otherBounds = hoverLinkRef.connectingPlatform.childLink.GetComponent<SpriteRenderer>().bounds;
                        Bounds hoverBounds = hoverLinkRef.GetComponent<SpriteRenderer>().bounds;
                        Vector3 worldDiffNorm = (otherBounds.center - hoverBounds.center).normalized;

                        // extend the bridge a little farther than needed to make it more user friendly.
                        Vector3 p0 = hoverLinkRef.transform.worldToLocalMatrix.MultiplyPoint(otherBounds.center + (worldDiffNorm * 0.1f));
                        Vector3 p1 = hoverLinkRef.transform.worldToLocalMatrix.MultiplyPoint(hoverBounds.center);
                        Vector3 diff = p0 - p1;
                        float scalePerpScale = 0.8f;
                        // http://mathworld.wolfram.com/PerpendicularVector.html
                        Vector3 perpDiff = (new Vector2(-diff.y, diff.x)).normalized;
                        Vector2[] bridgePoints = new Vector2[4];
                        bridgePoints[0] = p0 + (scalePerpScale * perpDiff);
                        bridgePoints[1] = p0 - (scalePerpScale * perpDiff);
                        bridgePoints[2] = p1 - (scalePerpScale * perpDiff);
                        bridgePoints[3] = p1 + (scalePerpScale * perpDiff);

                        Vector3 md = new Vector3(0, 0, 40);

                        Vector3[] linePositions = new Vector3[2];
                        linePositions[0] = p0 + md;
                        linePositions[1] = p1 + md;

                        // TODO: Transform the collider to go from world space to local space;
                        hoverLinkRef.GetComponent<PolygonCollider2D>().points = bridgePoints;
                        hoverLinkRef.GetComponent<LineRenderer>().startColor = Color.cyan;
                        hoverLinkRef.GetComponent<LineRenderer>().endColor = Color.cyan;
                        hoverLinkRef.GetComponent<LineRenderer>().positionCount = linePositions.Length;
                        hoverLinkRef.GetComponent<LineRenderer>().SetPositions(linePositions);
                        hoverLinkRef.GetComponent<LineRenderer>().startWidth = scalePerpScale - 0.2f; // make the path look a little smaller than it really is
                        hoverLinkRef.GetComponent<LineRenderer>().endWidth = scalePerpScale - 0.2f; 
                    
                        hoverLinkRef.connectingPlatform.updatePlatformValuesAndSprite();
                    }
                } // end creating the "bridge"
            }
        }

    } // end set hover link

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
        if (selectedLink != null) // you can only connect to a platform when there is a Link you have selected.
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
        Bounds linkBounds = lb.GetComponent<SpriteRenderer>().bounds; 
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
        linePos[1] = pTo + zOffset - ((pTo - pFrom).normalized * 0.1f);

        float headLength = 0.25f;
        Vector3 diffNorm = (pTo - pFrom).normalized;
        Vector3[] linePosHead = new Vector3[2];
        linePosHead[0] = pTo - (diffNorm * headLength) + zOffset;
        linePosHead[1] = pTo + zOffset;

        lineRenderer.SetPositions(linePos);
        lineRendererHead.SetPositions(linePosHead);
        return arrowParts;
    }

    private void setCursorToDefault()
    {
        Debug.Log("SET CURSOR TO DEFAULT");
        Cursor.SetCursor(null, new Vector2(), cursorMode);
    }

    private void setCursorToPointer()
    {
        Debug.Log("SET CURSOR TO POINTER");
        Cursor.SetCursor(cursorPointingTexture, new Vector2(12, 0), cursorMode);
    }

    private void setCursorToDragging()
    {
        Debug.Log("SET CURSOR TO DRAGGING");
        Cursor.SetCursor(cursorDraggingTexture, new Vector2(18, 8), cursorMode);
    }

    public void showInstructionScreen(string key)
    {
        instructionScreenBehavior.showScreen(key);
    }

    public void hideInstructionScreen(string key)
    {
        instructionScreenBehavior.hideScreen(key);
    }
}


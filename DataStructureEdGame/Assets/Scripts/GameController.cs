using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Where all global references and important game logic is.
 */ 
public class GameController : MonoBehaviour {

    public enum WinCondition
    {
        None,
        SortListAscending,
        SortListDescending,
        SortListDuplicatesNotAllBlocks,
    }

    public LoggingManager loggingManager; // set in unity editor
    public WorldGenerationBehavior worldGenerator; // set in unity editor
    public HUDBehavior hudBehavior; // set in unity editor

    [Header("Debugging")]
    public long debugFrameCount; // so log messages can be unique by appending what frame the game is on.

    [Header("Gameplay options")]
    public bool enableLinkChaining = false;

    [Header("Level specific Properties")]
    public WinCondition winCondition;

    [Header("Canvas References")]
    public Canvas winGameCanvas;
    public Canvas menuCanvas;
    private Canvas gameCanvas;

    [Header("UI Element Scripts")]
    public CodePanelBehavior codePanelBehavior;
    public InstructionScreensBehavior instructionScreenBehavior;

    [Header("PreFabs and Game Objects")] 
    public Transform linePreFab;
    public Transform deleteXPreFab;
    public Transform cameraRef;

    [Header("Cursor")]
    public Texture2D cursorPointingTexture;
    public Texture2D cursorDraggingTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    [Tooltip("How many milliseconds the user has to click twice to double click.")]
    public int doubleClickDelay;

    [Header("Internal references")]
    public Transform playerRef;
    public Transform helicopterRobotRef;
    private Transform deleteSpriteRef;

    [Header("Internal link references")]
    public LinkBehavior startingLink;
    public LinkBehavior selectedLink;
    public LinkBehavior hoverLink;
    private List<LinkBehavior> mouseOverLinkRefs; // the link block the mouse is hovering over. 
    
    public DateTime lastTimeClickedMillis;// a queue of platforms that may be added in this level
    public List<PlatformBehavior> platformsToAdd; 
    public bool addingPlatforms = false; // whether the player is currently in an the Add Platform mode.

    private List<ObjectiveBlockBehavior> objectiveBlocks; // a referernce to all objective entities in the level
    private List<PlatformBehavior> platformEntities; // a reference to all platform entities in the level. TODO: generalize this?


    void Start()
    {
        debugFrameCount = 0;
        
        selectedLink = null;
        hoverLink = null;

        mouseOverLinkRefs = new List<LinkBehavior>();
        loggingManager.setGameController(this);
        worldGenerator.setGameController(this);
        hudBehavior.setGameController(this);
        hudBehavior.setLoggingManager(loggingManager);
        gameCanvas = hudBehavior.GetComponent<Canvas>();

        // set the game to its initial state
        gameCanvas.gameObject.SetActive(false);
        winGameCanvas.gameObject.SetActive(false);
        menuCanvas.gameObject.SetActive(true);
    }

    void Update()
    {
        debugFrameCount++;

        // don't process the game if the world is still being instantiated.
        if (worldGenerator.isBusy())
        {
            return;
        }

        if (playerRef != null)
        {
            // always set the camera on top of the player.
            cameraRef.transform.position = new Vector3(playerRef.position.x, playerRef.position.y, transform.position.z - 50);
        }

        // if you are adding a platform...
        // cancels when you click on nothing or when you right click.

        Vector3 mousePointInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePointInWorld.z = 0;
        // add platform system 
        if (addingPlatforms && platformsToAdd.Count > 0)
        {
            PlatformBehavior platformToPreviewForAdd = platformsToAdd[0];  // show a preview of the platform.
            platformToPreviewForAdd.GetComponent<ContainerEntityBehavior>().refreshChildList();
            platformToPreviewForAdd.renderAsFadedPreview();
            platformToPreviewForAdd.setValueBlockText("" + platformToPreviewForAdd.getValue());
            platformToPreviewForAdd.transform.position = mousePointInWorld;
            platformToPreviewForAdd.gameObject.SetActive(true);
            platformToPreviewForAdd.GetComponent<BoxCollider2D>().isTrigger = true;

            if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) && hoverLink == null) // deselecting with right click?
            {
                addingPlatforms = false;
                platformToPreviewForAdd.gameObject.SetActive(false);
                if (selectedLink != null)  // deselect on canceling placement.
                {
                    selectedLink.setPreviewConnection(null);
                    selectedLink.setState(LinkBehavior.State.NORMAL);
                    selectedLink = null; 
                }
            }
            else if (selectedLink != null && Input.GetMouseButton(0)) // dragging from a link.
            {
                selectedLink.setPreviewConnection(platformToPreviewForAdd.GetComponent<ConnectableEntityBehavior>());
                selectedLink.UpdateRendering();
            }
            else if (selectedLink != null && Input.GetMouseButtonUp(0)) // release to finish adding platform,
            {
                platformsToAdd.Remove(platformToPreviewForAdd);
                platformToPreviewForAdd.transform.position = mousePointInWorld;
                platformToPreviewForAdd.gameObject.SetActive(true);
                platformToPreviewForAdd.GetComponent<BoxCollider2D>().isTrigger = false;
                platformToPreviewForAdd.isInLevel = true; // set as being added to the level.
                platformToPreviewForAdd.GetComponent<ContainerEntityBehavior>().refreshChildList();
                selectedLink.setConnectionTo(platformToPreviewForAdd.GetComponent<ConnectableEntityBehavior>()); // connect the selected link to the new platform
                platformToPreviewForAdd.updateRenderAndState(); // force rendering update. 
                addingPlatforms = false;
                codePanelBehavior.appendCodeText(selectedLink.getVariableName() + " = new Node();"); // append code to the generated code window.
                
                string actMsg = "Platform is added from link " + selectedLink.GetComponent<LoggableBehavior>().getLogID() + " at (" + mousePointInWorld.x + ", " + mousePointInWorld.y + ")";
                loggingManager.sendLogToServer(actMsg);

                selectedLink.setPreviewConnection(null);
                selectedLink.setState(LinkBehavior.State.NORMAL);
                selectedLink = null; // deselect.

                hudBehavior.setPlatformsToAddText(platformsToAdd.Count); // update UI
                updateObjectiveHUDAndBlocks();
                updatePlatformEntities();
            }
        } // end addingPlatforms block


        if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(0))   // the state of clicking has not changed this frame.
        {
            hoverLink = null; // to make sure it is properly updated this next frame. 
        }
        // if it is a hover link and the mouse is released, then check to establish a connection.
        mousePointInWorld.z = 0;
        for (int i = 0; i < worldGenerator.levelLinks.Count; i++)
        {
            LinkBehavior lb = worldGenerator.levelLinks[i];
            if (lb.selectable)
            {
                if (lb.isPointInside(mousePointInWorld))
                {
                    if (lb.state == LinkBehavior.State.NORMAL)
                    {
                        hoverLink = lb;
                        lb.setState(LinkBehavior.State.HOVER);
                        if (selectedLink != null && hoverLink.connectableEntity != null)
                        {
                            selectedLink.setPreviewConnection(hoverLink.connectableEntity); // preview the connection if there is a select link.
                            selectedLink.UpdateRendering();
                        }
                    }

                    if (Input.GetMouseButtonDown(0)) // if the link is just being clicked.
                    {
                        if (lb.state == LinkBehavior.State.SELECTED)
                        {
                            // if the link being clicked is selected
                            if (getMsSinceLastClick() < doubleClickDelay)
                            {
                                lb.setConnectionTo(null); // remove connection on double click
                                lb.setPreviewConnection(null);
                                lb.UpdateRendering();
                                codePanelBehavior.appendCodeText(lb.getVariableName() + " = null;");
                                updatePlatformEntities(); // changing links - update platforms
                                updateObjectiveHUDAndBlocks();
                            }
                        }
                        else // clicking on a link that is NOT selected
                        {
                            selectedLink = lb;
                            lb.setState(LinkBehavior.State.SELECTED);
                            if (hoverLink == selectedLink)
                            {
                                hoverLink.setPreviewConnection(null);
                                hoverLink.UpdateRendering();
                                hoverLink = null; // hover link can't be the same as selected link.
                            }
                        }
                    }
                    else if (Input.GetMouseButtonUp(0)) // if the mouse is being released over this link
                    {
                        if (lb.state == LinkBehavior.State.HOVER && selectedLink != null && selectedLink != lb) // if there IS a selected link. establish link.
                        {
                            selectedLink.setConnectionEqualTo(ref lb);
                            codePanelBehavior.appendCodeText(selectedLink.getVariableName() + " = " + lb.getVariableName() + ";");
                            selectedLink.setPreviewConnection(null);
                            selectedLink.setState(LinkBehavior.State.NORMAL);
                            lb.UpdateRendering();
                            updatePlatformEntities(); // changing links - update platforms
                            updateObjectiveHUDAndBlocks();
                        }
                    }
                }
                else
                { // mouse is NOT over link block
                    if (lb.state == LinkBehavior.State.HOVER)
                    {
                        lb.setState(LinkBehavior.State.NORMAL); // no longer a hover block.
                    }
                    else if (lb.state == LinkBehavior.State.SELECTED && !Input.GetMouseButton(0))
                    {
                        lb.setState(LinkBehavior.State.NORMAL); // no longer the selected block
                        lb.setPreviewConnection(null); // no preview
                        lb.UpdateRendering();
                    }
                }
            } else
            {
                if (lb.state != LinkBehavior.State.NORMAL)
                {
                    lb.setState(LinkBehavior.State.NORMAL);
                }
            }
        } // end iterating through links.

        // validate the selected link
        if (selectedLink != null && selectedLink.state != LinkBehavior.State.SELECTED)
        {
            selectedLink = null;
        }
        // validate the hover link
        if (hoverLink != null && hoverLink.state != LinkBehavior.State.HOVER)
        {
            hoverLink = null;
        }
        

        // record the last time that the mouse clicked for double clicking
        if (Input.GetMouseButton(0))
        {
            lastTimeClickedMillis = System.DateTime.Now;
        }
    }

    /**
     * Returns whether or not this level's win condition has been
     * satisfied based on the current world state.
     */ 
    bool isWinConditonSatisfied()
    {
        if (winCondition == WinCondition.None)
        {
            return true; // always beat it when there is no condition.
        }

        switch (winCondition)
        {
            case WinCondition.SortListDuplicatesNotAllBlocks:
                // after ensuring the list is sorted, see if there are duplicates for this case.
                // assuming ascending order sort.
                List<int> unique = new List<int>();
                var listSize = getSizeOfList();
                //this dictionary of all the levels at the start isn't helpful
                foreach (PlatformBehavior pb in platformEntities)
                {
                    if (!unique.Contains(pb.getValue()))
                    {
                        unique.Add(pb.getValue());
                    }
                }
                if (unique.Count != listSize)
                {
                    return false; // there were duplicates.
                }
                return true;
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
                if (startingLink.connectableEntity == null)
                {
                    return (numberOfTotalPlatformsInLevel == 0); // if there are no platforms in the level
                }
                startingLink.connectableEntity.GetComponent<ContainerEntityBehavior>().refreshChildList();
                if (startingLink.connectableEntity.GetComponent<ContainerEntityBehavior>() != null &&
                        startingLink.connectableEntity.GetComponent<ContainerEntityBehavior>().GetChildComponent<LinkBehavior>() != null &&
                        startingLink.connectableEntity.GetComponent<ContainerEntityBehavior>().GetChildComponent<LinkBehavior>().connectableEntity == null)
                {
                    return (numberOfTotalPlatformsInLevel == 1); // if there is only one platform in the level
                }
                sizeOfList = 1;
                ContainerEntityBehavior temp = startingLink.connectableEntity.GetComponent<ContainerEntityBehavior>();
                temp.refreshChildList();
                while (temp != null)
                {
                    if (temp.GetChildComponent<LinkBehavior>().connectableEntity == null)
                    {
                        break;
                    }
                    ContainerEntityBehavior next = temp.GetChildComponent<LinkBehavior>().connectableEntity.GetComponent<ContainerEntityBehavior>();
                    next.refreshChildList();
                    
                    if (((winCondition == WinCondition.SortListAscending || winCondition == WinCondition.SortListDuplicatesNotAllBlocks) && 
                            next.GetChildComponent<ValueBehavior>().getValue() < temp.GetChildComponent<ValueBehavior>().getValue()) ||
                        (winCondition == WinCondition.SortListDescending && 
                            next.GetChildComponent<ValueBehavior>().getValue() > temp.GetChildComponent<ValueBehavior>().getValue()))
                    {
                        return false; // not sorted.
                    } // otherwise just keep on iterating.
                    temp = next;
                    sizeOfList++;
                }
                return (sizeOfList == numberOfTotalPlatformsInLevel); // the list is sorted if all platforms in the level are in the list.
        }
        return false;
    }

    

    /**
     * This will update the objective block states based on the win condition.
     * It will also update the HUD for saying what the objective is and its state.
     */ 
    public void updateObjectiveHUDAndBlocks()
    {
        if (winCondition != WinCondition.None)
        {
            bool isWinSatisfied = isWinConditonSatisfied(); 
            if (winCondition == WinCondition.SortListAscending)
            {
                if (isWinSatisfied) 
                    hudBehavior.setObjectiveHUD("Sort the list in increasing order while including all Platforms.\nThe List is sorted!", true, isWinSatisfied);
                else
                    hudBehavior.setObjectiveHUD("Sort the list in increasing order while including all Platforms.\nThe List is not sorted.", true, isWinSatisfied);
            } else if (winCondition == WinCondition.SortListDescending)
            {
                if (isWinSatisfied)
                    hudBehavior.setObjectiveHUD("Sort the list in decreasing order while including all Platforms.\nThe List is sorted!", true, isWinSatisfied);
                else
                    hudBehavior.setObjectiveHUD("Sort the list in decreasing order while including all Platforms.\nThe List is not sorted.", true, isWinSatisfied);
            }
            else if (winCondition == WinCondition.SortListDuplicatesNotAllBlocks)
            {
                if (isWinSatisfied)
                    hudBehavior.setObjectiveHUD("Delete all duplicate blocks\nThe List has no duplicates!", true, isWinSatisfied);
                else
                    hudBehavior.setObjectiveHUD("Delete all duplicate blocks\nThe List contains duplicates.", true, isWinSatisfied);
            }

            // update the blocks
            for (int i = 0; i < objectiveBlocks.Count; i++)
            {
                objectiveBlocks[i].updatedFadedState(isWinSatisfied);
            }
        } else
        {
            hudBehavior.setObjectiveHUD("", false, false); 
        }
    }

    /**
     * Update the rendering and state information of all platforms.
     */ 
    public void updatePlatformEntities()
    {
        foreach (PlatformBehavior pb in platformEntities)
        {
            if (pb.isInLevel)
                pb.updateRenderAndState();
        }
    }


    /**
     * Set the cursor to its default icon.
     */ 
    private void setCursorToDefault()
    {
        Cursor.SetCursor(null, new Vector2(), cursorMode);
    }

    /**
     * Set the cursor to a pointer icon.
     */
    private void setCursorToPointer()
    {
        Cursor.SetCursor(cursorPointingTexture, new Vector2(12, 0), cursorMode);
    }

    /**
     * Set the cursor to a dragging icon.
     */ 
    private void setCursorToDragging()
    {
        Cursor.SetCursor(cursorDraggingTexture, new Vector2(18, 8), cursorMode);
    }

    /**
     * Show the given instruction screen.
     */
    public void showInstructionScreen(string key)
    {
        instructionScreenBehavior.showScreen(key);
    }

    /**
     * Hide the given instruction screen.
     */
    public void hideInstructionScreen(string key)
    {
        instructionScreenBehavior.hideScreen(key);
    }


    /**
     * Internal function
     */
    public void setLevelPlatformEntitiesList(List<PlatformBehavior> plist)
    {
        if (platformEntities == null)
        {
            platformEntities = new List<PlatformBehavior>();
        }
        platformEntities.AddRange(plist);
    }

    /**
     * Internal function
     */
    public void setLevelObjectiveBlocksList(List<ObjectiveBlockBehavior> oblist)
    {
        if (objectiveBlocks == null)
        {
            objectiveBlocks = new List<ObjectiveBlockBehavior>();
        }
        objectiveBlocks.AddRange(oblist);
    }

    /**
     * Internal function
     */
    public void clearReferenceLists()
    {
        objectiveBlocks.Clear();
        platformEntities.Clear();
    }

    /**
     * Get the size of the list entity in the current level.
     */ 
    public int getSizeOfList()
    {
        List<ContainerEntityBehavior> visited = new List<ContainerEntityBehavior>();
        int sizeOfList = 1;
        ContainerEntityBehavior temp = startingLink.connectableEntity.GetComponent<ContainerEntityBehavior>();
        temp.refreshChildList();
        while (temp != null)
        {
            if (temp.GetChildComponent<LinkBehavior>().connectableEntity == null)
            {
                break;
            }
            ContainerEntityBehavior next = temp.GetChildComponent<LinkBehavior>().connectableEntity.GetComponent<ContainerEntityBehavior>();
            next.refreshChildList();
            if (visited.Contains(next))
            {
                return int.MaxValue; // inifite loop
            }
            visited.Add(temp);
            temp = next;
            sizeOfList++; 
        }
        return sizeOfList;
    }

    public int getMsSinceLastClick()
    {
        return lastTimeClickedMillis.Millisecond - System.DateTime.Now.Millisecond;
    }
}


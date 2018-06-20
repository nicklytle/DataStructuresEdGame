using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public WorldGenerationBehavior worldGenerator;
    public List<ObjectiveBlockBehavior> objectiveBlocks;
    public List<PlatformBehavior> platformEntities;
    public List<PlatformBehavior> platformsToAdd;
    public int debugLinkControlVersion; // 0 for Link->Platform, 1 for Link=Link version.
    public bool addingPlatforms = false;
    public DateTime lastTimeClickedMillis;

    public Texture2D pointerCursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;

    // different win conditions for the level.
    public enum WinCondition
    {
        None,
        SortListAscending,
        SortListDescending
    }

    // what win condition we need to make this block faded.
    public GameController.WinCondition winConditon;

    // References to important objects in the scene. 
    public Transform playerRef;
    public Transform helicopterRobotRef;
    public LinkBlockBehavior selectedLink; // what Link block the player is adding a connection to, if any
    
    // Linked list properties
    public LinkBlockBehavior startingLink; // what is this level's starting link block?
    public PlatformBehavior hoverPlatform; // the platform the mouse is hovering over for version 1.
    public LinkBlockBehavior hoverLink; // the link block the mouse is hovering over. 

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
        updatePlatformEntities();
        updateObjectiveHUDAndBlocks();
    }
    
    bool isWinConditonSatisfied()
    {
        switch (winConditon)
        {
            case GameController.WinCondition.SortListAscending:
            case GameController.WinCondition.SortListDescending:
                if (startingLink == null)
                    return false;

                if (startingLink.connectingPlatform == null)
                {
                    return true;
                }
                if (startingLink.connectingPlatform.childLink.GetComponent<LinkBlockBehavior>().connectingPlatform == null)
                {
                    return true;
                }
                PlatformBehavior temp = startingLink.connectingPlatform.GetComponent<PlatformBehavior>();
                while (temp != null)
                {
                    PlatformBehavior next = temp.childLink.GetComponent<LinkBlockBehavior>().connectingPlatform;
                    if (next == null)
                    {
                        return true;
                    }
                    else
                    {
                        if ((winConditon == GameController.WinCondition.SortListAscending && next.getValue() < temp.getValue()) ||
                            (winConditon == GameController.WinCondition.SortListDescending && next.getValue() > temp.getValue()))
                        {
                            return false;
                        } // otherwise just keep on iterating.
                    }
                    temp = next;
                }
                break;
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
            selectedLink.setDisplaySelected(false);
        }
        selectedLink = aLink; // do other data processing?

        if (selectedLink != null)
        { 
            selectedLink.setDisplaySelected(true);
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
    public void setConnectingPlatform(PlatformBehavior platform)
    {
        if (selectedLink != null) // you can only connect to a platform when there is a Link.
        {
            if (selectedLink.parentPlatform != null && selectedLink.parentPlatform == platform)
                return; // don't change anything if the platform is the parent of the adding link.
            
            if (hoverPlatform != null)
            {
                hoverPlatform.setDisplaySelected(false);
            }
            hoverPlatform = platform;
            if (selectedLink != null && hoverPlatform != null) // only display it for this platform if you're also selecting a Link.
            {
                hoverPlatform.setDisplaySelected(true);
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

    void Update()
    { 
        if (playerRef != null)
        {
            // always set the camera on top of the player.
            transform.position = new Vector3(playerRef.position.x, playerRef.position.y, transform.position.z);
        }

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

        if (debugLinkControlVersion == 0) {  // Link -> Platform controls
            if (selectedLink != null && !Input.GetMouseButton(0)) // mouse was released 
            {
                if (hoverPlatform != null)
                {
                    selectedLink.setConnectingPlatform(hoverPlatform);
                }
                // deselect when you release the mouse button.
                setConnectingPlatform(null);
                setSelectedLink(null); 
            }
        } 
        else if (debugLinkControlVersion == 1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // check for double clicking here
                bool doubleClick = selectedLink == hoverLink && lastTimeClickedMillis != null && (System.DateTime.Now - lastTimeClickedMillis).Milliseconds < 200;
                
                if (doubleClick)
                {
                    if (hoverLink != null && hoverLink.isConnectedToPlatform()) { 
                        Debug.Log("DOUBLE CLICKED ON A LINK");
                        // remove the link
                        setSelectedLink(null);
                        hoverLink.removeLinkConnection();
                        setStatusText("Removed link");
                        updateObjectiveHUDAndBlocks(); // update any objective blocks
                        updatePlatformEntities();
                    }
                }
                else
                {
                    if (hoverLink != null)
                    {
                        if (selectedLink == null) // you are not deleting it
                        {
                            setSelectedLink(hoverLink); // set that this is the link being dragged from the player. 
                            setStatusText("Click on another Link to set this one equal to it or press Shift to deselect.");
                        } else if (selectedLink != null && selectedLink != hoverLink) // don't connect to yourself
                        {
                            Debug.Log("Single click with hoverLink and selectedLink not null");
                            if (hoverLink.parentPlatform == null || selectedLink.parentPlatform != hoverLink.parentPlatform)
                            {
                                Debug.Log("Establishing connection");
                                // this means there is a valid connection!
                                // before establishing the connection for the addingLink, remove any links current there.
                                if (selectedLink.isConnectedToPlatform())
                                {
                                    selectedLink.removeLinkConnection();
                                }
                                selectedLink.setConnectingPlatform(hoverLink.connectingPlatform);
                            }
                            setSelectedLink(null);
                            updateObjectiveHUDAndBlocks(); // update any objective blocks
                            updatePlatformEntities();
                        }
                    } // end hoverLink != null block
                    else // you are clicking once but not on a link.
                    {
                        // deselect
                        setSelectedLink(null); // deselect adding link to deselect
                        setStatusText("Deselected link block");
                    }
                }
            } // end MouseButtonDown(0) check.
        }


        if (Input.GetMouseButtonDown(0))
        {
            lastTimeClickedMillis = System.DateTime.Now;
        }
    }

    public void clearReferenceLists()
    {
        objectiveBlocks.Clear();
        platformEntities.Clear();
    }
}

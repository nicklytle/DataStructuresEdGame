using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public WorldGenerationBehavior worldGenerator;
    public List<ObjectiveBlockBehavior> objectiveBlocks;
    public List<PlatformBehavior> platformEntities;
    public int debugLinkControlVersion; // 0 for Link->Platform, 1 for Link=Link version.

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
    public LinkBlockBehavior addingLink; // what Link block the player is adding a connection to, if any
    
    // Linked list properties
    public LinkBlockBehavior startingLink; // what is this level's starting link block?
    public PlatformBehavior connectingPlatform;

    public Text statusTextUI;
    public Image objectiveHudPanelUI;
    public Text objectiveTextUI;

    void Start()
    {
        addingLink = null; 
        setStatusText("");
        // ensure the starting link has the proper property
        if (startingLink != null)
        {
            startingLink.isStartingLink = true;
        }
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
    public void setAddingLink(LinkBlockBehavior aLink)
    {
        if (addingLink != null)
        {
            addingLink.setDisplaySelected(false);
        }
        addingLink = aLink; // do other data processing?

        if (addingLink != null)
        { 
            addingLink.setDisplaySelected(true);
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
        if (addingLink != null) // you can only connect to a platform when there is a Link.
        {
            if (addingLink.parentPlatform != null && addingLink.parentPlatform == platform)
                return; // don't change anything if the platform is the parent of the adding link.
            
            if (connectingPlatform != null)
            {
                connectingPlatform.setDisplaySelected(false);
            }
            connectingPlatform = platform;
            if (addingLink != null && connectingPlatform != null) // only display it for this platform if you're also selecting a Link.
            {
                connectingPlatform.setDisplaySelected(true);
            }
        }
    }

    /**
     * This will update the objective block states based on the win condition.
     * It will also update the HUD for saying what the objective is and its state.
     */ 
    public void updateObjectiveBlocks()
    {
        Debug.Log("Checking win!");
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
    }

    void Update()
    {
        if (playerRef != null)
        {
            // always set the camera on top of the player.
            transform.position = new Vector3(playerRef.position.x, playerRef.position.y, transform.position.z);
        }

        if (debugLinkControlVersion == 0) {  // Link -> Platform controls

            if (addingLink != null && !Input.GetMouseButton(0)) // mouse was released 
            {
                if (connectingPlatform != null)
                {
                    addingLink.setConnectingPlatform(connectingPlatform);
                }
                // deselect when you release the mouse button.
                setConnectingPlatform(null);
                setAddingLink(null); 
            }
        } 
        else if (debugLinkControlVersion == 1)
        {
            if (addingLink != null && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)))
            {
                setAddingLink(null); // deselect adding link to deselect
                setStatusText("Deselected link block");
            } 
        }
    }

    public void clearReferenceLists()
    {
        objectiveBlocks.Clear();
        platformEntities.Clear();
    }
}

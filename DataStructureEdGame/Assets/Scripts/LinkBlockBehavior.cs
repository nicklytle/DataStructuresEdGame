using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkBlockBehavior : MonoBehaviour
{

    public GameController gameController;

    public PlatformBehavior parentPlatform; // if this link block is in a platform, then this is the parent 
    public PlatformBehavior connectingPlatform; // this is the platform object this link is pointing to.

    // references to the arrow instances
    public Transform linkArrow; // this is the current arrow that is instantiated 
    public Transform linkArrowHead; 

    // Prefabs (old version of arrows)
    //public Transform linkArrowPreFab;
    //public Transform linkArrowFadedPreFab;
    //public Transform linkArrowHelicopterPreFab;

    // sprites for the different states of the link block
    public Sprite nullLinkSprite; // what to show when it is null.
    public Sprite defaultSprite; // what to show when it is NOT null (default to what it starts as).

    // special flags for the type of link block.
    public bool isStartingLink;
    public bool isHelicopterLink;
    // whether to render this arrow or not.
    public bool renderArrow;

    void Start()
    {
        if (parentPlatform == null)
        {
            UpdateLinkArrow();
        }
        renderArrow = true; // default
    }

    /**
     * Update the Link Arrow to match the data of the platform. 
     */
    public void UpdateLinkArrow()
    {
        removeArrowBetween();

        if (connectingPlatform == null) // only update the sprite if there is no connection
        {
            if (GetComponent<SpriteRenderer>().sprite != nullLinkSprite)
            {
                GetComponent<SpriteRenderer>().sprite = nullLinkSprite;
            }
        } else {
            if (GetComponent<SpriteRenderer>().sprite != defaultSprite)
            {
                GetComponent<SpriteRenderer>().sprite = defaultSprite;
            }
        }

        if (renderArrow && connectingPlatform != null)
        { 
            // set the arrow color
            Color color = Color.red;
            if ((parentPlatform != null && parentPlatform.isPhasedOut) || (isHelicopterLink && connectingPlatform.isPhasedOut))
            {
                color = Color.gray;  // arrowPreFab = linkArrowFadedPreFab;
            }
            else if (isHelicopterLink)
            {
                color = Color.yellow; // arrowPreFab = linkArrowHelicopterPreFab;
            }
            // create the arrow and save references to the new instances.
            Transform[] linkArrowParts = gameController.createArrowInstanceBetweenLinkPlatform(this, connectingPlatform, color);
            linkArrow = linkArrowParts[0];
            linkArrowHead = linkArrowParts[1];
        } // end render arrow
    }

    /**
     * Remove the arrow coming from this link block, if there is one.
     */
    public void removeArrowBetween()
    { 
        if (linkArrow != null)
        {
            //Debug.Log("delete the link arrow");
            Destroy(linkArrow.gameObject);
            linkArrow = null;
            Destroy(linkArrowHead.gameObject);
            linkArrowHead = null;
        }
    }
    

    /**
     *  see if this Link has a connection to a Platform.
     */
    public bool isConnectedToPlatform()
    {
        return connectingPlatform != null;
    }

    /**
     *  Remove the connection from this link to the current platform. Precondtion: isConnectedToPlatform() is true.
     */
    public void removeLinkConnection()
    {
        connectingPlatform.removeIncomingConnectingLink(this); // set connect platform null to make render update correctly
        connectingPlatform = null;
        UpdateLinkArrow();
    }

    /**
     *  Set the playform this is going to be linking to.
     */
    public void setConnectingPlatform(PlatformBehavior platform)
    {
        //Debug.Log("Setting the connected platform link connection");
        if (platform != null)
        {
            connectingPlatform = platform;
            connectingPlatform.addIncomingConnectingLink(this);
            if (isHelicopterLink) // if the link belongs to the helicopter robot...
            {
                gameController.helicopterRobotRef.GetComponent<HelicopterRobotBehavior>().MoveAboveLinkedPlatform();
            }
            UpdateLinkArrow();
        }
    }

    public void setDisplaySelected(bool b)
    {
        transform.Find("SelectMarker").gameObject.SetActive(b);
    }


    void OnMouseEnter()
    {
        Cursor.SetCursor(gameController.pointerCursorTexture, Vector2.zero, gameController.cursorMode);
        gameController.setHoverLink(this);
        if (gameController.debugLinkControlVersion == 0)
        {
            if (gameController.selectedLink == null)
            {
                gameController.setStatusText("Click to set this to remove the link and set this as the adding link");
            }
        }
        else if (gameController.debugLinkControlVersion == 1)
        {
            if (gameController.selectedLink == null)
            {
                gameController.setStatusText("Click to set this as the adding link. Shift + Click to delte the connection");
            }
        }
    }

    void OnMouseExit()
    {
        gameController.setHoverLink(null);
        Cursor.SetCursor(null, Vector2.zero, gameController.cursorMode);
        if (gameController.selectedLink == null)
        {
            gameController.setStatusText("");
        }
    }

    // if the user clicks on this block.
    void OnMouseDown()
    {
        if (gameController.debugLinkControlVersion == 0)
        {
            if (isConnectedToPlatform())  // there is a link block there.
            {
                removeLinkConnection();
                gameController.updateObjectiveHUDAndBlocks(); // update any objective blocks
            } 
            gameController.setSelectedLink(this); // set that this is the link being dragged from the player. 
            gameController.updatePlatformEntities();
        }
        else if (gameController.debugLinkControlVersion == 1)
        {

            // moved to inside of GameController's Update() 
            /*if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && isConnectedToPlatform())  // there is a link block there.
            {
                // Debug.Log("DELETING LINK!");
                gameController.setAddingLink(null);
                removeLinkConnection();
                gameController.setStatusText("Removed link");
                gameController.updateObjectiveHUDAndBlocks(); // update any objective blocks
                gameController.updatePlatformEntities();
            }
            if (gameController.addingLink == null && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) // you are not deleting it
            {
                gameController.setAddingLink(this); // set that this is the link being dragged from the player. 
                gameController.setStatusText("Click on another Link to set this one equal to it or press Shift to deselect.");
            }
            // make sure it is OK to make addingLink's connectingPlatform equal to this connectingPlatform
            if (gameController.addingLink != null && gameController.addingLink != this) // don't connect to yourself
            {
                if (gameController.addingLink.connectingPlatform != connectingPlatform && connectingPlatform != gameController.addingLink.parentPlatform)
                {
                    // this means there is a valid connection!
                    // before establishing the connection for the addingLink, remove any links current there.
                    if (gameController.addingLink.isConnectedToPlatform())
                    {
                        gameController.addingLink.removeLinkConnection();
                    }
                    gameController.addingLink.setConnectingPlatform(connectingPlatform);
                }
                gameController.setAddingLink(null);
                gameController.updateObjectiveHUDAndBlocks(); // update any objective blocks
                gameController.updatePlatformEntities();
            }*/
        }
    }

    /**
     * Set whether to render this link's arrow yet or not. 
     * This will only update the link arrow if the value is different.
     */ 
    public void setRenderArrow(bool r)
    {
        if (renderArrow != r) { 
            renderArrow = r;
            UpdateLinkArrow();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

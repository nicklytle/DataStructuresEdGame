using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkBlockBehavior : MonoBehaviour {

    public GameController gameController;
    public PlatformBehavior parentPlatform; // if this link block is in a platform, then this is the parent 
    public PlatformBehavior connectingPlatform; // this is the platform object this link is pointing to.
    public Transform linkArrow; // this is the current arrow that is instantiated 
    public Transform linkArrowPreFab;
    public Sprite nullLinkSprite; // what to show when it is null.
    public Sprite defaultSprite; // what to show when it is NOT null (default to what it starts as).
    
    // special flags for the type of link block.
    public bool isStartingLink; 
    public bool isHelicopterLink;

    void Start () {
        if (parentPlatform == null) { 
            UpdateLinkArrow();
        }
    }
	
    /**
     * Update the Link Arrow to match the data of the platform. 
     */
    public void UpdateLinkArrow()
    {
        // always reset the linkArrow when updating
        if (linkArrow != null)
        {
            Debug.Log("delete the link arrow");
            Destroy(linkArrow.gameObject);
            linkArrow = null;
        }

        if (connectingPlatform == null) // only update the sprite if there is no connection
        {
            if (GetComponent<SpriteRenderer>().sprite != nullLinkSprite)
            {
                Debug.Log("Set to the null link");
                GetComponent<SpriteRenderer>().sprite = nullLinkSprite;
            }
        }
        else
        { 
            Bounds linkBounds = GetComponent<SpriteRenderer>().bounds; // the bounds for this link block.;
            if (parentPlatform != null) // If this link is a child link, then the parent platform's bounds is the link bounds for rendering
            {
                linkBounds = parentPlatform.GetComponent<SpriteRenderer>().bounds;
            }
            Bounds platBounds = connectingPlatform.GetComponent<SpriteRenderer>().bounds;

            // find the closest points on both bounding boxes to the center point to make the arrow.
            Vector3 betweenPoint = new Vector3((linkBounds.center.x + platBounds.center.x) / 2,
                (linkBounds.center.y + platBounds.center.y) / 2, 0);
            Vector3 closestToLink = linkBounds.ClosestPoint(betweenPoint);
            Vector3 closestToPlat = platBounds.ClosestPoint(betweenPoint);
            betweenPoint = (closestToLink + closestToPlat) / 2; // update the between point 

            linkArrow = Instantiate(linkArrowPreFab, betweenPoint, Quaternion.identity);
            linkArrow.transform.localScale = new Vector3(Vector3.Distance(closestToLink, closestToPlat), 1, 1);
            Vector3 diff = closestToPlat - closestToLink;
            float rotationAmount = 0; // the number of radians to rotate it.
            // rotationAmount = Mathf.Asin(diff.normalized.y);

            // TODO: this needs to actually work and not be so complicated

            if (diff.y != 0)
            {
                // TODO: Fix the rotation amount; sometimes it looks funky. 
                rotationAmount = Mathf.Sin(diff.y / diff.magnitude);
                if (diff.x < 0)
                {
                    linkArrow.transform.localScale = new Vector3(-linkArrow.transform.localScale.x,
                        linkArrow.transform.localScale.y, linkArrow.transform.localScale.z);
                    rotationAmount *= -1;
                }
            }
            else if (diff.x < 0)
            {
                linkArrow.transform.localScale = new Vector3(-linkArrow.transform.localScale.x,
                        linkArrow.transform.localScale.y, linkArrow.transform.localScale.z);
            }
            linkArrow.transform.Rotate(new Vector3(0, 0, Mathf.Rad2Deg * rotationAmount));
            // update the links sprite
            if (GetComponent<SpriteRenderer>().sprite != defaultSprite)
            {
                GetComponent<SpriteRenderer>().sprite = defaultSprite;
            }
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
        Debug.Log("Setting the connected platform link connection");
        connectingPlatform = platform;
        connectingPlatform.addIncomingConnectingLink(this);
        UpdateLinkArrow();
    }

    public void setDisplaySelected(bool b)
    {
        transform.Find("SelectMarker").gameObject.SetActive(b); 
    }


    void OnMouseEnter()
    {
        if (gameController.addingLink == null) { 
            gameController.setStatusText("Click to set this as the adding link. Shift + Click to delte the connection");
        }
    }

    void OnMouseExit()
    {
        if (gameController.addingLink == null)
        {
            gameController.setStatusText("");
        }
    }

    // if the user clicks on this block.
    void OnMouseDown()
    {
        if (gameController.debugLinkControlVersion == 0) { 
            if (isConnectedToPlatform())  // there is a link block there.
            {
                gameController.setAddingLink(null);
                removeLinkConnection(); 
            } else // the link block is empty
            { 
                gameController.setAddingLink(this); // set that this is the link being dragged from the player. 
            }
        }
        else if (gameController.debugLinkControlVersion == 1) 
        { 
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && isConnectedToPlatform())  // there is a link block there.
            {
                Debug.Log("DELETING LINK!");
                gameController.setAddingLink(null);
                removeLinkConnection();
                gameController.setStatusText("Removed link");
                gameController.updateObjectiveBlocks(); // update any objective blocks
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
                    if (gameController.addingLink.isConnectedToPlatform()) { 
                        gameController.addingLink.removeLinkConnection();
                    } 
                    gameController.addingLink.setConnectingPlatform(connectingPlatform);
                    /*if (gameController.addingLink.parentPlatform != null) // see if the updated link was inside of a platform.
                    {
                        gameController.addingLink.parentPlatform.updatePlatformValuesAndSprite(); // update that platform and all connected platforms
                    } else
                    {
                        connectingPlatform.updatePlatformValuesAndSprite(); // update connected platform only.
                    }*/
                } 
                gameController.setAddingLink(null);
                gameController.updateObjectiveBlocks(); // update any objective blocks
                gameController.updatePlatformEntities();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

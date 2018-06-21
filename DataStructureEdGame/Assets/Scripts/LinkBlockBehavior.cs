using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkBlockBehavior : MonoBehaviour
{

    public GameController gameController;
    public PlatformBehavior parentPlatform; // if this link block is in a platform, then this is the parent 
    public PlatformBehavior connectingPlatform; // this is the platform object this link is pointing to.
    public Transform linkArrow; // this is the current arrow that is instantiated 
    public Transform linkArrowHead;  
    public Transform linePreFab;
    public Transform linkArrowPreFab;
    public Transform linkArrowFadedPreFab;
    public Transform linkArrowHelicopterPreFab;
    public Sprite nullLinkSprite; // what to show when it is null.
    public Sprite defaultSprite; // what to show when it is NOT null (default to what it starts as).

    // special flags for the type of link block.
    public bool isStartingLink;
    public bool isHelicopterLink;
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

        if (linkArrow != null)
        {
            //Debug.Log("delete the link arrow");
            Destroy(linkArrow.gameObject);
            linkArrow = null;
            Destroy(linkArrowHead.gameObject);
            linkArrowHead = null;
        }

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
            closestToLink = new Vector3(closestToLink.x, closestToLink.y, 0);
            Vector3 closestToPlat = platBounds.ClosestPoint(betweenPoint);
            closestToPlat = new Vector3(closestToPlat.x, closestToPlat.y, 0);

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
            linkArrow = Instantiate(linePreFab, betweenPoint, Quaternion.identity);
            linkArrowHead = Instantiate(linePreFab, betweenPoint, Quaternion.identity);
            LineRenderer lineRenderer = linkArrow.GetComponent<LineRenderer>();
            LineRenderer lineRendererHead = linkArrowHead.GetComponent<LineRenderer>();

            lineRenderer.enabled = true;
            lineRenderer.widthMultiplier = 0.1f; 
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRendererHead.enabled = true;
            lineRendererHead.startColor = color;
            lineRendererHead.endColor = color;
            lineRendererHead.startWidth = 0.5f;
            lineRendererHead.endWidth = 0f;

            Vector3[] linePos = new Vector3[2];
            linePos[0] = closestToLink;
            linePos[1] = closestToPlat;

            float headLength = 0.25f;
            Vector3 diffNorm = (closestToPlat - closestToLink).normalized;
            Vector3[] linePosHead = new Vector3[2];
            linePosHead[0] = closestToPlat - (diffNorm * headLength);
            linePosHead[1] = closestToPlat;

            lineRenderer.SetPositions(linePos);
            lineRendererHead.SetPositions(linePosHead);
        } // end render arrow
    }
    
    /// OLD VERSION OF ARROW DRAWING
    /**
     * Update the Link Arrow to match the data of the platform. 
     */
    /*public void UpdateLinkArrow()
    {
        bool vertical = false;
        bool horiztonal = false;
        // always reset the linkArrow when updating
        if (linkArrow != null)
        {
            //Debug.Log("delete the link arrow");
            Destroy(linkArrow.gameObject);
            linkArrow = null;
        }

        if (connectingPlatform == null) // only update the sprite if there is no connection
        {
            if (GetComponent<SpriteRenderer>().sprite != nullLinkSprite)
            {
                //Debug.Log("Set to the null link");
                GetComponent<SpriteRenderer>().sprite = nullLinkSprite;
            }
        } else // if there is a connection, then draw the default sprite.
        { 
            if (GetComponent<SpriteRenderer>().sprite != defaultSprite)
            {
                GetComponent<SpriteRenderer>().sprite = defaultSprite;
            } 
        }

        if (renderArrow && connectingPlatform != null)
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
            //Debug.Log(closestToLink);
            Vector3 closestToPlat = platBounds.ClosestPoint(betweenPoint);
            //Debug.Log(closestToPlat);
            betweenPoint = (closestToLink + closestToPlat) / 2; // update the between point 

            Transform arrowPreFab = linkArrowPreFab;
            if ((parentPlatform != null && parentPlatform.isPhasedOut) || (isHelicopterLink && connectingPlatform.isPhasedOut))
            {
                arrowPreFab = linkArrowFadedPreFab;
            }
            else if (isHelicopterLink)
            {
                arrowPreFab = linkArrowHelicopterPreFab;
            }

            if (Mathf.Abs(linkBounds.center.x - platBounds.center.x) < 0.2f)
            {
                vertical = true;
                Debug.Log("vertical arrow needed");
                linkArrow = Instantiate(arrowPreFab, betweenPoint, Quaternion.identity);
                linkArrow.transform.localScale = new Vector3(Vector3.Distance(closestToLink, closestToPlat), 1, 1);
                Vector3 vertDiff = closestToPlat - closestToLink;
                if (vertDiff.y < 0)
                {
                    linkArrow.transform.localScale = new Vector3(linkArrow.transform.localScale.x,
                    linkArrow.transform.localScale.y, linkArrow.transform.localScale.z);
                    linkArrow.transform.Rotate(new Vector3(0, 0, 270));
                    Debug.Log("platform is lower");
                }
                if (vertDiff.y > 0)
                {
                    linkArrow.transform.localScale = new Vector3(linkArrow.transform.localScale.x,
                    linkArrow.transform.localScale.y, linkArrow.transform.localScale.z);
                    linkArrow.transform.Rotate(new Vector3(0, 0, 90));
                    Debug.Log("platform is higher");
                }
            }

            if (Mathf.Abs(linkBounds.center.y - platBounds.center.y) < 0.2f)
            {
                horiztonal = true;
                Debug.Log("horizontal arrow needed");
                linkArrow = Instantiate(arrowPreFab, betweenPoint, Quaternion.identity);
                linkArrow.transform.localScale = new Vector3(Vector3.Distance(closestToLink, closestToPlat), 1, 1);
                Vector3 vertDiff = closestToPlat - closestToLink;
                if (vertDiff.x < 0)
                {
                    linkArrow.transform.localScale = new Vector3(linkArrow.transform.localScale.x,
                    linkArrow.transform.localScale.y, linkArrow.transform.localScale.z);
                    linkArrow.transform.Rotate(new Vector3(0, 0, 180));
                    Debug.Log("platform is on left");
                }
                if (vertDiff.x > 0)
                {
                    linkArrow.transform.localScale = new Vector3(linkArrow.transform.localScale.x,
                    linkArrow.transform.localScale.y, linkArrow.transform.localScale.z);
                    linkArrow.transform.Rotate(new Vector3(0, 0, 0));
                    Debug.Log("platform is on right");
                }
            }
            if (!horiztonal && !vertical)
            {
                linkArrow = Instantiate(arrowPreFab, betweenPoint, Quaternion.identity);
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
            }
        } // end render arrow
    }*/


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
        gameController.hoverLink = this;
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
        gameController.hoverLink = null;
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

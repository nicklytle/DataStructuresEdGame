using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkBlockBehavior : MonoBehaviour
{
    public string logId; // the ID of this object when it is logged

    public GameController gameController;

    public PlatformBehavior containerPlatform; // if this link block is in a platform, then this is the parent 
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
    public Sprite defaultSelectMarkerSprite;  // what the sprite of the selected marker is by default
    public Sprite highlightSelectMarkerSprite; // the sprite of the selected marker when it is highlighted.

    // for displaying the code variable name of this link block
    public Image variableNamePanel;
    public Text variableNameText;
    public string variableName; // the string representing the variable name of this link

    // special flags for the type of link block.
    public bool isStartingLink;
    public bool isHelicopterLink;
    // whether to render this arrow or not.
    public bool renderArrow;

    void Start()
    {
        if (containerPlatform == null)
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
            if ((containerPlatform != null && containerPlatform.isPhasedOut) || (isHelicopterLink && connectingPlatform.isPhasedOut))
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

    public void setDisplayMarker(bool b)
    {
        setDisplayMarker(b, false); 
    }

    public void setDisplayMarker(bool b, bool highlighted)
    {
        if (!highlighted && transform.Find("SelectMarker").GetComponent<SpriteRenderer>().sprite != defaultSelectMarkerSprite)
        {
            transform.Find("SelectMarker").GetComponent<SpriteRenderer>().sprite = defaultSelectMarkerSprite;
        } else if (highlighted && transform.Find("SelectMarker").GetComponent<SpriteRenderer>().sprite != highlightSelectMarkerSprite)
        {
            transform.Find("SelectMarker").GetComponent<SpriteRenderer>().sprite = highlightSelectMarkerSprite;
        }
        transform.Find("SelectMarker").gameObject.SetActive(b);
        variableNamePanel.gameObject.SetActive(b);
        if (b)
        {
            variableNameText.text = getCodeVariableString();
        }
    }

    void OnMouseOver()
    {

    }

    void OnMouseExit()
    {

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

    public string getCodeVariableString()
    {
        string str = variableName;
        if (this == gameController.startingLink)
        {
            str = "list.head";
        } else if (isHelicopterLink)
        {
            str = "temp";
        }
        else if (containerPlatform != null)
        {
            // simulate accessing the next field from a variable.
            Debug.Log(containerPlatform);
            Debug.Log(containerPlatform.getMostRecentlyConnectedLink());
            str = containerPlatform.getMostRecentlyConnectedLink().getCodeVariableString() + ".next";
        }
        return str;
    }


    public string getLogID()
    {
        return logId;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.GameObject.Interfaces;

/**
 * The behavior for link blocks, which the player can use
 * to connect link blocks to connectable entities. 
 */ 
public class LinkBlockBehavior : MonoBehaviour, Loggable
{
    public string logId; // the ID of this object when it is logged

    public GameController gameController;

    public ConnectableEntity connectingEntity; // this is the platform object this link is pointing to.
    public PlatformBehavior containerPlatform; // if this link block is in a platform, then this is
                                               // the parent (this should be more generalized)

    [Header("Special link flags")]
    public bool isStartingLink; // special flag for the starting link
    public bool isHelicopterLink; // special flag for the helicopter link.

    [Header("Sprites")]
    public Sprite nullLinkSprite; // what to show when it is null.
    public Sprite defaultSprite; // what to show when it is NOT null (default to what it starts as).
    public Sprite defaultSelectMarkerSprite;  // what the sprite of the selected marker is by default
    public Sprite highlightSelectMarkerSprite; // the sprite of the selected marker when it is highlighted.

    [Header("References to internal UI elements")]
    public Image variableNamePanel; // the panel which shows the variable name
    public Text variableNameText; // the text component showing the variable name
    public string variableName; // the string representing the variable name of this link
    
    [Header("Internal render options")]
    public bool renderArrow; // whether to render this arrow or not.

    private Transform linkArrow; // this is the current arrow that is instantiated 
    private Transform linkArrowHead; // the head of the current arrow

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

        if (connectingEntity == null) // only update the sprite if there is no connection
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

        if (renderArrow && connectingEntity != null)
        { 
            // set the arrow color
            Color color = Color.red;
            if ((containerPlatform != null && containerPlatform.isPhasedOut) || (isHelicopterLink && connectingEntity.isPhasedOut()))
            {
                color = Color.gray;  // arrowPreFab = linkArrowFadedPreFab;
            }
            else if (isHelicopterLink)
            {
                color = Color.yellow; // arrowPreFab = linkArrowHelicopterPreFab;
            }
            // create the arrow and save references to the new instances.
            Transform[] linkArrowParts = gameController.createArrowInstanceBetweenLinkPlatform(this, connectingEntity, color);
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
        return connectingEntity != null;
    }

    /**
     *  Remove the connection from this link to the current platform. Precondtion: isConnectedToPlatform() is true.
     */
    public void removeLinkConnection()
    {
        connectingEntity.removeIncomingConnectingLink(this); // set connect platform null to make render update correctly
        connectingEntity = null;
        UpdateLinkArrow();
    }

    /**
     *  Set the playform this is going to be linking to.
     */
    public void setConnectingEntity(ConnectableEntity entity)
    { 
        if (entity != null)
        {
            connectingEntity = entity;
            connectingEntity.addIncomingConnectingLink(this);
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

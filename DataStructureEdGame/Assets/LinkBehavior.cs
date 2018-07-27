using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * This entity can connect to connectable objects. 
 * Links can only be established by being set equal to another link.
 */
public class LinkBehavior : MonoBehaviour {

    // The difference states that a link can be in
    public enum State {
        NORMAL, // no interaction
        HOVER, // the player's mouse is hovering over this link
        SELECTED // the player has clicked on this link.
    };

    // the different types of links that there could be
    public enum Type
    {
        NORMAL,
        HELICOPTER,
        START
    }

    public Type type; // what type of link it is
    public State state; // what the state of the link is
    private string variableName; // set on world generation or by the container entity?

    public ConnectableEntityBehavior connectableEntity; // the entity this link connects to
    public ConnectableEntityBehavior previewConnectableEntity; // the entity that is being previewed to be connected to.
    public ContainerEntityBehavior containerEntity;

    public Transform linePreFab;
    // the variables for the arrow
    public Transform arrowLine;
    public Transform arrowHead;
    // preview arrow
    public Transform previewArrowLine;
    public Transform previewArrowHead;
    private SpriteRenderer selectMarker;
    public Sprite hoverSelectedMarkerSprite;
    public Sprite selectSelectedMarkerSprite;
    public Sprite defaultSprite;
    public Sprite nullSprite;
    // whether or not to render the arrow
    public bool renderArrow;
    public bool selectable; // whether this link can be selected or not.
      
    // variables for displaying the variables
    private Canvas variableCanvas;
    private Text variableText;

	// Use this for initialization
	void Start () {
        renderArrow = true;
        ensureReferences();
        selectMarker.gameObject.SetActive(false); // do not show by default 
    }

    public bool isPointInside(Vector2 point)
    {
        return GetComponent<BoxCollider2D>().OverlapPoint(point) || (connectableEntity != null && GetComponent<BoxCollider2D>().OverlapPoint(point));
    }

    /**
     * Set this connection equal to the given link block
     */
    public void setConnectionEqualTo(ref LinkBehavior lb)
    {
        setConnectionTo(lb.connectableEntity);
    } 

    public void setConnectionTo(ConnectableEntityBehavior ce)
    {
        // remove current connection
        removeCurrentConnection();
        connectableEntity = ce;
        if (ce != null)
        {
            ce.addIncomingLink(this);
        }
        UpdateRendering();
        if (containerEntity != null)
        {
            containerEntity.UpdateChildLinkRendering();
            if (containerEntity.GetComponent<HelicopterRobotBehavior>() != null)
            {
                containerEntity.GetComponent<HelicopterRobotBehavior>().MoveAboveLinkedPlatform();
            }
        }
    }

    /**
     * Set the preview connection of this link equal to the given link block
     */ 
    public void setPreviewConnectionEqualTo(ref LinkBehavior lb)
    {
        setPreviewConnection(lb.connectableEntity);
    }

    public void setPreviewConnection(ConnectableEntityBehavior ce)
    {
        previewConnectableEntity = ce;
        UpdateRendering();
    }

    /**
     * Update the rendering state of the line
     */
    public void UpdateRendering()
    {
        clearArrowInstances();
        // update the sprite
        if (connectableEntity != null)
        {
            if (GetComponent<SpriteRenderer>().sprite != defaultSprite)
            {
                GetComponent<SpriteRenderer>().sprite = defaultSprite;
            }
        } else
        {
            if (GetComponent<SpriteRenderer>().sprite != nullSprite)
            {
                GetComponent<SpriteRenderer>().sprite = nullSprite;
            } 
        }

        // render the line if connecting to something
        if (renderArrow && connectableEntity != null)
        {
            Color c = Color.red;
            if (type == Type.HELICOPTER)
            {
                c = Color.yellow;
            }

            Transform[] arrowParts = createArrowInstanceToEntity(connectableEntity, c);
            arrowLine = arrowParts[0];
            arrowHead = arrowParts[1];
        }

        if (renderArrow && previewConnectableEntity != null)
        {
            Color c = Color.gray;
            c.a = 0.3f;
            Transform[] pArrowParts = createArrowInstanceToEntity(previewConnectableEntity, c);
            previewArrowLine = pArrowParts[0];
            previewArrowHead = pArrowParts[1];
        }

        // update this sprite based on its interaction state
        if (state == State.HOVER)
        {
            selectMarker.gameObject.SetActive(true);
            selectMarker.sprite = hoverSelectedMarkerSprite;
        } else if (state == State.SELECTED)
        {
            selectMarker.gameObject.SetActive(true);
            selectMarker.sprite = selectSelectedMarkerSprite;
            // display the selected marker.
            // show the selected marker.
        } else if (state == State.NORMAL)
        {
            selectMarker.gameObject.SetActive(false);  
        }

        // if it is selected or hover, then show variable next
        if (state == State.HOVER || state == State.SELECTED)
        {
            variableCanvas.gameObject.SetActive(true);
            variableText.text = getVariableName();
        } else if (state == State.NORMAL)
        {
            variableCanvas.gameObject.SetActive(false);
        }
    }

    public void clearArrowInstances()
    {
        if (arrowLine != null)
        {
            Destroy(arrowLine.gameObject);
            arrowLine = null;
            Destroy(arrowHead.gameObject);
            arrowHead = null;
        }
        if (previewArrowLine != null)
        {
            Destroy(previewArrowLine.gameObject);
            previewArrowLine = null;
            Destroy(previewArrowHead.gameObject);
            previewArrowHead = null;
        }
    }

    /**
     * Instantiate an arrow that goes from the given link block to the given platform using the given color.
     */
    private Transform[] createArrowInstanceToEntity(ConnectableEntityBehavior ce, Color color)
    {
        // determine the start and end points of the arrow.
        Bounds linkBounds = GetComponent<SpriteRenderer>().bounds;
        Bounds platBounds = ce.getSpriteBounds();

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
    private Transform[] createArrowInstanceBetweenPoints(Vector3 pFrom, Vector3 pTo, Color color)
    {
        // index 0 is the line; index 1 is the head.
        Transform[] arrowParts = new Transform[2];
        arrowParts[0] = Instantiate(linePreFab, pFrom, Quaternion.identity);
        arrowParts[0].name = "Arrow line from " + GetComponent<LoggableBehavior>().getLogID();
        arrowParts[1] = Instantiate(linePreFab, pFrom, Quaternion.identity);
        arrowParts[1].name = "Arrow head from " + GetComponent<LoggableBehavior>().getLogID();
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

    private void removeCurrentConnection()
    {
        if (connectableEntity != null)
        {
            connectableEntity.removeIncomingLink(this);
            connectableEntity = null;
            if (containerEntity != null)
                containerEntity.UpdateChildLinkRendering();
        }
    }

    public void setState(State s)
    {
        if (state != s) { 
            state = s;
            UpdateRendering();
        }
    }

    public void setRenderArrow(bool r)
    {
        if (r != renderArrow)
        {
            renderArrow = r;
            UpdateRendering();
        }
    }

    public void ensureReferences()
    {
        if (selectMarker == null) { 
            selectMarker = transform.Find("SelectMarker").GetComponent<SpriteRenderer>();
        }
        if (variableCanvas == null)
        {
            variableCanvas = transform.Find("Canvas").GetComponent<Canvas>();
        }
        if (variableText == null)
        {
            variableText = transform.Find("Canvas/VariableNameDisplay/Text").GetComponent<Text>();
        }
    }

    public void setContainerEntity(ContainerEntityBehavior ceb)
    {
        containerEntity = ceb;
    }

    public bool isParentContainerHidden()
    {
        return (containerEntity != null && containerEntity.isHidden());
    }

    public void setVariableName(string vn)
    {
        variableName = vn;
    }

    public string getVariableName()
    {
        if (containerEntity == null)
        {
            return variableName;
        }

        if (containerEntity != null && containerEntity.GetComponent<ConnectableEntityBehavior>() != null && 
                containerEntity.GetComponent<ConnectableEntityBehavior>().getMostRecentlyConnectedLink() != null)
        {
            LinkBehavior mostRecent = containerEntity.GetComponent<ConnectableEntityBehavior>().getMostRecentlyConnectedLink();
            string mostRecentVarName = mostRecent.getVariableName();
            return mostRecentVarName + ".next";
        }
        return variableName;
    }
}

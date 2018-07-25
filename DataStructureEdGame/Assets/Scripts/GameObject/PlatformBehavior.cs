using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlatformBehavior : MonoBehaviour, ConnectableEntity, LinkContainerEntity, ValueEntity, Loggable
{ 
    public string logId; // the ID of this object when it is logged

    public GameController gameController; // reference to the very important game controller.
    public List<LinkBlockBehavior> incomingConnectionLinkBlocks; // the link block that connects to this platform, or null if not being connected to. 
    private LinkBlockBehavior childLink; // reference to the child link object.
    private GameObject childValueBlock; // reference to the child link object.
    private int value;

    public Material defaultChildMaterial; // the material for when the children objects are revealed.
    public Material fadedChildMaterial; // The material for when the children objected are hidden
    public Sprite defaultSprite; // initial sprite
    public Sprite phasedOutSprite; // sprite to display when phased out.

    // game specific values
    private bool hidden; // if not Hidden, then Revealed.
    private bool phasedOut; // if not Phased Out, then Solid. 
    public bool isInLevel; // whether this platform has been placed in the level yet or not

    void Start()
    {
        childLink.GetComponent<LinkBlockBehavior>().gameController = gameController;
        childLink.GetComponent<LinkBlockBehavior>().containerEntity = this; 
    }

    /**
     * Remove an incoming link reference to this platform. 
     */
    void ConnectableEntity.removeIncomingConnectingLink(LinkBlockBehavior link)
    {
        incomingConnectionLinkBlocks.Remove(link); // remove this reference to the link
        link.connectingEntity = null; // remove the reference to this platform in the link 
        gameController.updatePlatformEntities(); //  updatePlatformValuesAndSprite();
    }

    /**
     * Set what Link block this platform is being connected by. Also updates Platform state and render information.
     */
    void ConnectableEntity.addIncomingConnectingLink(LinkBlockBehavior link)
    {
        incomingConnectionLinkBlocks.Add(link);
        gameController.updatePlatformEntities(); // updatePlatformValuesAndSprite();
    }


    /**
     * Check if this platform is connected by the special starting link
     */
    public bool isPlatformConnectingToStart()
    {
        List<PlatformBehavior> alreadySearchedPlatforms = new List<PlatformBehavior>();
        PlatformBehavior temp = (PlatformBehavior)gameController.startingLink.connectingEntity;
        while (temp != null)
        {
            if (temp == this)
            {
                return true;
            }
            if (temp.childLink != null)
            {
                alreadySearchedPlatforms.Add(temp);
                temp = (PlatformBehavior)temp.childLink.GetComponent<LinkBlockBehavior>().connectingEntity;
                if (alreadySearchedPlatforms.Contains(temp)) // you have reached the end of the list or there is an infinite loop
                    return false;
            }
        }
        return false;
    }
    
    public void setDisplaySelected(bool b)
    { 
        transform.Find("SelectMarker").gameObject.SetActive(b);
    }

    /**
     * Make this platform look faded for the add platform mechanic
     */ 
    public void renderAsFadedPreview()
    {
        GetComponent<SpriteRenderer>().sprite = phasedOutSprite;
        childLink.GetComponent<SpriteRenderer>().material = fadedChildMaterial;
        childValueBlock.GetComponent<SpriteRenderer>().material = fadedChildMaterial;
    }

    /**
     * Update the link arrow of this platform's child
     */ 
    public void UpdateChildLinkArrow()
    {
        bool old_state = childLink.gameObject.activeSelf;
        childLink.gameObject.SetActive(true);
        childLink.UpdateLinkArrow();
        childLink.gameObject.SetActive(old_state);
    }

    /**
     * Get the LinkBlock which most recently connected to this platform.
     */
    LinkBlockBehavior ConnectableEntity.getMostRecentlyConnectedLink()
    {
        if (incomingConnectionLinkBlocks.Count > 0)
        {
            LinkBlockBehavior linkToReturn = null;
            foreach (LinkBlockBehavior lb in incomingConnectionLinkBlocks)
            {
                if (lb.isHelicopterLink || lb.isStartingLink || lb.containerEntity == null)
                {
                    linkToReturn = lb;
                    break;
                }
            }
            if (linkToReturn != null) { 
                return linkToReturn;  
            }
            return incomingConnectionLinkBlocks[incomingConnectionLinkBlocks.Count - 1]; ;
        } else { 
            return null;
        }
    }

    /**
     * Update the sprite based on the isHidden, isPhasedOut values. 
     */
    void LinkContainerEntity.updateRenderAndState()
    {
        phasedOut = !isPlatformConnectingToStart();
        if (phasedOut)
        {
            if (GetComponent<SpriteRenderer>().sprite != phasedOutSprite)
            {
                GetComponent<SpriteRenderer>().sprite = phasedOutSprite;
            }
            GetComponent<BoxCollider2D>().isTrigger = true; // so player can pass through it.   
        }
        else // solid
        {
            if (GetComponent<SpriteRenderer>().sprite != defaultSprite) // default sprite for solid platform.
            {
                GetComponent<SpriteRenderer>().sprite = defaultSprite;
            }
            GetComponent<BoxCollider2D>().isTrigger = false;
        }

        // see if you are connected to a helicopter link. If so then make block Revealed
        hidden = true;
        foreach (LinkBlockBehavior lnk in incomingConnectionLinkBlocks)
        {
            // if the link going to it is the helicopter or an external link then it is revealed.
            // also reveal this platform if the previous connecting link is the hover link whiel selecting a link
            if (lnk.isHelicopterLink || lnk.containerEntity == null ||
                  (lnk == gameController.hoverLinkRef && gameController.selectedLink != null) ||
                  (gameController.hoverLinkRef == childLink))
            {
                hidden = false; // Reveal block.
                if ((gameController.hoverLinkRef == childLink))
                    Debug.Log("Revealed for being a hover block");
                break;
            }
        }

        // set the material of the children based on if its hidden or not.
        if (hidden && childLink.GetComponent<SpriteRenderer>().material != fadedChildMaterial)
        {
            childLink.GetComponent<SpriteRenderer>().material = fadedChildMaterial;
            childValueBlock.GetComponent<SpriteRenderer>().material = fadedChildMaterial;
            setValueBlockText("?"); // can't see the value
            childLink.GetComponent<BoxCollider2D>().enabled = false;
        }
        else if (!hidden && childLink.GetComponent<SpriteRenderer>().material != defaultChildMaterial)
        {
            childLink.GetComponent<SpriteRenderer>().material = defaultChildMaterial;
            childValueBlock.GetComponent<SpriteRenderer>().material = defaultChildMaterial;
            setValueBlockText("" + value); // can see the value
            childLink.GetComponent<BoxCollider2D>().enabled = true;
        }

    }

    bool ContainerEntity.isPhasedOut()
    {
        return phasedOut;
    }

    bool ContainerEntity.isHidden()
    {
        return hidden;
    }
    
    void ValueEntity.setValue(int i)
    {
        if (childValueBlock != null)
        {
            value = i;
            setValueBlockText("" + value);
        }
    }

    int ValueEntity.getValue()
    {
        return value;
    }

    public LinkBlockBehavior getChildLink()
    {
        return childLink;
    }

    void LinkContainerEntity.setChildLink(LinkBlockBehavior lb)
    {
        childLink = lb;
    }
    
    string Loggable.getLogID()
    {
        return logId;
    }

    public void setChildValueBlock(GameObject vb)
    {
        childValueBlock = vb;
    }

    public void setValueBlockText(string s)
    {
        if (childValueBlock != null)
        {
            childValueBlock.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = s;
        }
    }
}

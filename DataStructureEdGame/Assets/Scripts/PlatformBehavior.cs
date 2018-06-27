using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlatformBehavior : MonoBehaviour {

    public string logId; // the ID of this object when it is logged

    public GameController gameController; // reference to the very important game controller.
    public List<LinkBlockBehavior> incomingConnectionLinkBlocks; // the link block that connects to this platform, or null if not being connected to.
    public GameObject childLink; // reference to the child link object.
    public GameObject childValueBlock; // reference to the child link object.
    private int value;

    public Material defaultChildMaterial; // the material for when the children objects are revealed.
    public Material fadedChildMaterial; // The material for when the children objected are hidden
    public Sprite defaultSprite; // initial sprite
    public Sprite phasedOutSprite; // sprite to display when phased out.

    // game specific values
    public bool isHidden; // if not Hidden, then Revealed. 
    public bool isPhasedOut; // if not Phased Out, then Solid. 
    public bool isInLevel;

    /**
     * Remove an incoming link reference to this platform. 
     */
    public void removeIncomingConnectingLink(LinkBlockBehavior link)
    {
        incomingConnectionLinkBlocks.Remove(link); // remove this reference to the link
        link.connectingPlatform = null; // remove the reference to this platform in the link 
        gameController.updatePlatformEntities(); //  updatePlatformValuesAndSprite();
    }

    /**
     * Set what Link block this platform is being connected by. Also updates Platform state and render information.
     */
    public void addIncomingConnectingLink(LinkBlockBehavior link)
    {
        incomingConnectionLinkBlocks.Add(link);
        gameController.updatePlatformEntities(); // updatePlatformValuesAndSprite();
    }

    /**
     * Update the sprite based on the isHidden, isPhasedOut values. 
     */
    public void updatePlatformValuesAndSprite()
    {
        isPhasedOut = !isPlatformConnectingToStart();
        if (isPhasedOut)
        {
            if (GetComponent<SpriteRenderer>().sprite != phasedOutSprite)
            {
                GetComponent<SpriteRenderer>().sprite = phasedOutSprite;
            }
            GetComponent<BoxCollider2D>().isTrigger = true; // so player can pass through it.   
        } else // solid
        {
            if (GetComponent<SpriteRenderer>().sprite != defaultSprite) // default sprite for solid platform.
            {
                GetComponent<SpriteRenderer>().sprite = defaultSprite;
            }
            GetComponent<BoxCollider2D>().isTrigger = false; 
        }
        
        // see if you are connected to a helicopter link. If so then make block Revealed
        isHidden = true;
        foreach (LinkBlockBehavior lnk in incomingConnectionLinkBlocks)
        {
            // if the link going to it is the helicopter or an external link then it is revealed.
            // also reveal this platform if the previous connecting link is the hover link whiel selecting a link
            if (lnk.isHelicopterLink || lnk.parentPlatform == null || 
                (lnk == gameController.hoverLinkRef && gameController.selectedLink != null))
            {
                isHidden = false; // Reveal block.
                break;
            }
        }
        
        // set the material of the children based on if its hidden or not.
        if (isHidden && childLink.GetComponent<SpriteRenderer>().material != fadedChildMaterial)
        {
            childLink.GetComponent<SpriteRenderer>().material = fadedChildMaterial;
            childValueBlock.GetComponent<SpriteRenderer>().material = fadedChildMaterial;
            setValueBlockText("?"); // can't see the value
            childLink.GetComponent<BoxCollider2D>().enabled = false;
        } else if (!isHidden && childLink.GetComponent<SpriteRenderer>().material != defaultChildMaterial)
        {
            childLink.GetComponent<SpriteRenderer>().material = defaultChildMaterial;
            childValueBlock.GetComponent<SpriteRenderer>().material = defaultChildMaterial;
            setValueBlockText("" + value); // can see the value
            childLink.GetComponent<BoxCollider2D>().enabled = true;
        }
        
    }

    /**
     * Check if this platform is connected by the special starting link
     */
    public bool isPlatformConnectingToStart()
    {
        List<PlatformBehavior> alreadySearchedPlatforms = new List<PlatformBehavior>();
        PlatformBehavior temp = gameController.startingLink.connectingPlatform;
        while (temp != null)
        {
            if (temp == this)
            {
                return true;
            }
            if (temp.childLink != null)
            {
                alreadySearchedPlatforms.Add(temp);
                temp = temp.childLink.GetComponent<LinkBlockBehavior>().connectingPlatform;
                if (alreadySearchedPlatforms.Contains(temp)) // you have reached the end of the list or there is an infinite loop
                    return false;
            }
        }
        return false;
    }

    void OnMouseEnter()
    {

    }

    void OnMouseExit()
    {

    }

    public void setDisplaySelected(bool b)
    { 
        transform.Find("SelectMarker").gameObject.SetActive(b);
    }

    public void setValue(int s)
    {
        if (childValueBlock != null) { 
            value = s;
            setValueBlockText("" + value);
        }
    }
    
    public void setValueBlockText(string s)
    {
        if (childValueBlock != null)
        {
            childValueBlock.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = s;
        }
    }

    public int getValue()
    {
        return value;
    }
    
    // Use this for initialization
    void Start () {
        // defaultSprite = GetComponent<SpriteRenderer>().sprite;
        // establish child link connections
        childLink = transform.Find("LinkBlock").gameObject;
        childValueBlock = transform.Find("ValueBlock").gameObject;
        childLink.GetComponent<LinkBlockBehavior>().gameController = gameController;
        childLink.GetComponent<LinkBlockBehavior>().parentPlatform = this;
        // updatePlatformValuesAndSprite();
    }

    // Update is called once per frame
    void Update () {

    }

    public string getLogID()
    {
        return logId;
    }
}

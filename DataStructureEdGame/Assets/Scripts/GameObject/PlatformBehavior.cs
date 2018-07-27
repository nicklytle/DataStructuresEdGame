using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlatformBehavior : MonoBehaviour
{ 
    public GameController gameController; // reference to the very important game controller.
    
    public Sprite defaultSprite; // initial sprite
    public Sprite phasedOutSprite; // sprite to display when phased out.

    public bool isInLevel; // whether this platform has been placed in the level yet or not
    public bool phasedOut; // if not Phased Out, then Solid.
    
    /**
     * Make this platform look faded for the add platform mechanic
     */ 
    public void renderAsFadedPreview()
    {
        GetComponent<SpriteRenderer>().sprite = phasedOutSprite;
        GetComponent<ContainerEntityBehavior>().setHidden(true);
    }

    /**
     * Update the sprite based on the isHidden, isPhasedOut values. 
     */
    public void updateRenderAndState()
    {
        phasedOut = !GetComponent<ConnectableEntityBehavior>().isConnectedToStart(gameController.startingLink);
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
        
        bool hide = true;
        foreach (LinkBehavior lb in GetComponent<ConnectableEntityBehavior>().incomingConnectionLinks)
        {
            // reveal if being pointed at by a start block, helicopter robot, or an external link block.
            if (lb.type == LinkBehavior.Type.START || lb.type == LinkBehavior.Type.HELICOPTER || (lb.containerEntity == null))
            {
                hide = false; // Reveal block.
                break;
            }
        }

        GetComponent<ContainerEntityBehavior>().setHidden(hide);
        if (GetComponent<ContainerEntityBehavior>().isHidden())
        {
            setValueBlockText("?");
        } else
        {
            setValueBlockText("" + getValue());
        }
    }

    public void setValue(int i)
    {
        if (GetComponent<ContainerEntityBehavior>().GetChildComponent<ValueBehavior>() != null)
        {
            GetComponent<ContainerEntityBehavior>().GetChildComponent<ValueBehavior>().setValue(i);
            setValueBlockText("" + getValue());
        }
    }

    public int getValue()
    {
        return GetComponent<ContainerEntityBehavior>().GetChildComponent<ValueBehavior>().getValue();
    }

    public LinkBehavior getChildLink()
    {
        LinkBehavior lb = GetComponent<ContainerEntityBehavior>().GetChildComponent<LinkBehavior>();
        if (lb == null)
        {
            GetComponent<ContainerEntityBehavior>().refreshChildList();
            lb = GetComponent<ContainerEntityBehavior>().GetChildComponent<LinkBehavior>();
        }
        return lb;
    }

    public bool isPhasedOut()
    {
        return phasedOut;
    }
    
    public void setValueBlockText(string s)
    {
        if (GetComponent<ContainerEntityBehavior>().GetChildComponent<ValueBehavior>() != null)
        {
            GetComponent<ContainerEntityBehavior>().GetChildComponent<ValueBehavior>().transform.Find("Canvas/ValueText").GetComponent<Text>().text = s;
        }
    }
}

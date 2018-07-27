using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Links can connect to this entity. It's state may change
 * depending on if it is connected to the start or not.
 */
public class ConnectableEntityBehavior : MonoBehaviour {

    public List<LinkBehavior> incomingConnectionLinks; // the link block that connects to this platform, or null if not being connected to. 
    
    public void removeIncomingLink(LinkBehavior lb)
    {
        incomingConnectionLinks.Remove(lb);
    }

    public void addIncomingLink(LinkBehavior lb)
    {
        incomingConnectionLinks.Add(lb);
    }

    public LinkBehavior getMostRecentlyConnectedLink()
    {
        if (incomingConnectionLinks.Count > 0)
        {
            LinkBehavior linkToReturn = null;
            foreach (LinkBehavior lb in incomingConnectionLinks)
            {
                if (lb.type == LinkBehavior.Type.HELICOPTER || lb.type == LinkBehavior.Type.START || lb.containerEntity == null)
                {
                    linkToReturn = lb;
                    break;
                }
            }
            if (linkToReturn != null)
            {
                return linkToReturn;
            }
            return incomingConnectionLinks[incomingConnectionLinks.Count - 1]; // return last one added
        }
        return null;
    }

    // to help links render a line to this entity
    public Bounds getSpriteBounds()
    {
        return GetComponent<SpriteRenderer>().bounds;
    }

    /**
     * Check if this platform is connected by the special starting link
     */
    public bool isConnectedToStart(LinkBehavior startingLink)
    {
        List<ConnectableEntityBehavior> alreadySearched = new List<ConnectableEntityBehavior>();
        ConnectableEntityBehavior temp = startingLink.connectableEntity;
        while (temp != null)
        {
            if (temp == this)
            {
                return true;
            }
            if (temp.GetComponent<ContainerEntityBehavior>() != null && 
                temp.GetComponent<ContainerEntityBehavior>().GetChildComponent<LinkBehavior>() != null &&
                temp.GetComponent<ContainerEntityBehavior>().GetChildComponent<LinkBehavior>().connectableEntity != null)
            {
                alreadySearched.Add(temp);
                temp = temp.GetComponent<ContainerEntityBehavior>().GetChildComponent<LinkBehavior>().connectableEntity;
                if (alreadySearched.Contains(temp)) // you have reached the end of the list or there is an infinite loop
                    return false;
            } else
            {
                return false;
            }
        }
        return false;
    }
}

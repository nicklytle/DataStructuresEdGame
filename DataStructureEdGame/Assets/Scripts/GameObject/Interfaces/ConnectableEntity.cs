using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/**
    * A general interface that links can connect to.
    */
public interface ConnectableEntity : Loggable
{
    LinkBlockBehavior getMostRecentlyConnectedLink();
    void removeIncomingConnectingLink(LinkBlockBehavior lb);
    void addIncomingConnectingLink(LinkBlockBehavior lb);
}


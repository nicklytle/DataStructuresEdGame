using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.GameObject.Interfaces
{
    /**
     * A general interface that links can connect to.
     */
    public interface ConnectableEntity : Loggable
    {
        bool isPhasedOut();
        bool isHidden();
        void removeIncomingConnectingLink(LinkBlockBehavior lb);
        void addIncomingConnectingLink(LinkBlockBehavior lb);
    }
}

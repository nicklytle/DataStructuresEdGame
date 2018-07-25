using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/**
* For objects which have only one outgoing link.
*/
public interface LinkContainerEntity : ContainerEntity
{
    LinkBlockBehavior getChildLink();
    void setChildLink(LinkBlockBehavior lb);
    void updateRenderAndState();
}



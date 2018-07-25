using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
 * For any entity which contains other objects that may be hidden or revealed.
 */
public interface ContainerEntity
{
    bool isPhasedOut();
    bool isHidden();
}


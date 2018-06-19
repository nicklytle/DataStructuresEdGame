using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.WorldGeneration
{
    // A single linked list Platform for Single linked list levels
    [Serializable]
    class SingleLinkedListPlatform : Block
    {
        // type, x, and y from Block

        // the ID that LinkBlocks will use to refer to this block
        public string objId;
        // this platform's inner link
        public string childLinkBlockConnectId;

        public int value;
        public bool toAdd;

    }
}

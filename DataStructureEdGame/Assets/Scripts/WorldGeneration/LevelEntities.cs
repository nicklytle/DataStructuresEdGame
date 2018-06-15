using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.WorldGeneration
{
    [Serializable]
    class LevelEntities
    {
        // the specific player and startLink objects in this level.
        public Block player;
        public LinkBlock startLink;
        public Block goalPortal;

        public Block[] blocks; // basic blocks
        public LinkBlock[] linkBlocks; 

        // stuff specific to single-linked lists.
        public SingleLinkedListPlatform[] singleLinkedListPlatforms;
    }
}

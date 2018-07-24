using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.WorldGeneration
{
    /**
     * The main class with all objects to describe an entire level.
     * This class is for level file serialization.
     */
    [Serializable]
    class LevelEntities
    {
        public string winCondition;
        // the specific player and startLink objects in this level.
        public Block player;
        public LinkBlock startLink;
        public Block goalPortal;
        public Block helicopterRobot;

        public SizedBlock[] blocks; // basic blocks
        public LinkBlock[] linkBlocks;
        public Block[] objectiveBlocks;

        // stuff specific to single-linked lists.
        public SingleLinkedListPlatform[] singleLinkedListPlatforms;

        public InstructionBlock[] instructionBlocks;
    }
}

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
    class LevelEntitiesJSON
    {
        public string winCondition;
        // the specific player and startLink objects in this level.
        public BlockJSON player;
        public LinkBlockJSON startLink;
        public BlockJSON goalPortal;
        public BlockJSON helicopterRobot;

        public SizedBlockJSON[] blocks; // basic blocks
        public LinkBlockJSON[] linkBlocks;
        public BlockJSON[] objectiveBlocks;

        // stuff specific to single-linked lists.
        public SingleLinkedListPlatformJSON[] singleLinkedListPlatforms;

        public InstructionBlockJSON[] instructionBlocks;
    }
}

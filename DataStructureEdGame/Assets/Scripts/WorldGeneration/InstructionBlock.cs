using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.WorldGeneration
{
    /**
     * An instruction block.
     * This class is for level file serialization.
     */
    [Serializable]
    class InstructionBlock : Block
    {
        public string screenId;
    }
}

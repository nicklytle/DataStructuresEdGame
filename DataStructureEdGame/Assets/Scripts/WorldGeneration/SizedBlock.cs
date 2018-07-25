using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.WorldGeneration
{
    /**
     * A basic block with extra size information for when the size can vary.
     * This class is for level file serialization.
     */
    [Serializable]
    class SizedBlockJSON : BlockJSON
    {
        public double width;
        public double height;
    }
}

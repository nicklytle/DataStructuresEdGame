using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.WorldGeneration
{
    /**
     * A Basic Block in the World
     */
    [Serializable]
    class Block
    {
        public string type;
        public double x;
        public double y; 
    }
}

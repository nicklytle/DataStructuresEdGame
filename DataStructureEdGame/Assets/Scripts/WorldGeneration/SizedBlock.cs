using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.WorldGeneration
{
    [Serializable]
    class SizedBlock : Block
    {
        public double width;
        public double height;
    }
}

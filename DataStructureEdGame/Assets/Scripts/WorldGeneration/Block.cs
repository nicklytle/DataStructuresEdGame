﻿using System;
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
        public string logId; // the ID of the object when it is logged.
        public double x;
        public double y; 
    }
}

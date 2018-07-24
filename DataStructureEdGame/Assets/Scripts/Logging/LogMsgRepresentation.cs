using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.WorldGeneration
{
    /**
     * All of the information for the world state log messge.
     * This class is for log message serialization.
     */
    [Serializable]
    class LogMsgRepresentation
    {
        // has type, x, and y from Block

        // what is the ID of the object this connects to?
        //public Block[] blockPart;
        public LinkBlock[] linkBlockPart;
        public LLPlatformForLogging[] platformPart;
        public Block player;
        public Block helicopter;

        public string SaveString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}

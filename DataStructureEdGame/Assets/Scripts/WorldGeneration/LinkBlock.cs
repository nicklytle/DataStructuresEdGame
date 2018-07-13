using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.WorldGeneration
{
    [Serializable]
    class LinkBlock : Block
    {
        // has type, x, and y from Block

        // what is the ID of the object this connects to?
        public string objIDConnectingTo;

        public string SaveString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.WorldGeneration
{
    /**
     * A link game link block. This may be a stand alone or as a child to another object.
     * This class is for level file serialization.
     */
    [Serializable]
    class LinkBlockJSON : BlockJSON
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

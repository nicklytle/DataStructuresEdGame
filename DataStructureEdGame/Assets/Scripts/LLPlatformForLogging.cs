using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.WorldGeneration
{
    // A single linked list Platform for Single linked list levels
    [Serializable]
    class LLPlatformForLogging : SingleLinkedListPlatform
    {
        // type, x, and y from Block

        //whether platform is hidden, not revealed
        public bool isHidden;
        //whether platform is solid, not phased out
        public bool isSolid;

        
        public override string SaveString()
        {
            return JsonUtility.ToJson(this);
        }

    }
}

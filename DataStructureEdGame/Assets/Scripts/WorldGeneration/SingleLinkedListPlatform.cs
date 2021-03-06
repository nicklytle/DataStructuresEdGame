﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.WorldGeneration
{
    /**
     * A single linked list Platform for Single linked list levels.
     * This class is for level file serialization.
     */
    [Serializable]
    class SingleLinkedListPlatformJSON : BlockJSON
    {
        // type, x, and y from Block

        // the ID that LinkBlocks will use to refer to this block // DOES this replace logId?
        public string objId;
        // this platform's inner link
        public string childLinkBlockConnectId;

        public int value;
        public bool toAdd;

        public override string SaveString()
        {
            return JsonUtility.ToJson(this);
        }

    }
}

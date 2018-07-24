using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Basic ground blocks. This class is only so they can be logged.
 */
public class GroundBehavior : MonoBehaviour, Loggable
{

    public string logId;

    public string getLogID()
    {
        return logId;
    }
}

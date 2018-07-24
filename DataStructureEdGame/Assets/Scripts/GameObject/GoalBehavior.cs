using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * The goal which the player must reach to complete the level.
 */ 
public class GoalBehavior : MonoBehaviour, Loggable
{

    public string logId;

    public string getLogID()
    {
        return logId;
    }
}

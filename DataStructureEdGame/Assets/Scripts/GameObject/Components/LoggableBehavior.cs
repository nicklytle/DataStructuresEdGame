using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggableBehavior : MonoBehaviour {

    private string logId;

    public string getLogID()
    {
        return logId;
    }

    public void setLogID(string s)
    {
        logId = s;
    }
}

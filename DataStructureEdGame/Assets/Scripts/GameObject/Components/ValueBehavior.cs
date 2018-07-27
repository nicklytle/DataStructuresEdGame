using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueBehavior : MonoBehaviour {

    private int value;

    public void setValue(int v)
    {
        value = v;
    }

    public int getValue()
    {
        return value;
    }
}

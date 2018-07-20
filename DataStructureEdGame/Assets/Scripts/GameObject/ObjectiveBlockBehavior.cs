using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveBlockBehavior : MonoBehaviour, Loggable
{

    public string logId;

    public Sprite solidSprite;
    public Sprite fadedSprite;

    public bool canMoveThrough;
     
    public void updatedFadedState(bool winConditionSatisfied)
    {
        // check if the win condition is met or not.
        canMoveThrough = winConditionSatisfied;
        //Debug.Log("We have updated the faded state of this objective block");
        //Debug.Log(canMoveThrough);
        GetComponent<BoxCollider2D>().isTrigger = canMoveThrough;
        if (canMoveThrough)
        {
            if (GetComponent<SpriteRenderer>().sprite != fadedSprite)
            {
                GetComponent<SpriteRenderer>().sprite = fadedSprite;
            }
        } else
        {
            if (GetComponent<SpriteRenderer>().sprite != solidSprite)
            {
                GetComponent<SpriteRenderer>().sprite = solidSprite;
            }
        }
    }

	// Use this for initialization
	void Start () {
        

    }
	
	// Update is called once per frame
	void Update () {

    }

    public string getLogID()
    {
        return logId;
    }
}

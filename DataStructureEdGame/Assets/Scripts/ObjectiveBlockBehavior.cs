using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveBlockBehavior : MonoBehaviour {

    public GameController gameController;

    // what win condition we need to make this block faded.
    public GameController.WinCondition winConditon;

    public Sprite solidSprite;
    public Sprite fadedSprite;

    public bool canMoveThrough;

    
    bool isWinConditonSatisfied()
    {
        if (gameController == null && gameController.startingLink == null)
        {
            return false;
        }
        switch(winConditon)
        {
            case GameController.WinCondition.SortListAscending:
            case GameController.WinCondition.SortListDescending:
                if (gameController.startingLink.connectingPlatform == null)
                {
                    return true;
                }
                if (gameController.startingLink.connectingPlatform.childLink.GetComponent<LinkBlockBehavior>().connectingPlatform == null)
                {
                    return true;
                }
                PlatformBehavior temp = gameController.startingLink.connectingPlatform.GetComponent<PlatformBehavior>();
                while (temp != null)
                {
                    PlatformBehavior next = temp.childLink.GetComponent<LinkBlockBehavior>().connectingPlatform;
                    if (next == null)
                    {
                        return true;
                    } else
                    { 
                        if ((winConditon == GameController.WinCondition.SortListAscending && next.getValue() < temp.getValue()) ||
                            (winConditon == GameController.WinCondition.SortListDescending && next.getValue() > temp.getValue()))
                        {
                            return false;
                        } // otherwise just keep on iterating.
                    } 
                    temp = next;
                }
                break; 
        }
        return false;
    }

    public void updatedFadedState()
    {
        // check if the win condition is met or not.
        canMoveThrough = isWinConditonSatisfied();
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Behavior for objective blocks, which are solid when the 
 * level's win condition is not yet met, and become passable 
 * when the level's win condition is met.
 */
public class ObjectiveBlockBehavior : MonoBehaviour
{ 
    [Header("Sprites")]
    public Sprite solidSprite;
    public Sprite fadedSprite;

    // the current state of the block.
    public bool canMoveThrough;
     
    public void updatedFadedState(bool winConditionSatisfied)
    {
        // check if the win condition is met or not.
        canMoveThrough = winConditionSatisfied; 
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

    
}

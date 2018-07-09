using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviorSideToSide : MonoBehaviour {

    public bool movingLeft;
    public float speed;
    public float pushAmount;

    private Rigidbody2D rb;
    private BoxCollider2D bc2d;
    private BoxCollider2D spriteOn;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        bc2d = GetComponent<BoxCollider2D>();

    }
	
	// Update is called once per frame
	void Update () {
        float xvel = speed;
        if (movingLeft)
        {
            xvel *= -1;
        }
        rb.velocity = new Vector2(xvel, 0);

        // see if the enemy is on the edge of the platform they are on.
        if (spriteOn != null) { 
            if (movingLeft)
            {
                double thisLeftEdge = bc2d.bounds.center.x - bc2d.bounds.extents.x;
                double spriteOnLeftEdge = spriteOn.bounds.center.x - spriteOn.bounds.extents.x;
                if (thisLeftEdge < spriteOnLeftEdge)
                {
                    movingLeft = !movingLeft; // reverse direction.
                }
            } else
            {
                double thisRightEdge = bc2d.bounds.center.x + bc2d.bounds.extents.x;
                double spriteOnRightEdge = spriteOn.bounds.center.x + spriteOn.bounds.extents.x;
                if (thisRightEdge > spriteOnRightEdge)
                {
                    movingLeft = !movingLeft; // reverse direction.
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<PlayerBehavior>() != null)
        {
            // shove the player in some direction
            float diffX = collision.collider.transform.position.x - transform.position.x; 
            // determine the direction and normalize it.
            if (diffX < 0)
            {
                diffX = -1;
            } else
            {
                diffX = 1;
            }
            collision.collider.GetComponent<PlayerBehavior>().setShoveForce(new Vector2(diffX * pushAmount, 10f));
        } else {
            // if you did not collide with the player, then see if it was ground. 
            if (collision.collider.GetComponent<BoxCollider2D>() != null && 
                collision.collider.GetComponent<BoxCollider2D>() != spriteOn &&
                collision.collider.transform.position.y < transform.position.y) // must be below this enemy.
            {
                spriteOn = collision.collider.GetComponent<BoxCollider2D>();
            }
        }
    }
}

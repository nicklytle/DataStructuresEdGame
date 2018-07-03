using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour {

    public string logId;

    public GameController gameController;

    private Rigidbody2D rb2;
    private SpriteRenderer sr;
    public float speed;
    public float jumpSpeed;
    public bool onGround;

    public Sprite frontView;
    public Sprite leftSideView; // facing left

    public Vector2 shoveVector;

    public bool startOfMove = false;

    /** the platform or link's logID that the player is currently standing on */
    private string currentlyStandingOn;

    private bool initAssgmt = true;

	// Use this for initialization
	void Start () {
        rb2 = gameObject.GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        onGround = true; // initially assumed to not be on the ground. 
	}

    // Update is called once per frame
    void Update() {
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        
        float oldX = rb2.position.x;
        float oldY = rb2.position.y;

        float newX = horz * Time.deltaTime * speed;
        float newY = rb2.velocity.y;
        //Vector2 newMoveVect = new Vector2(horz * Time.deltaTime * speed, rb2.velocity.y);
        Vector2 newMoveVect = new Vector2(newX, newY);
        
        if((((horz != 0) && (vert == 0)) || ((vert != 0) && (horz == 0))) && (!startOfMove))
        {
            string timestampMove = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Player started moving from: (" + Math.Round(oldX, 1) + ", " + Math.Round(oldY, 1) + ") at time: " + timestampMove);
            startOfMove = true;

        }
        if(vert == 0 && horz == 0 && startOfMove)
        {
            startOfMove = false;
            string timestampEndMove = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Player landed at : (" + Math.Round(rb2.position.x, 1) + ", " + Math.Round(rb2.position.y, 1) + ") at time: " + timestampEndMove);
        }


        // reduce the shove amount as time goes on.
        shoveVector = shoveVector * 0.9f; // to offset the effects of deltaTime
        if (shoveVector.magnitude > 1 || shoveVector.magnitude < -1)
        {
            //Debug.Log("Shove vector magnitude: " + shoveVector.magnitude);
            newMoveVect = newMoveVect + (Time.deltaTime * shoveVector); // add the shove amount to the total movement.
        } 
        rb2.velocity = newMoveVect;

        if (rb2.velocity.y != 0)
        {
            onGround = false;
        }
        if(rb2.velocity.y == 0)
        {
            onGround = true;
        }

        if (horz == 0 && sr.sprite != frontView)
        {
            sr.sprite = frontView;
        }
        else if (horz < 0 && (sr.sprite != leftSideView || sr.flipX))
        {
            sr.sprite = leftSideView;
            sr.flipX = false;
        }
        else if (horz > 0 && (sr.sprite != leftSideView || !sr.flipX))
        {
            sr.sprite = leftSideView;
            sr.flipX = true;
        }

        if (onGround && (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)))
        {
            string timestampJump = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            //Debug.Log("Player: " + logId + " jumped at time: " + timestampJump);
            onGround = false;
            rb2.velocity += new Vector2(0, jumpSpeed); 
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "GroundTop(Clone)")
        {
            //Debug.Log(collision.gameObject.GetComponent<GroundBehavior>().logId);
            if(!initAssgmt && (currentlyStandingOn != collision.gameObject.GetComponent<GroundBehavior>().logId))
            {
                currentlyStandingOn = collision.gameObject.GetComponent<GroundBehavior>().logId;
                string timeStampNewGrnd = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Debug.Log("Player landed onto ground: " + currentlyStandingOn + " at time: " + timeStampNewGrnd);
            }
            if (initAssgmt)
            {
                initAssgmt = false;
                currentlyStandingOn = collision.gameObject.GetComponent<GroundBehavior>().logId;
            }
        }
        else if (collision.gameObject.name == "Platform(Clone)")
        {
            //Debug.Log(collision.gameObject.GetComponent<PlatformBehavior>().logId);
            if (!initAssgmt && (currentlyStandingOn != collision.gameObject.GetComponent<PlatformBehavior>().logId))
            {
                currentlyStandingOn = collision.gameObject.GetComponent<PlatformBehavior>().logId;
                string timeStampNewPlatf = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Debug.Log("Player landed onto platform: " + currentlyStandingOn + " at time: " + timeStampNewPlatf);
            }
            if (initAssgmt)
            {
                initAssgmt = false;
                currentlyStandingOn = collision.gameObject.GetComponent<PlatformBehavior>().logId;
            }
        }

        // for setting onGround, make sure that the other block is below the player
        if (collision.collider.bounds.center.y < GetComponent<BoxCollider2D>().bounds.center.y) { 
            onGround = true;
        } else if (collision.collider.bounds.center.y - collision.collider.bounds.extents.y > GetComponent<BoxCollider2D>().bounds.center.y)
        {
            // stop moving up
            rb2.velocity = new Vector2(rb2.velocity.x, -0.1f);
        }
    }

    void OnTriggerEnter2D(Collider2D c2d)
    {
        if (c2d.tag == "BottomOfWorld")
        {
            String timestamp1 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Player fell off and died, level was reset at time: " + timestamp1);
            gameController.worldGenerator.resetLevel();

        }
        else if (c2d.tag == "GoalPortal")
        {
            String timestamp1 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log(c2d.GetComponent<GoalBehavior>().logId);
            Debug.Log("Level " + (gameController.worldGenerator.levelFileIndex + 1) + " won at time: " + timestamp1);
            gameController.worldGenerator.levelFileIndex = gameController.worldGenerator.levelFileIndex + 1;
            gameController.worldGenerator.resetLevel();
        }
        //else
        //{
        //    Debug.Log(c2d.tag);
        //}
        /**
        else if(c2d.tag == "PlatformTag")
        {
            Debug.Log("Made it here platform");
            String timestamp1 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Player landed on " + c2d.GetComponent<PlatformBehavior>().logId + "at time: " + timestamp1);
        }
        else if (c2d.tag == "GroundTop")
        {
            Debug.Log("Made it here yo");
            String timestamp1 = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Player landed on " + c2d.GetComponent<GroundBehavior>().logId + "at time: " + timestamp1);
        }
        */
    }

    // shove the player in the given direction
    public void setShoveForce(Vector2 sa)
    {

        //Debug.Log("PLayer being shoved!:  " + sa.x + ", " + sa.y);
        shoveVector = sa;
    }

    public string getLogID()
    {
        return logId;
    }
}

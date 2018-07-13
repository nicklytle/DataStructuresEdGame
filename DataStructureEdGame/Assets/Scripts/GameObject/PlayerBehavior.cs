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
            string actMsg = "Player started moving from: (" + Math.Round(oldX, 1) + ", " + Math.Round(oldY, 1) + ")";
            gameController.currentPlayerLogs.send_To_Server(actMsg);
            startOfMove = true;

        }
        if(vert == 0 && horz == 0 && startOfMove)
        {
            startOfMove = false;
            string actMsg2 = "Player landed at : (" + Math.Round(rb2.position.x, 1) + ", " + Math.Round(rb2.position.y, 1) + ")";
            gameController.currentPlayerLogs.send_To_Server(actMsg2);
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
            string actMsg3 = "Player: " + logId + " jumped.";
            gameController.currentPlayerLogs.send_To_Server(actMsg3);
            onGround = false;
            rb2.velocity += new Vector2(0, jumpSpeed); 
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // for setting onGround, make sure that the other block is below the player
        if (collision.collider.bounds.center.y < GetComponent<CircleCollider2D>().bounds.center.y)
        {
            onGround = true;
            if (collision.gameObject.name == "GroundTop(Clone)")
            {
                //Debug.Log(collision.gameObject.GetComponent<GroundBehavior>().logId);
                if (!initAssgmt && (currentlyStandingOn != collision.gameObject.GetComponent<GroundBehavior>().logId))
                {
                    currentlyStandingOn = collision.gameObject.GetComponent<GroundBehavior>().logId;
                    string actMsg = "Player landed onto ground: " + currentlyStandingOn;
                    gameController.currentPlayerLogs.send_To_Server(actMsg);
                    Debug.Log(actMsg);
                }
                if (initAssgmt)
                {
                    initAssgmt = false;
                    currentlyStandingOn = collision.gameObject.GetComponent<GroundBehavior>().logId;
                    string actMsg = "Player started by standing on ground: " + currentlyStandingOn;
                    gameController.currentPlayerLogs.send_To_Server(actMsg);
                    Debug.Log(actMsg);
                }
            }
            else if (collision.gameObject.name == "Platform(Clone)")
            {
                //Debug.Log(collision.gameObject.GetComponent<PlatformBehavior>().logId);
                if (!initAssgmt && (currentlyStandingOn != collision.gameObject.GetComponent<PlatformBehavior>().logId))
                {
                    currentlyStandingOn = collision.gameObject.GetComponent<PlatformBehavior>().logId;
                    string actMsg1 = "Player landed onto platform: " + currentlyStandingOn;
                    gameController.currentPlayerLogs.send_To_Server(actMsg1);
                    Debug.Log(actMsg1);
                }
                if (initAssgmt)
                {
                    initAssgmt = false;
                    currentlyStandingOn = collision.gameObject.GetComponent<PlatformBehavior>().logId;
                    string actMsg = "Player started by standing on platform: " + currentlyStandingOn;
                    gameController.currentPlayerLogs.send_To_Server(actMsg);
                    Debug.Log(actMsg);
                }
            }
        }
        // else if (collision.collider.bounds.center.y - collision.collider.bounds.extents.y > GetComponent<BoxCollider2D>().bounds.center.y)
        else if (collision.collider.bounds.center.y - collision.collider.bounds.extents.y > GetComponent<CircleCollider2D>().bounds.center.y)
        {
            // stop moving up
            rb2.velocity = new Vector2(rb2.velocity.x, -0.1f);
        }
    }

    void OnTriggerEnter2D(Collider2D c2d)
    {
        if (c2d.tag == "BottomOfWorld")
        {
            string actMsg = "Player fell off and died, level was reset";
            gameController.currentPlayerLogs.send_To_Server(actMsg);
            gameController.worldGenerator.resetLevel();

        }
        else if (c2d.tag == "GoalPortal")
        {
            string actMsg = "Level " + (gameController.worldGenerator.levelFileIndex + 1) + " won at time";
            gameController.currentPlayerLogs.send_To_Server(actMsg);

            gameController.worldGenerator.levelFileIndex = gameController.worldGenerator.levelFileIndex + 1;
            gameController.currentPlayerLogs.beginUpdateLastLevelOn(); // update what level the player is on
            gameController.worldGenerator.resetLevel();
        }
        else if (c2d.tag == "InstructionViewBlock")
        {
            gameController.showInstructionScreen(c2d.GetComponent<InstructionViewBlockBehavior>().screenId);
        }

    }

    void OnTriggerExit2D(Collider2D c2d)
    {
        if (c2d.tag == "InstructionViewBlock")
        {
            gameController.hideInstructionScreen(c2d.GetComponent<InstructionViewBlockBehavior>().screenId);
        }
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

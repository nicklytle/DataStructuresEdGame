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
        rb2.velocity = new Vector2(horz * Time.deltaTime * speed, rb2.velocity.y);
        if(rb2.velocity.y != 0)
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

        if (onGround && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
        {
            onGround = false;
            rb2.velocity += new Vector2(0, jumpSpeed); 
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // TODO: Only have this true when you are colliding on the ground below you.    
        
        onGround = true;
    }

    void OnTriggerEnter2D(Collider2D c2d)
    {
        if (c2d.tag == "BottomOfWorld")
        { 
            gameController.worldGenerator.resetLevel();
        }
        else if (c2d.tag == "GoalPortal")
        {
            gameController.worldGenerator.levelFileIndex = gameController.worldGenerator.levelFileIndex + 1;
            gameController.worldGenerator.resetLevel();
        }
    }

    public string getLogID()
    {
        return logId;
    }
}

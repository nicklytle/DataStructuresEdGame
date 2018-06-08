using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

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
    void Update () { 
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        rb2.velocity = new Vector2(horz * Time.deltaTime * speed, rb2.velocity.y);

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


        if (onGround && Input.GetKeyDown(KeyCode.Space))
        {
            onGround = false;
            rb2.velocity += new Vector2(0, jumpSpeed);
            // this.gameObject.GetComponent<Rigidbody2D>().AddForce(-Vector2.up * jumpSpeed);
        } 

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collide");
        onGround = true;
    }
}

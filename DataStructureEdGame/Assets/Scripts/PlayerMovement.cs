using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    private Rigidbody2D rb2;
    public float speed;
    public float jumpSpeed;
    public bool onGround;

	// Use this for initialization
	void Start () {
        rb2 = gameObject.GetComponent<Rigidbody2D>();
        onGround = true; // initially assumed to not be on the ground. 
	}

    // Update is called once per frame
    void Update () {

        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        rb2.velocity = new Vector2(horz * Time.deltaTime * speed, rb2.velocity.y);

        if (onGround && Input.GetKeyDown(KeyCode.Space))
        {
            // onGround = false;
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

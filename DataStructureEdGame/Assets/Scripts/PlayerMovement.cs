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

        if (onGround && vert > 0 )
        {
            onGround = false;
            rb2.velocity += new Vector2(0, jumpSpeed);
        }

        /*
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(new Vector2(0, 0.1f));

        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(new Vector2(0, -0.1f));

        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(new Vector2(-0.1f,0));

        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector2(0.1f,0));

        }
        */
    }
}

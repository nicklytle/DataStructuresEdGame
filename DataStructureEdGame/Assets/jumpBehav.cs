using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jumpBehav : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown (KeyCode.Space))
		{
			//this.transform.Translate (Vector3.up * 2.5f);
			this.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector3.up * 350.0f);
		}
		
	}
}

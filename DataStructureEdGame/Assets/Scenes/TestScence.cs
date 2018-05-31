using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScence : MonoBehaviour {
    public Transform ground;
	// Use this for initialization
	void Start () {
        for(int j = 0; j < 16; j++)
        {
            for (int i = 0; i < 16; i++)
            {
                Instantiate(ground, new Vector2(i,j), Quaternion.identity);

            }

        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

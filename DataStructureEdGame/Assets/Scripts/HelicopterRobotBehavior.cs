using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterRobotBehavior : MonoBehaviour {

    public GameController gameController;
    public GameObject childLink;

	void Start () {
        childLink = transform.Find("LinkBlock").gameObject;
        childLink.GetComponent<LinkBlockBehavior>().isHelicopterLink = true;
        childLink.GetComponent<LinkBlockBehavior>().gameController = gameController;
    }

    // Update is called once per frame
    void Update() {
        if (gameController.playerRef != null) { 
        //    transform.position = gameController.playerRef.Find("HelicopterPosition").transform.position;
        }   
    }
}

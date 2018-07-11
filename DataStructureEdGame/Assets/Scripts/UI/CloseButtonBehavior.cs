using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseButtonBehavior : MonoBehaviour {

	// Use this for initialization
	void Start () { 
        GetComponent<Button>().onClick.AddListener(onCloseButtonPressed);
    }
	
    void onCloseButtonPressed()
    {
        transform.parent.gameObject.SetActive(false); 
    }
}

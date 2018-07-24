using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * A close button for closing instruction panels, but it can be used in any panel. 
 */ 
public class CloseButtonBehavior : MonoBehaviour {

	void Start () { 
        GetComponent<Button>().onClick.AddListener(onCloseButtonPressed);
    }
	
    void onCloseButtonPressed()
    {
        transform.parent.gameObject.SetActive(false); 
    }
}

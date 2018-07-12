using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseButtonBehavior : MonoBehaviour {

	void Start () { 
        GetComponent<Button>().onClick.AddListener(onCloseButtonPressed);
    }
	
    void onCloseButtonPressed()
    {
        transform.parent.gameObject.SetActive(false); 
    }
}

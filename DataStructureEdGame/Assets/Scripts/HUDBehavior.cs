using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBehavior : MonoBehaviour {

    public GameController gameController;
    public Button resetButton;

    private float debounce;

    void Start()
    {
        debounce = 0;
        resetButton.onClick.AddListener(OnResetButtonClick);
    }

    void Update()
    {
        debounce += Time.fixedDeltaTime;
    }

    public void OnResetButtonClick()
    {
        //Debug.Log("Reset Level");
        //Debug.Log(debounce);
        if (debounce > 1.0f) { 
            gameController.worldGenerator.resetLevel();
            debounce = 0; 
        }
    }
}

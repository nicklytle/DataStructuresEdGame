using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBehavior : MonoBehaviour {

    public GameController gameController;
    public Button resetButton;

    // public Text controlSchemeView;
    public Button addPlatformButton;
    public PlatformBehavior oneToAdd;
     
    private float debounce; 
    public bool selected;

    void Start()
    {
        selected = false;
        debounce = 0;
        resetButton.onClick.AddListener(OnResetButtonClick);  

        addPlatformButton.onClick.AddListener(OnControlAddPlatform);

    }

    void Update()
    {
        debounce += Time.fixedDeltaTime; 
    }

    public void OnResetButtonClick()
    { 
        if (debounce > 1.0f) { 
            gameController.worldGenerator.resetLevel();
            debounce = 0; 
        }
    }
    
    void OnControlAddPlatform()
    {
        gameController.addingPlatforms = true;

    }
}

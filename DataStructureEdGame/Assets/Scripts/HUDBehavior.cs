using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBehavior : MonoBehaviour {

    public GameController gameController;
    public Button resetButton;
    public Button changeControlButton;

    private float debounce;

    void Start()
    {
        debounce = 0;
        resetButton.onClick.AddListener(OnResetButtonClick);
        changeControlButton.onClick.AddListener(OnControlButtonClick);
    }

    void Update()
    {
        debounce += Time.fixedDeltaTime;
    }

    public void OnControlButtonClick()
    {
        if (debounce > 1.0f)
        {
            if (gameController.debugLinkControlVersion == 0)
            {
                gameController.debugLinkControlVersion = 1;
            } else if (gameController.debugLinkControlVersion == 1)
            {
                gameController.debugLinkControlVersion = 0;
            }
            debounce = 0;
        }
    }

    public void OnResetButtonClick()
    {
        if (debounce > 1.0f) { 
            gameController.worldGenerator.resetLevel();
            debounce = 0; 
        }
    }
}

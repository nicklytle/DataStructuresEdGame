using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBehavior : MonoBehaviour {

    public GameController gameController;

    [Header("References to Important UI Elements")]
    public Button addPlatformButton;
    public Button resetButton;
    public Image addPlatformPanel;

    [Header("Internal variables")]
    public PlatformBehavior oneToAdd; 
    private float debounce; 
    public bool selected;
    private int countPlatformsToAdd;


    public LoggingManager currentPlayerLogs;

    void Start()
    {
        selected = false;
        debounce = 0;
        resetButton.onClick.AddListener(OnResetButtonClick);  

        addPlatformButton.onClick.AddListener(OnControlAddPlatform);
        countPlatformsToAdd = -1;

    }

    void Update()
    {
        debounce += Time.fixedDeltaTime; 
        if (gameController.platformsToAdd.Count != countPlatformsToAdd)
        {
            countPlatformsToAdd = gameController.platformsToAdd.Count; 
            addPlatformPanel.gameObject.SetActive(countPlatformsToAdd != 0); // only activate if there are platforms to add
        }
        if (debounce > 1.0f && Input.GetKeyDown(KeyCode.P))
        {
            OnResetButtonClick();
            debounce = 0;
        }
    }

    public void OnResetButtonClick()
    { 
        if (debounce > 1.0f) {

            string actMsg = "Level was reset";
            currentPlayerLogs.send_To_Server(actMsg);

            gameController.worldGenerator.resetLevel();
            debounce = 0; 
        }
    }
    
    void OnControlAddPlatform()
    {
        gameController.addingPlatforms = true;

    }
}

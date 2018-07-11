using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBehavior : MonoBehaviour {

    public GameController gameController;
    public Button resetButton;

    // public Text controlSchemeView;
    public Button addPlatformButton;
    public Image addPlatformPanel;
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
    }

    public void OnResetButtonClick()
    { 
        if (debounce > 1.0f) {

            string timestampRST = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string actMsg = "Level was reset";
            currentPlayerLogs.send_To_Server(actMsg, timestampRST);

            gameController.worldGenerator.resetLevel();
            debounce = 0; 
        }
    }
    
    void OnControlAddPlatform()
    {
        gameController.addingPlatforms = true;

    }
}

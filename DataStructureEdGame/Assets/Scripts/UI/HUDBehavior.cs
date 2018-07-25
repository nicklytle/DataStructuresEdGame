using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * The main class for the game canvas which holds all important game related HUD interfaces.
 */
public class HUDBehavior : MonoBehaviour {

    private GameController gameController;
    private LoggingManager loggingManager;

    private Button addPlatformButton;
    private Button resetButton;
    private Image addPlatformPanel;

    private RectTransform objectivePanel;
    private Text objectiveText;
    private Text levelOnText;
    private Text addPlatformButtonTextUI;

    private float debounce;
    
    void Start()
    {
        // get internal references
        addPlatformPanel = transform.Find("HUDPanelAddPlatform").GetComponent<Image>();
        addPlatformButton = addPlatformPanel.transform.Find("AddPlatformButton").GetComponent<Button>();
        resetButton = transform.Find("HUDPanelReset/ResetButton").GetComponent<Button>();
        ensureReferences();

        resetButton.onClick.AddListener(OnResetButtonClick);  
        addPlatformButton.onClick.AddListener(OnControlAddPlatform);
        debounce = 0;
    }

    void Update()
    {
        debounce += Time.fixedDeltaTime; 
        if (gameController.platformsToAdd.Count == 0 && addPlatformPanel.enabled)
        {
            addPlatformPanel.gameObject.SetActive(false); 
        } else if (gameController.platformsToAdd.Count != 0 && !addPlatformPanel.enabled)
        {
            addPlatformPanel.gameObject.SetActive(true);
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
            loggingManager.sendLogToServer(actMsg);

            gameController.worldGenerator.resetLevel();
            debounce = 0; 
        }
    }
    
    void OnControlAddPlatform()
    {
        gameController.addingPlatforms = true;
    }

    public void setObjectiveHUD(string txt, bool show, bool isWinSatisfied)
    {
        ensureReferences();
        if (show)
        {
            Color col = isWinSatisfied ? new Color(0, 1, 0, (160.0f / 255.0f)) : new Color(1, 0.02f, 0.02f, (160.0f / 255.0f));
            objectivePanel.gameObject.SetActive(true);
            objectiveText.text = txt;
        } else
        {
            objectivePanel.gameObject.SetActive(false);
        }
    }

    public void setLevelOnText(int levelOn)
    {
        ensureReferences();
        levelOnText.text = "Level " + levelOn;
    }

    public void setPlatformsToAddText(int count)
    {
        ensureReferences(); 
        if (count > 0)
        {
            addPlatformPanel.gameObject.SetActive(true);
            addPlatformButtonTextUI.text = "Add Platform (" + count  + ")";
        } else
        {
            addPlatformPanel.gameObject.SetActive(false); 
        }
    }

    
    private void ensureReferences()
    {
        if (objectivePanel == null)
        {
            objectivePanel = transform.Find("HUDPanelObjective").GetComponent<RectTransform>();
        }
        if (objectiveText == null)
        {
            objectiveText = transform.Find("HUDPanelObjective/ObjectiveText").GetComponent<Text>();
        }
        if (levelOnText == null)
        {
            levelOnText = transform.Find("HUDLevelOn/LevelOnText").GetComponent<Text>();
        }
        if (addPlatformButtonTextUI == null)
        {
            addPlatformButtonTextUI = transform.Find("HUDPanelAddPlatform/AddPlatformButton/AddPlatformButtonText").GetComponent<Text>();
        }
    }

    public void setGameController(GameController gc)
    {
        gameController = gc;
    }

    public void setLoggingManager(LoggingManager lm)
    {
        loggingManager = lm;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBehavior : MonoBehaviour {

    public GameController gameController;
    public Button resetButton;

    public Text controlSchemeView;
    public Button addPlatformButton;
    public PlatformBehavior oneToAdd;


    private float debounce;
    private int versionNumber; // local copy

    // DEBUG/TEST OPTION STUFF
    public Image controlSelectPanel;
    public Button controlOneButton;
    public Button controlTwoButton;
    public TextAsset[] controlOneLevelFiles;
    public TextAsset[] controlTwoLevelFiles;
    public bool selected;

    void Start()
    {
        selected = false;
        debounce = 0;
        resetButton.onClick.AddListener(OnResetButtonClick);
        versionNumber = -1;

        controlOneButton.onClick.AddListener(OnControlOptionOne);
        controlTwoButton.onClick.AddListener(OnControlOptionTwo);

        addPlatformButton.onClick.AddListener(OnControlAddPlatform);

    }

    void Update()
    {
        debounce += Time.fixedDeltaTime;
        // update the GUI when it needs to be updated.
        if (versionNumber != gameController.debugLinkControlVersion) {
            versionNumber = gameController.debugLinkControlVersion;
            string ver = "1";
            if (versionNumber == 1)
            {
                ver = "2";
            }
            controlSchemeView.text = "Control v" + ver;
        }
    }

    public void OnResetButtonClick()
    {
        if (debounce > 1.0f) { 
            gameController.worldGenerator.resetLevel();
            debounce = 0; 
        }
    }
    

    // DEBUG/TESTING STUFF ONLY!
    void OnControlOptionOne()
    {
        if (selected)
            return;
        selected = true;
        gameController.worldGenerator.levelDescriptionJsonFiles = controlOneLevelFiles;
        gameController.debugLinkControlVersion = 0;
        controlSelectPanel.gameObject.SetActive(false);
        gameController.worldGenerator.ManualStartGenerator();
    }

    void OnControlOptionTwo()
    {
        if (selected)
            return;
        selected = true;
        gameController.worldGenerator.levelDescriptionJsonFiles = controlTwoLevelFiles;
        gameController.debugLinkControlVersion = 1;
        controlSelectPanel.gameObject.SetActive(false);
        gameController.worldGenerator.ManualStartGenerator();

    }

    void OnControlAddPlatform()
    {
        gameController.addingPlatforms = true;

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * The panel for the initial login panel.
 */
public class LoginPanelBehavior : MonoBehaviour {

    // for debugging
    private Button debugSkipLoginButton;

    public LoggingManager loggingManager;
    public GameController gameController;

    [Header("Canvas References")]
    public Canvas gameCanvas;
    public Canvas menuCanvas;
    
    private Text statusText;
    private Button submitButton;
    private InputField playerIdInputField;
    private InputField passwordInputField;
    private bool submitPressed; // flag for if the submit button has been pressed 

	// Use this for initialization
	void Start () {
        debugSkipLoginButton = transform.Find("DebugSkipButton").GetComponent<Button>();
        statusText = transform.Find("StatusText").GetComponent<Text>();
        submitButton = transform.Find("SubmitButton").GetComponent<Button>();
        playerIdInputField = transform.Find("PlayerIDInputField").GetComponent<InputField>();
        passwordInputField = transform.Find("PasswordInputField").GetComponent<InputField>();

        // for debugging only.
        debugSkipLoginButton.onClick.AddListener(skipLoginShortcut);

        submitButton.onClick.AddListener(onSubmitButtonPressed);
        playerIdInputField.ActivateInputField();
        passwordInputField.ActivateInputField();
        submitPressed = false;
    }

    void Update()
    {
        // if there was a response sent out, wait for the response to come back.
        if (submitPressed)
        {
            if (loggingManager.getLoginAttemptResponse().Length > 0)
            {
                // process the response.
                if (loggingManager.getLoginAttemptResponse().StartsWith("success"))
                {
                    string[] tokens = loggingManager.getLoginAttemptResponse().Split(' ');

                    gameCanvas.gameObject.SetActive(true);
                    menuCanvas.gameObject.SetActive(false);
                    int playerId = System.Convert.ToInt32(tokens[1]);
                    int startingLevelIndex = System.Convert.ToInt32(tokens[2]);
                    gameController.loggingManager.currentPlayerID = playerId;
                    gameController.worldGenerator.levelFileIndex = startingLevelIndex;
                    // populate the previous instruction panel based on the level you're in.
                    gameController.instructionScreenBehavior.revealPlatformsForLevels(startingLevelIndex); 
                    gameController.worldGenerator.ManualStartGenerator();
                }
                else if (loggingManager.getLoginAttemptResponse().Equals("fail"))
                {
                    statusText.text = "Login failed. Please try again.";
                }
                else
                {
                    statusText.text = "Database error. Please notify an instructor.";
                }

                submitPressed = false;
            }
        }
    }


    void skipLoginShortcut()
    {
        gameCanvas.gameObject.SetActive(true);
        menuCanvas.gameObject.SetActive(false);
        gameController.loggingManager.currentPlayerID = -1;
        gameController.instructionScreenBehavior.ensureInstructionPanelReferences();
        gameController.instructionScreenBehavior.revealPlatformsForLevels(gameController.worldGenerator.levelFileIndex);
        loggingManager.sendLogToServer("Logged in with debug login");
        gameController.worldGenerator.ManualStartGenerator();
    }

    void onSubmitButtonPressed()
    {
        if (!submitPressed) {
            submitPressed = true; 
            string playerId = playerIdInputField.text;
            string pw = passwordInputField.text;
            if (playerId.Length > 0 && pw.Length > 0) { 
                loggingManager.beginAttemptLogin(playerId, pw);  // there may be a delay since it is making a request to the server.
            }
            else
            {
                statusText.text = "Please provide a Player ID and Password";
                submitPressed = false;
            }
        }
    }
}

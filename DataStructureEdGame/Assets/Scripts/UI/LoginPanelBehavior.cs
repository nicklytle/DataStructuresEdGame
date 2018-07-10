using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanelBehavior : MonoBehaviour {

    // TEMPORARY debug login button
    public Button debugSkipLoginButton;

    public LoggingManager loggingManager;

    // need references to the canvases for transitions
    public Canvas gameCanvas;
    public Canvas menuCanvas;
    public GameController gameController;

    public Text statusText;
    public Button submitButton;
    public InputField playerIdInputField;
    public InputField passwordInputField;
    private bool submitPressed;

	// Use this for initialization
	void Start () {
        submitPressed = false;
        submitButton.onClick.AddListener(onSubmitButtonPressed);

        // TEMPORARY
        debugSkipLoginButton.onClick.AddListener(skipLoginShortcut);

        playerIdInputField.ActivateInputField();
        passwordInputField.ActivateInputField();
    }

    void skipLoginShortcut()
    {
        Debug.Log("Skipping log in");
        gameCanvas.gameObject.SetActive(true);
        menuCanvas.gameObject.SetActive(false);
        gameController.currentPlayerID = -1;
        gameController.worldGenerator.ManualStartGenerator(); 
    }

    void Update()
    {
        // if there was a response sent out, wait for the response to come back.
        if (submitPressed)
        {
            if (loggingManager.loginAttemptResponse.Length > 0)
            {
                // process the response.
                Debug.Log(loggingManager.loginAttemptResponse);
                if (loggingManager.loginAttemptResponse.StartsWith("success"))
                {
                    string playerIdExtracted = loggingManager.loginAttemptResponse.Substring(loggingManager.loginAttemptResponse.IndexOf(' ') + 1);

                    Debug.Log("Login successful! " + playerIdExtracted);
                    gameCanvas.gameObject.SetActive(true);
                    menuCanvas.gameObject.SetActive(false);
                    gameController.currentPlayerID = System.Convert.ToInt32(playerIdExtracted);
                    gameController.worldGenerator.ManualStartGenerator();
                }
                else if (loggingManager.loginAttemptResponse.Equals("fail"))
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

    void onSubmitButtonPressed()
    {
        if (!submitPressed) {
            submitPressed = true;
            Debug.Log("Submit");
            string playerId = playerIdInputField.text;
            string pw = passwordInputField.text;
            if (playerId.Length > 0 && pw.Length > 0) { 
                loggingManager.beginAttemptLogin(playerId, pw); 
            }
            else
            {
                statusText.text = "Please provide a Player ID and Password"; 
            }
        }
    }
}

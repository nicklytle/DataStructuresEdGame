using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanelBehavior : MonoBehaviour {

    public LoggingManager loggingManager;

    // need references to the canvases for transitions
    public Canvas gameCanvas;
    public Canvas menuCanvas;
    public GameController gameController;

    public Text statusText;
    public Button submitButton;
    public InputField playerIdInputField;
    public InputField passwordInputField;
    private bool debounce;

	// Use this for initialization
	void Start () {
        debounce = false;
        submitButton.onClick.AddListener(onSubmitButtonPressed);
    }
	

    void onSubmitButtonPressed()
    {
        if (!debounce) {
            debounce = true;
            Debug.Log("Submit");
            string playerId = playerIdInputField.text;
            string pw = passwordInputField.text;
            if (playerId.Length > 0 && pw.Length > 0) { 
                string loginAttemptResponse = loggingManager.attempt_Login(playerId, pw);
                statusText.text = "";
                if (loginAttemptResponse.Equals("success"))
                {
                    Debug.Log("Login successful!");
                    gameCanvas.gameObject.SetActive(true);
                    menuCanvas.gameObject.SetActive(false);
                    gameController.currentPlayerID = System.Convert.ToInt32(playerId);
                    gameController.worldGenerator.ManualStartGenerator();
                }
                else if (loginAttemptResponse.Equals("fail"))
                {
                    statusText.text = "Login failed. Please try again."; 
                }
                else
                {
                    statusText.text = "Database error. Please notify an instructor.";
                }
            }
            else
            {
                statusText.text = "Please provide a Player ID and Password"; 
            }
            debounce = false;
        }
    }
}

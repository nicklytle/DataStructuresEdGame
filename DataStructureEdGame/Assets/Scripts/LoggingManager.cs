using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoggingManager : MonoBehaviour
{

    public GameController gameRef;

    public bool enableLogging;
    public string loginAttemptResponse;  // store the response from the login attempt.


    private IEnumerator sendLogToServer(string actionMsg, string timestamp)
    {
        string logUrl = "http://localhost/test/sendingDataToPHP.php";
        WWWForm logForm = new WWWForm();
        logForm.AddField("playerID", gameRef.currentPlayerID);
        logForm.AddField("levelFile", gameRef.worldGenerator.levelDescriptionJsonFiles[gameRef.worldGenerator.levelFileIndex].name);
        logForm.AddField("actionMsg", actionMsg);
        logForm.AddField("timestamp", timestamp);

        using (UnityWebRequest www = UnityWebRequest.Post(logUrl, logForm))
        {
            yield return www.Send();
            if (www.isError)
            {
                Debug.Log("Error with sending the log message");
            } 
        }
    }


    public void send_To_Server(string actionMsg, string timestamp)
    {
        if (enableLogging) {
            StartCoroutine(sendLogToServer(actionMsg, timestamp));
        }
    }


    private IEnumerator attemptLogin(string playerId, string pw)
    {
        string loginUrl = "http://localhost/test/loginAuthentication.php";
        WWWForm loginForm = new WWWForm();
        loginForm.AddField("playerID", playerId);
        loginForm.AddField("pw", pw);

        using (UnityWebRequest www = UnityWebRequest.Post(loginUrl, loginForm))
        {
            yield return www.Send();
            if (!www.isError)
            {
                loginAttemptResponse = www.downloadHandler.text;
            }

        }
    }

    public void beginAttemptLogin(string playerId, string pw) {
        loginAttemptResponse = ""; 
        StartCoroutine(attemptLogin(playerId, pw));
    }

}

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.WorldGeneration;

public class LoggingManager : MonoBehaviour
{

    public GameController gameRef;
    public int currentPlayerID;
    public bool enableLogging;
    public string loginAttemptResponse;  // store the response from the login attempt.
    private string worldStateField;

    private IEnumerator sendLogToServer(string actionMsg, string timestamp)
    {
        string logUrl = "http://localhost/test/sendingDataToPHP.php";
        WWWForm logForm = new WWWForm();
        logForm.AddField("playerID", currentPlayerID);
        string levelFileName = "NO LEVEL";
        if (gameRef.worldGenerator.levelFileIndex < gameRef.worldGenerator.levelDescriptionJsonFiles.Length)
        {
            levelFileName = gameRef.worldGenerator.levelDescriptionJsonFiles[gameRef.worldGenerator.levelFileIndex].name;
        }
        logForm.AddField("levelFile", levelFileName);
        logForm.AddField("actionMsg", actionMsg);
        logForm.AddField("timestamp", timestamp);
        logForm.AddField("worldState", "cattttt");

        using (UnityWebRequest www = UnityWebRequest.Post(logUrl, logForm))
        {
            yield return www.Send();
            if (www.isError)
            {
                Debug.Log("Error with sending the log message");
            } 
        }
    }


    public void send_To_Server(string actionMsg)
    {
        List<Block> blockList;
        List<LinkBlock> linkyList;
        List<SingleLinkedListPlatform> singleLLlist;

        linkyList = new List<LinkBlock>();
        blockList = new List<Block>();
        singleLLlist = new List<SingleLinkedListPlatform>();
        foreach (Transform t in gameRef.worldGenerator.levelEntities)
        {
            if (t.GetComponent<LinkBlockBehavior>() != null)
            {
                //then this is of type LinkedBlock

                LinkBlock newB = new LinkBlock();
                newB.x = t.position.x;
                newB.y = t.position.y;
                newB.type = "linkBlock";
                newB.logId = t.GetComponent<LinkBlockBehavior>().logId;
                if(t.GetComponent<LinkBlockBehavior>().connectingPlatform != null)
                {
                    newB.objIDConnectingTo = t.GetComponent<LinkBlockBehavior>().connectingPlatform.logId;
                }
                else
                {
                    newB.objIDConnectingTo = "";
                }
                linkyList.Add(newB);
            }

            else if (t.GetComponent<GroundBehavior>() != null)
            {
                Block groundB = new Block();
                groundB.x = t.position.x;
                groundB.y = t.position.y;
                groundB.type = "ground";
                groundB.logId = t.GetComponent<GroundBehavior>().logId;
                blockList.Add(groundB);

            }
            else if(t.GetComponent<PlatformBehavior>() != null)
            {
                SingleLinkedListPlatform platB = new SingleLinkedListPlatform();
                platB.x = t.position.x;
                platB.y = t.position.y;
                platB.type = "LLplatform";
                platB.logId = t.GetComponent<PlatformBehavior>().logId;
                platB.objId = t.GetComponent<PlatformBehavior>().logId;
                platB.childLinkBlockConnectId = t.GetComponent<PlatformBehavior>().childLink.GetComponent<LinkBlockBehavior>().logId;
                platB.value = t.GetComponent<PlatformBehavior>().getValue();
                //platB.toAdd = t.GetComponent<PlatformBehavior>().toadd;
                platB.toAdd = false;
                singleLLlist.Add(platB);
                Debug.Log(singleLLlist.Count);
                //Platform Behavior has fields to tell if the platform isHidden, isSolid. Include?
            }
        }

        LogMsgRepresentation current = new LogMsgRepresentation();
        current.blockPart = blockList.ToArray();
        current.linkBlockPart = linkyList.ToArray();
        current.platformPart = singleLLlist.ToArray();
        worldStateField = current.SaveString();
        //Debug.Log(current.SaveString());

        //it should be a list of all these types of 

        if (enableLogging) { 
            String timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
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

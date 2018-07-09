using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggingManager : MonoBehaviour
{

    public GameController gameRef;

    public bool disableLogging;

    // Use this for initialization
    public void send_To_Server(string actionMsg, string timestamp)
    {
        if (disableLogging) {
            //to send data/submit a request to the server
            WebRequest request = WebRequest.Create("http://localhost/test/sendingDataToPHP.php"); //put in address of the PHP script
            request.Method = "POST";
            //string dataToPost = "logID=" + logID + "&playerID=" + playerId + "&levelFile=" + levelFile + "&actionMsg=" + actionMsg + "&timestamp=" + timestamp;
            string dataToPost = "playerID=" + gameRef.currentPlayerID + "&levelFile=" + gameRef.worldGenerator.levelDescriptionJsonFiles[gameRef.worldGenerator.levelFileIndex].name + "&actionMsg=" + actionMsg + "&timestamp=" + timestamp;
            Debug.Log("data to post: " + dataToPost);
            byte[] byteArray = Encoding.UTF8.GetBytes(dataToPost);

            //not sure what to put here
            request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            Debug.Log(responseFromServer);

            reader.Close();
            dataStream.Close();
            response.Close();

            /*
            //to receive data get a response from the server
            WebRequest request2 = WebRequest.Create("http://localhost/test/sendingDataToPHP.php");
            WebResponse response2 = request2.GetResponse();
            Stream dataStream2 = response2.GetResponseStream();
            StreamReader reader2 = new StreamReader(dataStream2);
            string responseFromServer2 = reader2.ReadToEnd();
            Debug.Log(responseFromServer2);
            reader2.Close();
            response2.Close();
            */
        }
    }


    public string attempt_Login(string playerId, string pw) {
        WebRequest request = WebRequest.Create("http://localhost/test/loginAuthentication.php"); 
        request.Method = "POST";
        string dataToPost = "playerID=" + playerId + "&pw=" + pw;
        byte[] byteArray = Encoding.UTF8.GetBytes(dataToPost);
        
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = byteArray.Length;

        // write the request data
        Stream dataStream = request.GetRequestStream();
        dataStream.Write(byteArray, 0, byteArray.Length);
        dataStream.Close();

        WebResponse response = request.GetResponse();
        dataStream = response.GetResponseStream();
        StreamReader reader = new StreamReader(dataStream);
        string responseFromServer = reader.ReadToEnd();
        Debug.Log("Response: " + responseFromServer);

        // close all of the connections
        reader.Close();
        dataStream.Close();
        response.Close();

        // return if it was a success or not.
        return responseFromServer;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionScreensBehavior : MonoBehaviour {

    // this class is used to keep a global reference of instruction screens

    public RectTransform moveInstructions;
    public RectTransform platformStateInstructions;
    public RectTransform platformHiddenRevealInstructions;
    public RectTransform addPlatformInstructions;
    public RectTransform deleteInstructions;
    public RectTransform createInstructions;
    public RectTransform goalInstructions;
    public RectTransform robotInstructions;
    public RectTransform platformAnalogyInstructions;
    public RectTransform generatedCodeInstruction;

    private void changeScreen(string key, bool b)
    {
        if (key.Equals("MoveInstructions"))
        {
            moveInstructions.gameObject.SetActive(b);
        }
        else if (key.Equals("PlatformStatesInstructions"))
        {
            platformStateInstructions.gameObject.SetActive(b);
        }
        else if (key.Equals("PlatformHiddenRevealInstructions"))
        {
            platformHiddenRevealInstructions.gameObject.SetActive(b);
        }
        else if (key.Equals("AddPlatformInstructions"))
        {
            addPlatformInstructions.gameObject.SetActive(b);
        }
        else if(key.Equals("DeleteInstructions"))
        {
            deleteInstructions.gameObject.SetActive(b);
        }
        else if (key.Equals("CreateInstructions"))
        {
            createInstructions.gameObject.SetActive(b);
        }
        else if (key.Equals("GoalInstructions"))
        {
            goalInstructions.gameObject.SetActive(b);
        }
        else if(key.Equals("HelicopterRobotInstructions"))
        {
            robotInstructions.gameObject.SetActive(b);
        }
        else if (key.Equals("PlatformAnalogyInstructions"))
        {
            platformAnalogyInstructions.gameObject.SetActive(b);
        }
        else if (key.Equals("GeneratedCodeInstruction"))
        {
            generatedCodeInstruction.gameObject.SetActive(b);
        }
    }
    
    public void showScreen(string key)
    {
        changeScreen(key, true);
    }

    public void hideScreen(string key)
    {
        changeScreen(key, false);
    }
}

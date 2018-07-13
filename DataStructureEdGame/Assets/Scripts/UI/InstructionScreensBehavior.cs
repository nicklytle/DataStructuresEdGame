using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionScreensBehavior : MonoBehaviour {

    public PreviousInstructionsPanelBehavior previousPanelBehavior;

    // this class is used to keep a global reference of instruction screens
    [Header("Instruction UI Panels")]
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
            if (b)
                previousPanelBehavior.addInstructionPanelToHistory(moveInstructions);
        }
        else if (key.Equals("PlatformStatesInstructions"))
        {
            platformStateInstructions.gameObject.SetActive(b);
            if (b)
                previousPanelBehavior.addInstructionPanelToHistory(platformStateInstructions);
        }
        else if (key.Equals("PlatformHiddenRevealInstructions"))
        {
            platformHiddenRevealInstructions.gameObject.SetActive(b);
            if (b)
                previousPanelBehavior.addInstructionPanelToHistory(platformHiddenRevealInstructions);
        }
        else if (key.Equals("AddPlatformInstructions"))
        {
            addPlatformInstructions.gameObject.SetActive(b);
            if (b)
                previousPanelBehavior.addInstructionPanelToHistory(addPlatformInstructions);
        }
        else if(key.Equals("DeleteInstructions"))
        {
            deleteInstructions.gameObject.SetActive(b);
            if (b)
                previousPanelBehavior.addInstructionPanelToHistory(deleteInstructions);
        }
        else if (key.Equals("CreateInstructions"))
        {
            createInstructions.gameObject.SetActive(b);
            if (b)
                previousPanelBehavior.addInstructionPanelToHistory(createInstructions);
        }
        else if (key.Equals("GoalInstructions"))
        {
            goalInstructions.gameObject.SetActive(b);
            if (b)
                previousPanelBehavior.addInstructionPanelToHistory(goalInstructions);
        }
        else if(key.Equals("HelicopterRobotInstructions"))
        {
            robotInstructions.gameObject.SetActive(b);
            if (b)
                previousPanelBehavior.addInstructionPanelToHistory(robotInstructions);
        }
        else if (key.Equals("PlatformAnalogyInstructions"))
        {
            platformAnalogyInstructions.gameObject.SetActive(b);
            if (b)
                previousPanelBehavior.addInstructionPanelToHistory(platformAnalogyInstructions);
        }
        else if (key.Equals("GeneratedCodeInstruction"))
        {
            generatedCodeInstruction.gameObject.SetActive(b);
            if (b)
                previousPanelBehavior.addInstructionPanelToHistory(generatedCodeInstruction);
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

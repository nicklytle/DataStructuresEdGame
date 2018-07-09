using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodePanelBehavior : MonoBehaviour {

    public Text codeTextUI;
    public Scrollbar scrollBar;
    private int lines;
    private float ySize;

    void Start()
    {
        ySize = 90; // codeTextUI.rectTransform.sizeDelta.y;
    }

    public void appendCodeText(string line)
    {
        if (codeTextUI.text.Length > 0)
        {
            codeTextUI.text = codeTextUI.text + "\n" + line;
        }
        else
        {
            codeTextUI.text = line;
        }
        lines++;
        // expand that window when u need it unfortunately this has to be hard-coded.
        if (lines > 5)
        {
            // increase the size of the text box
            ySize += 18; // expand by 18 for each line (hardcoded for the font size.
            Debug.Log("ySize = " + ySize); 
            Debug.Log(codeTextUI.rectTransform.position);
            scrollBar.value = 0; // move to the bottom
            scrollBar.value = 0; // to verify that the position updated properly.
        }
        codeTextUI.rectTransform.sizeDelta = new Vector2(codeTextUI.rectTransform.sizeDelta.x, ySize);
    }

    public void clearCodeText()
    {
        codeTextUI.text = "";
        lines = 0;
        codeTextUI.rectTransform.gameObject.SetActive(true);
        codeTextUI.rectTransform.sizeDelta = new Vector2(codeTextUI.rectTransform.sizeDelta.x, ySize); // reset the size
        Debug.Log("Cleared code text");
    }
}

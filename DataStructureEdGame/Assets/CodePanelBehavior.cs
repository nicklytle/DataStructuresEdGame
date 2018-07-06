using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CodePanelBehavior : MonoBehaviour {

    public Text codeTextUI;
    public Scrollbar scrollBar;
    private int lines;
    private float initialYSize;

    void Start()
    {
        initialYSize = codeTextUI.rectTransform.sizeDelta.y;
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
            codeTextUI.rectTransform.sizeDelta = new Vector2(codeTextUI.rectTransform.sizeDelta.x, 
                codeTextUI.rectTransform.sizeDelta.y + 18); // expand by 18 for each line (hardcoded for the font size.
            Debug.Log(codeTextUI.rectTransform.position);
            scrollBar.value = 0; // move to the bottom
            scrollBar.value = 0; // to verify that the position updated properly.
        }
    }

    public void clearCodeText()
    {
        codeTextUI.text = "";
        lines = 0;
        codeTextUI.rectTransform.sizeDelta = new Vector2(codeTextUI.rectTransform.sizeDelta.x, initialYSize); // reset the size (hard coded)
    }
}

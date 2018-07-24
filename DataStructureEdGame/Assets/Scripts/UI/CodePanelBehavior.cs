using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * The panel which displays code which was generated. When the 
 * code is longer than 5 lines, the generated code box can be scrolled.
 */ 
public class CodePanelBehavior : MonoBehaviour {

    private Text codeTextUI;
    private Scrollbar scrollBar;
    private int lines;
    private float ySize;

    void Start()
    {
        scrollBar = transform.Find("Scrollbar").GetComponent<Scrollbar>();
        ensureReferences();
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
        ensureReferences();
        codeTextUI.text = "";
        lines = 0;
        codeTextUI.rectTransform.gameObject.SetActive(true);
        codeTextUI.rectTransform.sizeDelta = new Vector2(codeTextUI.rectTransform.sizeDelta.x, ySize); // reset the size
        //Debug.Log("Cleared code text");
    }

    private void ensureReferences()
    {
        if (codeTextUI == null)
        {
            codeTextUI = transform.Find("ScrollView/ViewportContainer/CodeText").GetComponent<Text>();
            // make sure to record the correct initial size.
            ySize = transform.Find("ScrollView").GetComponent<RectTransform>().sizeDelta.y;
        }
    }
}

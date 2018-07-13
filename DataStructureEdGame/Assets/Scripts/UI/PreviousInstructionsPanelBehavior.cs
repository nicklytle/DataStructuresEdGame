using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviousInstructionsPanelBehavior : MonoBehaviour {

    private List<RectTransform> previousInstructions;
    public RectTransform containerPanel;
    public Button toggleButton;

    private Vector2 previousPosition; // where the button that was last added is located.
    public RectTransform viewInstructionButtonPrefab;
    private bool showingPanel;

	void Start ()
    {
        toggleButton.onClick.AddListener(TogglePanelView);
    }

    public void addInstructionPanelToHistory(RectTransform panel)
    {
        if (previousInstructions == null)
        {
            previousPosition = new Vector2(5, -5);
            showingPanel = false;
            previousInstructions = new List<RectTransform>();
        }

        if (!previousInstructions.Contains(panel))
        {
            Debug.Log("Adding panel to history");
            previousInstructions.Add(panel); // add to the end of the list.

            // make a panel and insert it
            RectTransform newButton = Instantiate<RectTransform>(viewInstructionButtonPrefab, new Vector3(), Quaternion.identity);
            newButton.gameObject.SetActive(true);
            newButton.SetParent(containerPanel); // add to the container.
            newButton.anchoredPosition = previousPosition; 
            newButton.GetComponent<ViewInstructionPanelButton>().panelToReveal = panel;
            newButton.GetComponent<ViewInstructionPanelButton>().previousPanel = this;
            // make the button text equal to the title inside of that instruction panel.
            Text textComp = newButton.Find("Text").GetComponent<Text>();
            textComp.text = panel.Find("TitleText").GetComponent<Text>().text;
            previousPosition += new Vector2(0, -30);
        }
    }

    void TogglePanelView()
    {
        if (showingPanel)
        {
            hideContainer();
        } else
        {
            showContainer();
        }
    }

    public void hideContainer()
    {
        showingPanel = false;
        containerPanel.gameObject.SetActive(false);
    }

    public void showContainer()
    {
        showingPanel = true;
        containerPanel.gameObject.SetActive(true);
    }
}

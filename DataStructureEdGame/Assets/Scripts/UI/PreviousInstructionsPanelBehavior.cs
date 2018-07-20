using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviousInstructionsPanelBehavior : MonoBehaviour {

    private List<RectTransform> previousInstructions;
    public RectTransform containerPanel;
    public Button toggleButton;

    //private Vector2 previousPosition; // where the button that was last added is located.
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
            showingPanel = false;
            previousInstructions = new List<RectTransform>();
        }
        if (!previousInstructions.Contains(panel))
        {
            // Debug.Log("Adding panel to history");
            previousInstructions.Add(panel); // add to the end of the list.

            // find the button in the container
            foreach (ViewInstructionPanelButton viewBtn in containerPanel.GetComponentsInChildren<ViewInstructionPanelButton>(true))
            {
                if (viewBtn.panelToReveal == panel) {
                    viewBtn.gameObject.SetActive(true);
                }
            }
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

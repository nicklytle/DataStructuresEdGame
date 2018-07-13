using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewInstructionPanelButton : MonoBehaviour {

    public PreviousInstructionsPanelBehavior previousPanel;
    public RectTransform panelToReveal;
    private Button btn;

	void Start () {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(RevealPanel);
	}
	
	void RevealPanel()
    {
        previousPanel.hideContainer();
        if (panelToReveal != null)
            panelToReveal.gameObject.SetActive(true);
    }
}

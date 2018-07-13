using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewInstructionPanelButton : MonoBehaviour {

    public RectTransform containerPanel;
    public RectTransform panelToReveal;
    private Button btn;

	void Start () {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(RevealPanel);
	}
	
	void RevealPanel()
    {
        containerPanel.gameObject.SetActive(false); // hide the containing panel
        if (panelToReveal != null)
            panelToReveal.gameObject.SetActive(true);
    }
}

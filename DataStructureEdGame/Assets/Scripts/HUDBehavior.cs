using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDBehavior : MonoBehaviour {

    public GameController gameController;
    public Button resetButton;
    public Text controlSchemeView;

    private float debounce;

    void Start()
    {
        debounce = 0;
        resetButton.onClick.AddListener(OnResetButtonClick);
        string ver = "1";
        if (gameController.debugLinkControlVersion == 1)
        {
            ver = "2";
        }
        controlSchemeView.text = "Control v" + ver;
    }

    void Update()
    {
        debounce += Time.fixedDeltaTime;
    }

    public void OnResetButtonClick()
    {
        if (debounce > 1.0f) { 
            gameController.worldGenerator.resetLevel();
            debounce = 0; 
        }
    }
}

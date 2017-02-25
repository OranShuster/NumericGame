using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Prime31.ZestKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject UserCodePanel;
    public InputField UserCodeInputField;

    private CanvasGroup _mainMenuGroup;
    private CanvasGroup _userCodeGroup;
	// Use this for initialization
	void Start ()
	{
	    _mainMenuGroup = MainMenuPanel.GetComponent<CanvasGroup>();
	    _userCodeGroup = UserCodePanel.GetComponent<CanvasGroup>();
	}

    // Update is called once per frame
    void Update () {
		
	}

    public void CodeSubmitButtonClick()
    {
        if (SendCodeToServer(UserCodeInputField.text))
        {
            HideUserInputElements();
            ShowMainMenu();
        }

    }

    private void ShowMainMenu()
    {
        _mainMenuGroup.ZKalphaTo(1f).start();
        _mainMenuGroup.interactable = true;
    }

    private void HideUserInputElements()
    {
        _userCodeGroup.ZKalphaTo(0f).start();
        _userCodeGroup.interactable = false;
    }

    private bool SendCodeToServer(string text)
    {
        return true;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void ShowPlayTimeStatistics()
    {
        
    }
}

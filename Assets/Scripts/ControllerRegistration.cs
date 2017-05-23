using System.Collections;
using Prime31.ZestKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerRegistration : MonoBehaviour
{
    public Text RegistrationHeader;
    public InputField RegistrationCodeInputField;
    public GameObject SubmitButton;
    public Text RegistrationCodeInputPlaceholderText;
    public Text RegistrationErrorText;
    public GameObject MenuButtonGameObject;

    public void CodeSubmitButtonClick()
    {
        var usercode = RegistrationCodeInputField.text;
        RegistrationErrorText.text = "";
        SubmitButton.GetComponentInChildren<Button>().interactable=false;
        SubmitButton.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("SentRequest");
        StartCoroutine(SendUserCode(usercode));
    }

    private IEnumerator SendUserCode(string usercode)
    {
        yield return new WaitForSeconds(0.2f);
        try
        {
            var userStatistics = new UserStatistics(usercode);
            if (userStatistics.UserLocalData != null)
            {
                SceneManager.LoadScene("MainMenu");
                yield break;
            }
            ShowRegistrationErrorMessage();
        }
        catch
        {
            ShowRegistrationErrorMessage();
            SubmitButton.gameObject.SetActive(true);
            SubmitButton.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("ConfirmText");
        }
    }

    private void ShowRegistrationErrorMessage()
    {
        RegistrationErrorText.text = Utilities.LoadStringFromFile("RegistrationErrorMessage",50);
    }

    void Awake()
    {
        if (UserLocalData.PlayerDataValid())
            SceneManager.LoadScene("MainMenu");
    }
    void Start()
    {
        ZestKit.enableBabysitter = true;
        ZestKit.removeAllTweensOnLevelLoad = true;
        Application.logMessageReceived += Utilities.LoggerCallback;
        RegistrationHeader.text = Utilities.LoadStringFromFile("UserRegistrationHeader");
        SubmitButton.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("ConfirmText");
        RegistrationCodeInputPlaceholderText.text = Utilities.LoadStringFromFile("RegistrationCodeInputPlaceholder");
        MenuButtonGameObject.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

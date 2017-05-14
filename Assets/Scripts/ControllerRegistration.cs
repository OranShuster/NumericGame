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

    public void CodeSubmitButtonClick()
    {
        var usercode = RegistrationCodeInputField.text;
        RegistrationErrorText.text = "";
        SubmitButton.gameObject.SetActive(false);
        SubmitButton.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("SentRequest");
        StartCoroutine(SendUserCode(usercode));
    }

    private IEnumerator SendUserCode(string usercode)
    {
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
        }
        finally
        {
            SubmitButton.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("ConfirmText");
            SubmitButton.gameObject.SetActive(true);
        }
    }

    private void ShowRegistrationErrorMessage()
    {
        RegistrationErrorText.text = Utilities.LoadStringFromFile("RegistrationErrorMessage",50);
    }

    void Awake()
    {
        if (UserStatistics.PlayerDataValid())
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }
    }
    void Start()
    {
        ZestKit.enableBabysitter = true;
        ZestKit.removeAllTweensOnLevelLoad = true;
        Application.logMessageReceived += Utilities.LoggerCallback;
        RegistrationHeader.text = Utilities.LoadStringFromFile("UserRegistrationHeader");
        SubmitButton.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("ConfirmText");
        RegistrationCodeInputPlaceholderText.text = Utilities.LoadStringFromFile("RegistrationCodeInputPlaceholder");

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

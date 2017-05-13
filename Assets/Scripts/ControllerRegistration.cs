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
        try
        {
            UserStatistics userStatistics = new UserStatistics(usercode);
            if (userStatistics.UserLocalData != null)
            {
                SceneManager.LoadScene("MainMenu");
                return;
            }
            ShowRegistrationErrorMessage();
        }
        catch
        {
            ShowRegistrationErrorMessage();
        }
        finally
        {
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

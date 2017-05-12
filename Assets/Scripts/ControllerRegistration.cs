using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerRegistration : MonoBehaviour
{
    public Text RegistrationHeader;
    public InputField RegistrationCodeInputField;
    public Text SubmitButtonText;
    public Text RegistrationCodeInputPlaceholderText;
    public Text RegistrationErrorText;

    public void CodeSubmitButtonClick()
    {
        var usercode = RegistrationCodeInputField.text;
        RegistrationErrorText.text = "";
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
    }

    private void ShowRegistrationErrorMessage()
    {
        RegistrationErrorText.text = Utilities.LoadStringFromFile("RegistrationErrorMessage",50);
    }

    void Start()
    {
        Application.logMessageReceived += Utilities.LoggerCallback;
        if (UserStatistics.PlayerDataValid())
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }
        RegistrationHeader.text = Utilities.LoadStringFromFile("UserRegistrationHeader");
        SubmitButtonText.text = Utilities.LoadStringFromFile("ConfirmText");
        RegistrationCodeInputPlaceholderText.text = Utilities.LoadStringFromFile("RegistrationCodeInputPlaceholder");

    }
}

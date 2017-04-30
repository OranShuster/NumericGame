using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerRegistration : MonoBehaviour
{
    public Text RegistrationHeader;
    public Text RegistrationCodeInputField;
    public Text SubmitButtonText;
    public Text RegistrationCodeInputPlaceholderText;
    public Text RegistrationErrorText;

    public void CodeSubmitButtonClick()
    {
        var usercode = RegistrationCodeInputField.text;
        RegistrationErrorText.text = "";
        try
        {
            UserStatistics _userStatistics = new UserStatistics(usercode);
            SceneManager.LoadScene("MainMenu");
        }
        catch
        {
            ShowRegistrationErrorMessage();
        }
    }

    private void ShowRegistrationErrorMessage()
    {
        RegistrationErrorText.text = Utilities.LoadStringFromFile("RegistrationErrorMessage");
    }

    // Use this for initialization
    void Start()
    {
        if (UserStatistics.PlayerDataExists())
        {
            SceneManager.LoadScene("MainMenu");
        }
        RegistrationHeader.text = Utilities.LoadStringFromFile("UserRegistrationHeader");
        SubmitButtonText.text = Utilities.LoadStringFromFile("ConfirmText");
        RegistrationCodeInputPlaceholderText.text = Utilities.LoadStringFromFile("RegistrationCodeInputPlaceholder");
    }
}

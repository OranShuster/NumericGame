using Prime31.ZestKit;
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
        SubmitButtonText.GetComponentInParent<Button>().interactable = false;
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
            SubmitButtonText.GetComponentInParent<Button>().interactable = true;
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
        SubmitButtonText.text = Utilities.LoadStringFromFile("ConfirmText");
        RegistrationCodeInputPlaceholderText.text = Utilities.LoadStringFromFile("RegistrationCodeInputPlaceholder");

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

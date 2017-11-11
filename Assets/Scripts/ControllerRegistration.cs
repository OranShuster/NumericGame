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
        StartCoroutine(SendUserCode(usercode));
    }

    private IEnumerator SendUserCode(string usercode)
    {
        SubmitButton.GetComponentInChildren<Button>().interactable = false;
        SubmitButton.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("SentRequest");
        yield return new WaitForSeconds(0.2f);
        try
        {
            GameMaster.UserInformation = new UserInformation(usercode);
            if (GameMaster.UserInformation.UserLocalData != null)
            {
                SceneManager.LoadScene("MainMenu");
                yield break;
            }
            ShowRegistrationErrorMessage();
            SubmitButton.GetComponentInChildren<Button>().interactable = true;
            SubmitButton.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("ConfirmText");
        }
        catch
        {
            ShowRegistrationErrorMessage();
            SubmitButton.GetComponentInChildren<Button>().interactable = true;
            SubmitButton.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("ConfirmText");
        }
    }

    private void ShowRegistrationErrorMessage()
    {
        RegistrationErrorText.text = Utilities.LoadStringFromFile("RegistrationErrorMessage", 50);
    }

    void Awake()
    {
        ZestKit.enableBabysitter = true;
        ZestKit.removeAllTweensOnLevelLoad = true;
        Application.logMessageReceived += Utilities.LoggerCallback;
        Application.targetFrameRate = 60;
        if (!UserLocalData.PlayerDataValid()) return;
        if (GameMaster.UserInformation == null)
            GameMaster.UserInformation = new UserInformation();
        SceneManager.LoadScene("MainMenu");
    }

    void Start()
    {
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
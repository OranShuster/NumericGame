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
    public GameObject FabricGameObject;
    public GameObject CrashGameObject;

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
            ApplicationState.UserStatistics = new UserStatistics(usercode);
            if (ApplicationState.UserStatistics.UserLocalData != null)
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
        {
            ApplicationState.UserStatistics = new UserStatistics();
            SceneManager.LoadScene("MainMenu");
        }
    }
    void Start()
    {
        ZestKit.enableBabysitter = true;
        ZestKit.removeAllTweensOnLevelLoad = true;
        DontDestroyOnLoad(FabricGameObject);
        DontDestroyOnLoad(CrashGameObject);
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

using System;
using System.Collections;
using Prime31.ZestKit;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControllerTutorial : MonoBehaviour, IControllerInterface
{
    private int Score { get; set; }
    public GameObject MessagesOverlay;
    public Text TutorialText;
    public Button SkipButton;
    public Image GameField;
    public Button MenuButton;
    private UserInformation UserInformation    
    {
        get { return GameManager.UserInformation; }
    }
    private bool _gamePaused;
    private Game _mainGame;

    public ControllerTutorial(int score)
    {
        Score = score;
    }

    private float TotalTimePlayed
    {
        get { return GameManager.TotalTimePlayed; }
    }

    void Start()
    {
        ZestKit.enableBabysitter = true;
        ZestKit.removeAllTweensOnLevelLoad = true;
        _mainGame = GameField.GetComponent<Game>();
        SkipButton.image.ZKalphaTo(1, 0.5f).start();
        SkipButton.GetComponentInChildren<Text>().ZKalphaTo(1, 0.5f).start();
        SkipButton.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("Tutorial",4);
        TutorialText.ZKalphaTo(1, 0.5f).start();
        var tutorialHeaderStringName = "TutorialHeaderControl";
        if (!UserInformation.IsControlSession())
        {
            tutorialHeaderStringName = string.Format("TutorialHeader{0}", GameManager.SeriesDelta);
        }
        TutorialText.text = Utilities.LoadStringFromFile(tutorialHeaderStringName, 35);
        MenuButton.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("Menu");
        if (GameManager.SeriesDelta == 0)
            GameManager.GameStartTime = UserInformation.SystemTime.Now();
        Debug.Log(string.Format("INFO|201710221539|Level {0} Tutorial started",GameManager.SeriesDelta));
    }

    public void StartGame()
    {
        ZestKit.instance.stopAllTweens();
        SceneManager.LoadScene("MainGame");
    }

    private void HideMessage(string target)
    {
        ZestKit.instance.stopAllTweens();
        var targetGameObject = GameField.transform.parent.Find(target).gameObject;
        Destroy(targetGameObject);
        _gamePaused = false;
    }
    public IEnumerator ShowMessage(string header, int Score, int Time, bool CanGoBack = false)
    {
        var MessagesOverlay_instance = Instantiate(MessagesOverlay, GameField.transform.parent);
        MessagesOverlay_instance.transform.name = "MessageOverlay";

        var menuButtonCtrl = MessagesOverlay_instance.transform.Find("OverlayBackground/Menu").gameObject.GetComponent<Button>();
        var menuButtonText = MessagesOverlay_instance.transform.Find("OverlayBackground/Menu/Text").gameObject.GetComponent<Text>();
        menuButtonText.text = Utilities.LoadStringFromFile("Menu");
        menuButtonCtrl.onClick.AddListener(() => BackToMenu());

        var cancelButtonCtrl = MessagesOverlay_instance.transform.Find("OverlayBackground/Cancel").gameObject.GetComponent<Button>();
        var cancelButtonText = MessagesOverlay_instance.transform.Find("OverlayBackground/Cancel/Text").gameObject.GetComponent<Text>();
        if (CanGoBack)
        {
            cancelButtonText.text = Utilities.LoadStringFromFile("Cancel");
            cancelButtonCtrl.onClick.AddListener(() => PauseGame());
        }
        else
        {
            cancelButtonCtrl.gameObject.SetActive(false);
            var menuRectTransform = menuButtonCtrl.transform as RectTransform;
            menuRectTransform.transform.localPosition += new Vector3(200, 0, 0);
        }
        var canvasGroup = MessagesOverlay_instance.GetComponent<CanvasGroup>();
        canvasGroup.ZKalphaTo(0.75f).setFrom(0).start();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        var scoreGameObject = MessagesOverlay_instance.transform.Find("OverlayBackground/Score").gameObject;
        var timeGameObject = MessagesOverlay_instance.transform.Find("OverlayBackground/TimePlayed").gameObject;
        scoreGameObject.GetComponent<Text>().text = string.Format("{1} - {0}", Utilities.LoadStringFromFile("Score"), Math.Max(Score, 0));
        timeGameObject.GetComponent<Text>().text = string.Format("{1} - {0}", Utilities.LoadStringFromFile("Round_Length"), Rounds.GetRoundLengthText(Time));
        var MessageTitleGameObject = MessagesOverlay_instance.transform.Find("OverlayBackground/MessageTitle").gameObject;
        MessageTitleGameObject.GetComponent<Text>().text = string.Format(Utilities.LoadStringFromFile(header));
        yield return new WaitForSeconds(0.4f);
    }
    public void PauseGame()
    {
        if (_gamePaused)
        {
            HideMessage("MessageOverlay");
            MenuButton.interactable = true;
            _gamePaused = false;
        }
        else
        {
            _gamePaused = true;
            MenuButton.interactable = false;
            StartCoroutine(ShowMessage("Pause", Score, (int)TotalTimePlayed, true));
        }
        _mainGame.ToggleBoard();
    }

    public bool IsPaused()
    {
        return false;
    }
    public void BackToMenu()
    {
        try
            {
            if (GameManager.SeriesDelta == 0) return;
            UserInformation.AddPlayTime((int) GameManager.TotalTimePlayed, Score);
            UserInformation.SendUserInfoToServerBlocking();
        }
        finally
        {
            Debug.Log("INFO|201711131815|Tutorial Ended");
            SceneManager.LoadScene("UserRegistration");
        }
    }

    private void OnApplicationQuit()
    {           
        Debug.Log("INFO|201711131815|Tutorial OnApplicationQuit hit");
        BackToMenu();
    }

    //Empty methods to comply with interface
    public void IncreaseScore(int amount)
    {
    }

    public void IncreaseGameTimer(float inc)
    {
    }

    public void LoseGame(LoseReasons _)
    {
    }

    public void MoveMade()
    {
    }

    public void LevelUp()
    {
    }
    public void QuitGame()
    {

    }
}
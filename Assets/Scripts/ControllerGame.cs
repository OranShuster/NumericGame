using System;
using System.Collections;
using Prime31.ZestKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerGame : MonoBehaviour, IControllerInterface
{
    private float _gameTimer = Constants.StartingGameTimer;

    private float TotalTimePlayed
    {
        get { return GameManager.TotalTimePlayed; }
        set { GameManager.TotalTimePlayed = value; }
    }

    private Game _mainGame;
    private ITween<float> _warningOverlayTween;
    private bool _gamePaused;

    public UserInformation UserInfo
    {
        get { return GameManager.UserInformation; }
        set { GameManager.UserInformation = value; }
    }

    public Text ScoreText;
    public Text TimeHeaderText;
    public Text ScoreHeaderText;
    public Text PlayTimeHeaderText;
    public Text LevelHeaderText;
    public Text MenuButtonText;

    private int Score
    {
        get { return GameManager.Score; }
        set { GameManager.Score = value; }
    }

    public Image GameField;
    public Text LevelNumText;
    public Text TimerText;
    public Text PlayTimeText;
    public GameObject MessagesOverlay;
    public Image TimerWarningOverlay;
    public GameObject LevelupTutorial;
    public Button MenuButton; 


    void Awake()
    {
        ZestKit.enableBabysitter = true;
        ZestKit.removeAllTweensOnLevelLoad = true;
        _mainGame = GameField.GetComponent<Game>();
        _warningOverlayTween = TimerWarningOverlay.ZKalphaTo(1, 1f).setFrom(0).setLoops(LoopType.PingPong, 1000)
            .setRecycleTween(false);
        InvokeRepeating("SendUserInfoToServer", Constants.ScoreReportingInterval, Constants.ScoreReportingInterval);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        if (UserInfo.GetToday().Control == 1)
            _mainGame.SetNextLevelScore(int.MaxValue, 0);
    }

    void Start()
    {
        ShowScore();
        _gameTimer = Constants.StartingGameTimer;
        LevelNumText.text = GameManager.SeriesDelta.ToString();
        TimeHeaderText.text = Utilities.LoadStringFromFile("Timer");
        ScoreHeaderText.text = Utilities.LoadStringFromFile("Score");
        PlayTimeHeaderText.text = Utilities.LoadStringFromFile("PlayTime");
        LevelHeaderText.text = Utilities.LoadStringFromFile("Level");
        MenuButtonText.text = Utilities.LoadStringFromFile("Menu");
        if (GameManager.SeriesDelta == 0)
        {
            IncreaseScore(0);
            StartCoroutine(UserInfo.SendUserInfoToServer());
        }
        Debug.Log("INFO|201710221540|Game Started");
    }

    // Update is called once per frame
    void Update()
    {
        if (_mainGame.GetState() == GameState.Playing || _mainGame.GetState() == GameState.SelectionStarted)
        {
            if (!_gamePaused)
                UpdateTimers();
            if (Input.GetKeyDown(KeyCode.Escape))
                PauseGame();
        }
        if (GameManager.ConnectionError)
        {
            GameManager.ConnectionError = false;
            _gamePaused = true;
            StartCoroutine(ShowMessage("Connection_Error", GameManager.Score,
                Mathf.CeilToInt(GameManager.TotalTimePlayed)));
        }
    }

    public void PauseGame()
    {
        if (_gamePaused)
        {
            HideMessage("MessageOverlay");
            MenuButton.interactable = true;
            _gamePaused = false;
            Debug.Log("INFO|201711021136|Game resumed");
        }
        else
        {
            _gamePaused = true;
            MenuButton.interactable = false;
            StartCoroutine(ShowMessage("Pause", Score, (int) TotalTimePlayed, true));
            Debug.Log("INFO|201711021137|Game Paused");
        }
        _mainGame.ToggleBoard();
    }

    private void HideMessage(string target)
    {
        ZestKit.instance.stopAllTweens();
        var targetGameObject = GameField.transform.parent.Find(target).gameObject;
        Destroy(targetGameObject);
        _gamePaused = false;
    }

    private void UpdateTimers()
    {
        var deltaTime = Time.deltaTime;
        _gameTimer -= deltaTime;
        TotalTimePlayed += deltaTime;
        TimerText.text = Utilities.SecondsToTime(Mathf.CeilToInt(_gameTimer));
        PlayTimeText.text = Utilities.SecondsToTime(Mathf.CeilToInt(TotalTimePlayed));

        var sessionTimeLeft = UserInfo.GetToday().SessionLength - UserInfo.GetToday().CurrentSessionTimeSecs;
        if (TotalTimePlayed > sessionTimeLeft)
            LoseGame(LoseReasons.SessionTime);
        if (_gameTimer <= 0)
            LoseGame(LoseReasons.GameTime);
        GameTimerWarning();
    }

    private void GameTimerWarning()
    {
        if (_gameTimer < Constants.TimerLow && !_warningOverlayTween.isRunning())
        {
            _warningOverlayTween.start();
            TimerWarningOverlay.color = Constants.ColorDangerRed;
        }
        if (_gameTimer > Constants.TimerLow && _warningOverlayTween.isRunning())
        {
            _warningOverlayTween.stop();
            _warningOverlayTween = TimerWarningOverlay.ZKalphaTo(1, 1f).setFrom(0)
                .setLoops(LoopType.PingPong, 1000);
            TimerWarningOverlay.color = Constants.ColorDangerRed - new Color(0, 0, 0, 1);
        }
    }

    private void ShowScore()
    {
        ScoreText.text = Math.Max(0, Score).ToString();
    }

    public void IncreaseGameTimer(float inc)
    {
        _gameTimer += inc;
        _gameTimer = Mathf.Min(_gameTimer, Constants.TimerMax);
    }

    public void IncreaseScore(int amount)
    {
        Score += amount;
        UserInfo.AddScoreReport(new ScoreReports()
        {
            score = Mathf.Max(amount),
            timestamp = Utilities.GetEpochTime(),
            session_id = UserInfo.GetToday().CurrentSession,
            game_id = GameManager.GameId
        });
        ShowScore();
    }

    public void LoseGame(LoseReasons reason)
    {
        _mainGame.StopGame();
        string headerMsg;
        switch (reason)
        {
            case LoseReasons.Idle:
                headerMsg = "Idle";
                break;
            case LoseReasons.Points:
                headerMsg = "Negative_Points";
                break;
            case LoseReasons.SessionTime:
                headerMsg = "Session_Time";
                break;
            case LoseReasons.GameTime:
                headerMsg = "Game_Time";
                break;
            default:
                throw new ArgumentOutOfRangeException(reason.ToString(), reason, null);
        }
        MenuButton.interactable = false;
        StartCoroutine(ShowMessage(headerMsg, Score, (int) TotalTimePlayed));
    }

    public void LevelUp(int level)
    {
        MenuButton.interactable = false;
        _gamePaused = true;
        ShowLevelupMessage(GameManager.SeriesDelta);
    }

    public void BackToMenu()
    {
        try
        {
            UserInfo.AddPlayTime((int) TotalTimePlayed, Score);
            UserInfo.SendUserInfoToServerBlocking();
        }
        finally
        {
            Debug.Log("INFO|201710221541|Game Ended");
            SceneManager.LoadScene("UserRegistration");
        }
    }

    public IEnumerator ShowMessage(string header, int Score, int Time, bool CanGoBack = false)
    {
        var messagesOverlayInstance = Instantiate(MessagesOverlay, GameField.transform.parent);
        messagesOverlayInstance.transform.name = "MessageOverlay";

        var menuButtonCtrl = messagesOverlayInstance.transform.Find("OverlayBackground/Menu").gameObject
            .GetComponent<Button>();
        var menuButtonText = messagesOverlayInstance.transform.Find("OverlayBackground/Menu/Text").gameObject
            .GetComponent<Text>();
        menuButtonText.text = Utilities.LoadStringFromFile("Menu");
        menuButtonCtrl.onClick.AddListener(() => BackToMenu());

        var cancelButtonCtrl = messagesOverlayInstance.transform.Find("OverlayBackground/Cancel").gameObject
            .GetComponent<Button>();
        var cancelButtonText = messagesOverlayInstance.transform.Find("OverlayBackground/Cancel/Text").gameObject
            .GetComponent<Text>();
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
        var canvasGroup = messagesOverlayInstance.GetComponent<CanvasGroup>();
        canvasGroup.ZKalphaTo(0.75f).setFrom(0).start();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        var scoreGameObject = messagesOverlayInstance.transform.Find("OverlayBackground/Score").gameObject;
        var timeGameObject = messagesOverlayInstance.transform.Find("OverlayBackground/TimePlayed").gameObject;
        scoreGameObject.GetComponent<Text>().text =
            string.Format("{1} - {0}", Utilities.LoadStringFromFile("Score"), Math.Max(Score, 0));
        timeGameObject.GetComponent<Text>().text = string.Format("{1} - {0}",
            Utilities.LoadStringFromFile("Round_Length"), Rounds.GetRoundLengthText(Time));
        var messageTitleGameObject = messagesOverlayInstance.transform.Find("OverlayBackground/MessageTitle")
            .gameObject;
        messageTitleGameObject.GetComponent<Text>().text = string.Format(Utilities.LoadStringFromFile(header));
        yield return new WaitForSeconds(0.4f);
    }

    public void ShowLevelupMessage(int level)
    {
        var levelupTutorialInstance = Instantiate(LevelupTutorial, GameField.transform.parent);
        levelupTutorialInstance.transform.name = "TutorialOverlay";
        var buttonCtrl = levelupTutorialInstance.transform.Find("OverlayBackground/Tutorial").gameObject
            .GetComponent<Button>();
        buttonCtrl.onClick.AddListener(() => SceneManager.LoadScene("Tutorial"));
        buttonCtrl.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("Tutorial");
        var canvasGroup = levelupTutorialInstance.GetComponent<CanvasGroup>();
        canvasGroup.ZKalphaTo(0.75f).setFrom(0).start();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        var header = levelupTutorialInstance.transform.Find("OverlayBackground/LevelupTutorialHeader").gameObject;
        header.GetComponent<Text>().text =
            string.Format("{0} {1}", level, Utilities.LoadStringFromFile("LevelUpMessage"));
    }

    public bool IsPaused()
    {
        return _gamePaused;
    }

    void SendUserInfoToServer()
    {
        StartCoroutine(UserInfo.SendUserInfoToServer());
    }

    public void OnApplicationPause(bool pause)
    {
        if (!pause)
            BackToMenu();
    }

    public void QuitGame()
    {
    }
}
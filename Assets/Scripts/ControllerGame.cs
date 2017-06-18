using System;
using System.Collections;
using Prime31.ZestKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerGame : MonoBehaviour, IControllerInterface
{
    //private int _numberStyle = 0;
    private float _gameTimer = Constants.StartingGameTimer;

    private float _totalTimePlayed
    {
        get { return ApplicationState.TotalTimePlayed; }
        set { ApplicationState.TotalTimePlayed = value; }
    }

    private Game _mainGame;
    private ITween<float> _warningOverlayTween;
    private bool _gamePaused = false;

    public UserStatistics UserInfo { get; set; }
    public Text ScoreText;
    public Text TimeHeaderText;
    public Text ScoreHeaderText;
    public Text LevelHeaderText;
    public Text MenuButtonText;
    public GameObject NumberTilePrefab;
    public Image GameTimerBar;

    private int Score
    {
        get { return ApplicationState.Score; }
        set { ApplicationState.Score = value; }
    }

    public Image GameField;
    public Text LevelNumText;
    public Text TimerText;
    public GameObject MessagesOverlay;
    public Image TimerWarningOverlay;
    public GameObject LevelupTutorial;
    public Button MenuButton;

    void Awake()
    {
        ZestKit.enableBabysitter = true;
        ZestKit.removeAllTweensOnLevelLoad = true;
        _mainGame = GameField.GetComponent<Game>();
        UserInfo = ApplicationState.UserStatistics;
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
        LevelNumText.text = ApplicationState.SeriesDelta.ToString();
        TimeHeaderText.text = Utilities.LoadStringFromFile("Timer");
        ScoreHeaderText.text = Utilities.LoadStringFromFile("Score");
        LevelHeaderText.text = Utilities.LoadStringFromFile("Level");
        MenuButtonText.text = Utilities.LoadStringFromFile("Menu");
        if (ApplicationState.SeriesDelta == 0)
        {
            IncreaseScore(0);
            StartCoroutine(UserInfo.SendUserInfoToServer());
        }
        Debug.Log("1002|Game Started");
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
        if (ApplicationState.ConnectionError)
        {
            ApplicationState.ConnectionError = false;
            _gamePaused = true;
            StartCoroutine(ShowMessage("Connection_Error", ApplicationState.Score,
                Mathf.CeilToInt(ApplicationState.TotalTimePlayed), false));
        }
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
            StartCoroutine(ShowMessage("Pause", Score, (int) _totalTimePlayed, true));
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
        _gameTimer -= Time.deltaTime;
        _totalTimePlayed += Time.deltaTime;
        TimerText.text = Mathf.CeilToInt(_gameTimer).ToString();

        var sessionTimeLeft = UserInfo.GetToday().SessionLength - UserInfo.GetToday().CurrentSessionTimeSecs;
        if (_totalTimePlayed > sessionTimeLeft)
            LoseGame(LoseReasons.SessionTime);
        if (_gameTimer <= 0)
            LoseGame(LoseReasons.GameTime);

        var gameTimerRelative = _gameTimer / Constants.TimerMax * 100;
        GameTimerBar.color = Constants.ColorOkGreen;
        if (gameTimerRelative < 50)
            GameTimerBar.color = Constants.ColorWarningOrange;
        if (gameTimerRelative < 25)
            GameTimerBar.color = Constants.ColorDangerRed;
        GameTimerBar.rectTransform.localScale = new Vector3(Math.Min(gameTimerRelative, 1), 1, 1);

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
            game_id = ApplicationState.GameId
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
        StartCoroutine(ShowMessage(headerMsg, Score, (int) _totalTimePlayed));
    }

    public void LevelUp(int level)
    {
        ShowLevelupMessage(ApplicationState.SeriesDelta);
        MenuButton.interactable = false;
        _gamePaused = true;
    }

    public void BackToMenu()
    {
        try
        {
            UserInfo.AddPlayTime((int) _totalTimePlayed, Score);
            UserInfo.SendUserInfoToServerBlocking();
        }
        finally
        {
            Debug.Log("1004|Game Ended");
            SceneManager.LoadScene("UserRegistration");
        }
    }

    public IEnumerator ShowMessage(string header, int Score, int Time, bool CanGoBack = false)
    {
        var MessagesOverlay_instance = Instantiate(MessagesOverlay, GameField.transform.parent);
        MessagesOverlay_instance.transform.name = "MessageOverlay";

        var menuButtonCtrl = MessagesOverlay_instance.transform.Find("OverlayBackground/Menu").gameObject
            .GetComponent<Button>();
        var menuButtonText = MessagesOverlay_instance.transform.Find("OverlayBackground/Menu/Text").gameObject
            .GetComponent<Text>();
        menuButtonText.text = Utilities.LoadStringFromFile("Menu");
        menuButtonCtrl.onClick.AddListener(() => BackToMenu());

        var cancelButtonCtrl = MessagesOverlay_instance.transform.Find("OverlayBackground/Cancel").gameObject
            .GetComponent<Button>();
        var cancelButtonText = MessagesOverlay_instance.transform.Find("OverlayBackground/Cancel/Text").gameObject
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
        var canvasGroup = MessagesOverlay_instance.GetComponent<CanvasGroup>();
        canvasGroup.ZKalphaTo(0.75f).setFrom(0).start();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        var scoreGameObject = MessagesOverlay_instance.transform.Find("OverlayBackground/Score").gameObject;
        var timeGameObject = MessagesOverlay_instance.transform.Find("OverlayBackground/TimePlayed").gameObject;
        scoreGameObject.GetComponent<Text>().text =
            String.Format("{1} - {0}", Utilities.LoadStringFromFile("Score"), Math.Max(Score, 0));
        timeGameObject.GetComponent<Text>().text = String.Format("{1} - {0}",
            Utilities.LoadStringFromFile("Round_Length"), Rounds.GetRoundLengthText(Time));
        var MessageTitleGameObject = MessagesOverlay_instance.transform.Find("OverlayBackground/MessageTitle")
            .gameObject;
        MessageTitleGameObject.GetComponent<Text>().text = String.Format(Utilities.LoadStringFromFile(header));
        yield return new WaitForSeconds(0.4f);
    }

    public void ShowLevelupMessage(int level)
    {
        var LevelupTutorial_instance = Instantiate(LevelupTutorial, GameField.transform.parent);
        LevelupTutorial_instance.transform.name = "TutorialOverlay";
        var buttonCtrl = LevelupTutorial_instance.transform.Find("OverlayBackground/Tutorial").gameObject
            .GetComponent<Button>();
        buttonCtrl.onClick.AddListener(() => SceneManager.LoadScene("Tutorial"));
        buttonCtrl.GetComponentInChildren<Text>().text = Utilities.LoadStringFromFile("Tutorial");
        var canvasGroup = LevelupTutorial_instance.GetComponent<CanvasGroup>();
        canvasGroup.ZKalphaTo(0.75f).setFrom(0).start();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        var header = LevelupTutorial_instance.transform.Find("OverlayBackground/LevelupTutorialHeader").gameObject;
        header.GetComponent<Text>().text =
            String.Format("{0} {1}", level, Utilities.LoadStringFromFile("LevelUpMessage"));
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
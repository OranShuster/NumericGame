﻿using UnityEngine;
using UnityEngine.UI;
using System;
using Prime31.ZestKit;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using System.Net;

public class ControllerGame : MonoBehaviour,IControllerInterface
{
    //private int _numberStyle = 0;
    private float _gameTimer = Constants.StartingGameTimer;
    private float _totalTimePlayed = 0;
    private Game _mainGame;
    private ITween<float> WarningOverlayTween;
    private bool GamePaused = false;
    Sprite[] numberSquareSprites;

    public UserStatistics UserInfo { get; set; }
    public Text ScoreText;
    public Text TimeHeaderText;
    public Text ScoreHeaderText;
    public Text LevelHeaderText;
    public Text MenuButtonText;
    public GameObject NumberTilePrefab;
    public Image GameTimerBar;
    public int Score { get; set; }

    public Image GameField;
    public Text LevelNumText;
    public Text TimerText;
    public GameObject MessagesOverlay;
    public Image TimerWarningOverlay;
    public GameObject LevelupTutorial;
    public Button MenuButton;

	void Awake()
	{
		_mainGame = GameField.GetComponent<Game>();
		UserInfo = new UserStatistics();
		WarningOverlayTween = TimerWarningOverlay.ZKalphaTo(1, 1f).setFrom(0).setLoops(LoopType.PingPong, 1000).setRecycleTween(false);
		InvokeRepeating("SendUserInfoToServer", Constants.ScoreReportingInterval, Constants.ScoreReportingInterval);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
	    if (UserInfo.GetToday().Control == 1)
	        _mainGame.SetNextLevelScore(int.MaxValue,0);
	    ZestKit.enableBabysitter = true;
	    ZestKit.removeAllTweensOnLevelLoad = true;
	}

    void Start()
    {
        numberSquareSprites = Resources.LoadAll<Sprite>("Images/Numbers").OrderBy(t => Convert.ToInt32(t.name)).ToArray();
        ShowScore();
        _gameTimer = Constants.StartingGameTimer;
        LevelNumText.text = "0";
        TimeHeaderText.text = Utilities.LoadStringFromFile("Time");
        ScoreHeaderText.text = Utilities.LoadStringFromFile("Score");
        LevelHeaderText.text = Utilities.LoadStringFromFile("Level");
        MenuButtonText.text = Utilities.LoadStringFromFile("Menu");
    }

    // Update is called once per frame
    void Update()
    {
        if (_mainGame.GetState() == GameState.Playing || _mainGame.GetState() == GameState.SelectionStarted)
        {
            if (!GamePaused)
                UpdateTimer();
            if (Input.GetKeyDown(KeyCode.Escape))
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (GamePaused)
        {
            HideMessage("MessageOverlay");
            MenuButton.interactable = true;
            GamePaused = false;
        }
        else
        {
            GamePaused = true;
            MenuButton.interactable = false;
            StartCoroutine(ShowMessage("Pause",Score,(int)_totalTimePlayed,true));
        }
        _mainGame.ToggleBoard();
    }

    private void HideMessage(string target)
    {
        ZestKit.instance.stopAllTweens();
        var targetGameObject = GameField.transform.parent.FindChild(target).gameObject;
        Destroy(targetGameObject);
        GamePaused = false;
    }

    private void UpdateTimer()
    {
        _gameTimer -= Time.deltaTime;
        TimerText.text = Math.Ceiling(_gameTimer).ToString();
        _totalTimePlayed += Time.deltaTime;
        var SessionTimeLeft = UserInfo.GetToday().SessionLength - UserInfo.GetToday().CurrentSessionTimeSecs;
        if (_totalTimePlayed > SessionTimeLeft)
        {
            LoseGame(true);
        }
        if (_gameTimer <= 0)
            LoseGame();

        var gameTimerRelative = _gameTimer / Constants.TimerMax*100;
        GameTimerBar.color = Constants.ColorOKGreen;
        if (gameTimerRelative < 50)
            GameTimerBar.color = Constants.ColorWarningOrange;
        if (gameTimerRelative < 25)
            GameTimerBar.color = Constants.ColorDangerRed;
        GameTimerBar.rectTransform.localScale = new Vector3(Math.Min(gameTimerRelative, 1), 1, 1);


        if (_gameTimer<Constants.TimerLow && !WarningOverlayTween.isRunning())
        {
            WarningOverlayTween.start();
        }
        if (_gameTimer > Constants.TimerLow && WarningOverlayTween.isRunning())
        {
            WarningOverlayTween.stop();
            WarningOverlayTween = TimerWarningOverlay.ZKalphaTo(1, 1f).setFrom(0).setLoops(LoopType.PingPong, 1000);
            TimerWarningOverlay.color = new Color(255/ 255f, 68/ 255f, 68 / 255f, 0);
        }
    }

    private void ShowScore()
    {

        ScoreText.text = Math.Max(0, Score).ToString();
    }

    public void MoveMade(){}

    public void IncreaseGameTimer(float inc)
    {
        _gameTimer += inc;
    }
    public void IncreaseScore(int amount)
    {
        Score += amount;
        var unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        UserInfo.AddScoreReport(new ScoreReports()
        {
            score = amount,
            timestamp = unixTimestamp,
            session_id = UserInfo.GetToday().CurrentSession
        });
        ShowScore();
    }

    public void LoseGame(bool sessionTimeUp=false)
    {
        _mainGame.StopGame();
        var headerMsg = "Game_Over";
        if (sessionTimeUp)
            headerMsg = "Session_Ended";
        UserInfo.AddPlayTime((int)_totalTimePlayed, Mathf.Max(0,Score));
        MenuButton.interactable = false;
        StartCoroutine(ShowMessage(headerMsg, Score, (int)_totalTimePlayed));
    }
    public void LevelUp(int level)
    {
        var levelString = level.ToString();
        LevelNumText.text = levelString;
        GamePaused = true;
        _gameTimer = Constants.StartingGameTimer;
        ShowLevelupTutorial(level);
    }
    public void BackToMenu()
    {
        UserInfo.AddPlayTime((int)_totalTimePlayed, Score);
        ZestKit.instance.stopAllTweens();
        SceneManager.LoadScene("MainMenu");
    }
    public void QuitGame()
    {
        UserInfo.AddPlayTime((int)_totalTimePlayed, Score);
        Application.Quit();
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
        scoreGameObject.GetComponent<Text>().text = String.Format("{1} - {0}",Utilities.LoadStringFromFile("Score"), Math.Max(Score, 0));
        timeGameObject.GetComponent<Text>().text = String.Format("{1} - {0}", Utilities.LoadStringFromFile("Round_Length"), Rounds.GetRoundLengthText(Time));
        var MessageTitleGameObject = MessagesOverlay_instance.transform.Find("OverlayBackground/MessageTitle").gameObject;
        MessageTitleGameObject.GetComponent<Text>().text = String.Format(Utilities.LoadStringFromFile(header));
        yield return new WaitForSeconds(0.4f);
    }
    public void ShowLevelupTutorial(int level)
    {
        var LevelupTutorial_instance = Instantiate(LevelupTutorial, GameField.transform.parent);
        LevelupTutorial_instance.transform.name = "TutorialOverlay";
        var buttonCtrl = LevelupTutorial_instance.transform.Find("OverlayBackground/Continue").gameObject.GetComponent<Button>();
        buttonCtrl.onClick.AddListener(() => HideMessage("TutorialOverlay"));
        var canvasGroup = LevelupTutorial_instance.GetComponent<CanvasGroup>();
        canvasGroup.ZKalphaTo(0.75f).setFrom(0).start();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        var Tile1 = LevelupTutorial_instance.transform.Find("OverlayBackground/Tile1").gameObject;
        var Tile2 = LevelupTutorial_instance.transform.Find("OverlayBackground/Tile2").gameObject;
        var Tile3 = LevelupTutorial_instance.transform.Find("OverlayBackground/Tile3").gameObject;
        var TileUp = LevelupTutorial_instance.transform.Find("OverlayBackground/TileUp").gameObject;
        var header = LevelupTutorial_instance.transform.Find("OverlayBackground/LevelupTutorialHeader").gameObject;
        Tile1.GetComponent<Image>().overrideSprite = numberSquareSprites[0];
        Tile2.GetComponent<Image>().overrideSprite =numberSquareSprites[0 + level];
        Tile3.GetComponent<Image>().overrideSprite= numberSquareSprites[0 + level*2];
        TileUp.GetComponent<Image>().overrideSprite = numberSquareSprites[numberSquareSprites.Length-1];
        header.GetComponent<Text>().text = String.Format("{0} {1}",level, Utilities.LoadStringFromFile("LevelUpMessage"));
        AnimateTutorial(new GameObject[] { Tile1, Tile2, Tile3, TileUp });
    }

    private void AnimateTutorial(GameObject[] tiles)
    {
        var tilesImages = new Image[] { tiles[0].GetComponent<Image>(), tiles[1].GetComponent<Image>(), tiles[2].GetComponent<Image>(), tiles[3].GetComponent<Image>() };
        Action<ITween<Vector3>> MarkTilesAction = (task) => StartCoroutine(MarkTilesAnimation(tilesImages));
        tiles[1].transform.ZKlocalPositionTo(tiles[3].transform.localPosition)
            .setFrom(tiles[1].transform.localPosition)
            .setLoops(LoopType.PingPong, 999, 1f).start();
        tiles[3].transform.ZKlocalPositionTo(tiles[1].transform.localPosition)
            .setFrom(tiles[3].transform.localPosition)
            .setLoops(LoopType.PingPong, 999, 1f).setLoopCompletionHandler(MarkTilesAction)
            .start();
    }

    private IEnumerator MarkTilesAnimation(Image[] tiles)
    {

        for (int index=0;index<3;index++)
        {
            if (tiles[index]!=null)
            {
                SetTileColorMatched(tiles[index]);
            }
        }
        yield return new WaitForSeconds(1f);
        foreach (var tile in tiles)
        {
            if (tile != null)
            {
                SetTileColorBase(tile);
            }
        }
    }

    void OnApplicationQuit()
    {
        QuitGame();
    }
    public bool IsPaused()
    {
        return GamePaused;
    }

    void SendUserInfoToServer()
    {
        StartCoroutine(UserInfo.SendUserInfoToServer());
    }

    public bool IsControl()
    {
        return UserInfo.GetToday().Control == 1;
    }
    private void SetTileColorBase(Image go)
    {
        go.color = IsControl()
            ? Constants.ControlBaseColors[go.GetComponent<NumberCell>().Value - 1]
            : Constants.ColorBase;
    }
    private void SetTileColorSelected(Image go)
    {
        go.color = IsControl()
            ? Constants.ControlSelectedColors[go.GetComponent<NumberCell>().Value - 1]
            : Constants.ColorSelected;
    }
    private void SetTileColorMatched(Image go)
    {
        go.color = IsControl()
            ? Constants.ControlMatchedColors[go.GetComponent<NumberCell>().Value - 1]
            : Constants.ColorMatched;
    }
}

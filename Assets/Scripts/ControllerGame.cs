using UnityEngine;
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

    public UserInformation UserInfo { get; set; }
    public Text ScoreText;
    public GameObject NumberSquarePrefab;
    public Image GameTimerBar;
    public int Score { get; set; }

    public Image GameField;
    public Text LevelNumText;
    public Text TimerText;
    public GameObject MessagesOverlay;
    public Image TimerWarningOverlay;
    public GameObject LevelupTurorial;

	void Awake()
	{
		_mainGame = GameField.GetComponent<Game>();
		UserInfo = new UserInformation("");
		var overlayCanvasGroup = MessagesOverlay.GetComponent<CanvasGroup>();
		overlayCanvasGroup.alpha = 0;
		overlayCanvasGroup.interactable = false;
		WarningOverlayTween = TimerWarningOverlay.ZKalphaTo(1, 0.5f).setFrom(0).setLoops(LoopType.PingPong, 1000).setRecycleTween(false);
		InvokeRepeating("SendUserInfoToServer", 10, 10);
	}

    // Use this for initialization
    void Start()
    {
        numberSquareSprites = Resources.LoadAll<Sprite>("Images/Numbers").OrderBy(t => Convert.ToInt32(t.name)).ToArray();
        ShowScore();
        _gameTimer = Constants.StartingGameTimer;
        LevelNumText.text = "0";

    }

    // Update is called once per frame
    void Update()
    {
        if (_mainGame.GetState() == GameState.Playing || _mainGame.GetState() == GameState.SelectionStarted)
        {
            if (!GamePaused)
                UpdateTimer();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GamePaused)
                {
                    HideMessage("MessageOverlay (clone)");
                    GamePaused = false;
                }
                else
                {
                    GamePaused = true;
                    ShowMessage("Pause");
                }
                _mainGame.ToggleBoard();
            }
        }
    }

    private void HideMessage(string target)
    {
        var targetGameObject = GameField.transform.parent.FindChild(target).gameObject;
        Destroy(targetGameObject);
        ZestKit.instance.stopAllTweens();
        GamePaused = false;
    }

    private void UpdateTimer()
    {
        _gameTimer -= Time.deltaTime;
        TimerText.text = Math.Ceiling(_gameTimer).ToString();
        _totalTimePlayed += Time.deltaTime;
        var SessionTimeLeft = UserInfo.GetToday().session_length - UserInfo.GetToday().CurrentSessionTime;
        if (_totalTimePlayed > SessionTimeLeft)
        {
            LoseGame(true);
        }
        if (_gameTimer <= 0)
            LoseGame();

        var gameTimerColor = _gameTimer.Remap(0, Constants.TimerMax/2, 0, 510);
        var gameTimerColorRed = 255- gameTimerColor.Remap(255,510,0,255);
        var gameTimerColorGreen = gameTimerColor.Remap(0, 255, 0, 255);
        GameTimerBar.color = new Color(gameTimerColorRed / 255f, gameTimerColorGreen / 255f, 0);
        GameTimerBar.rectTransform.localScale = new Vector3(Math.Min(_gameTimer / Constants.TimerMax, 1), 1, 1);


        if (_gameTimer<Constants.TimerLow && !WarningOverlayTween.isRunning())
        {
            WarningOverlayTween.start();
        }
        if (_gameTimer > Constants.TimerLow && WarningOverlayTween.isRunning())
        {
            WarningOverlayTween.stop();
            WarningOverlayTween = TimerWarningOverlay.ZKalphaTo(1, 0.5f).setFrom(0).setLoops(LoopType.PingPong, 1000);
            TimerWarningOverlay.color = new Color(1, 0, 0, 0);
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
        UserInfo.ScoreReportsToBeSent.Enqueue(new ScoreReports() { score = amount, timestamp = unixTimestamp, session_id = UserInfo.GetToday().CurrentSession });
        ShowScore();
    }

    public void LoseGame(bool SessionTimeUp=false)
    {
        _mainGame.StopGame();
        var headerMsg = "Game Over";
        if (SessionTimeUp)
            headerMsg = "Session Ended";
        UserInfo.AddPlayTime((int)_totalTimePlayed, Score);
        ShowMessage(headerMsg, Score,(int)_totalTimePlayed,true);
    }
    public void LevelUp(int level)
    {
        var levelString = level.ToString();
        LevelNumText.text = levelString;
        GamePaused = true;
        ShowLevelupTutorial(level);
    }
    public void BackToMenu(bool SaveData=false)
    {
        if (SaveData)
        {
            UserInfo.AddPlayTime((int)_totalTimePlayed, Score);
        }
        SceneManager.LoadScene("MainMenu");
    }
    public void QuitGame()
    {
        UserInfo.AddPlayTime((int)_totalTimePlayed, Score);
        Application.Quit();
    }
    public void ShowMessage(string header, int Score=0, int Time=0, bool ShowScoreTime = false)
    {
        MessagesOverlay = Instantiate(MessagesOverlay, GameField.transform.parent);
        MessagesOverlay.transform.name = "MessageOverlay";
        var buttonCtrl = MessagesOverlay.transform.Find("OverlayBackground/Menu").gameObject.GetComponent<Button>();
        buttonCtrl.onClick.AddListener(() => BackToMenu());
        var canvasGroup = MessagesOverlay.GetComponent<CanvasGroup>();
        canvasGroup.ZKalphaTo(0.75f).setFrom(0).start();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        var ScoreGameObject = MessagesOverlay.transform.Find("OverlayBackground/Score").gameObject;
        var TimeGameObject = MessagesOverlay.transform.Find("OverlayBackground/TimePlayed").gameObject;
        if (ShowScoreTime)
        {
            ScoreGameObject.GetComponent<Text>().text = String.Format("    Score - {0}", Math.Max(Score, 0));
            TimeGameObject.GetComponent<Text>().text = String.Format("  Time Played - {0}", Time);
        }
        else
        {
            ScoreGameObject.SetActive(false);
            TimeGameObject.SetActive(false);
        }
        var MessageTitleGameObject = MessagesOverlay.transform.Find("OverlayBackground/MessageTitle").gameObject;
        MessageTitleGameObject.GetComponent<Text>().text = String.Format(header);
    }
    public void ShowLevelupTutorial(int level)
    {
        var TutorialOverlay = Instantiate(LevelupTurorial, GameField.transform.parent);
        TutorialOverlay.transform.name = "TutorialOverlay";
        var buttonCtrl = TutorialOverlay.transform.Find("OverlayBackground/Continue").gameObject.GetComponent<Button>();
        buttonCtrl.onClick.AddListener(() => HideMessage("TutorialOverlay"));
        var canvasGroup = TutorialOverlay.GetComponent<CanvasGroup>();
        canvasGroup.ZKalphaTo(0.75f).setFrom(0).start();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        var Tile1 = TutorialOverlay.transform.Find("OverlayBackground/Tile1").gameObject;
        var Tile2 = TutorialOverlay.transform.Find("OverlayBackground/Tile2").gameObject;
        var Tile3 = TutorialOverlay.transform.Find("OverlayBackground/Tile3").gameObject;
        var TileUp = TutorialOverlay.transform.Find("OverlayBackground/TileUp").gameObject;
        Tile1.GetComponent<Image>().overrideSprite = numberSquareSprites[0];
        Tile2.GetComponent<Image>().overrideSprite =numberSquareSprites[1 + level];
        Tile3.GetComponent<Image>().overrideSprite= numberSquareSprites[1 + level*2];
        TileUp.GetComponent<Image>().overrideSprite = numberSquareSprites[6];
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
        tiles[0].color = Constants.ColorMatched;
        tiles[1].color = Constants.ColorMatched;
        tiles[2].color = Constants.ColorMatched;
        yield return new WaitForSeconds(1f);
        foreach (var tile in tiles)
        {
            tile.color = Constants.ColorBase;
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
}

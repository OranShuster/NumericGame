using System;
using System.Linq;
using Prime31.ZestKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerMenu : MonoBehaviour
{
    public Text StartGameButtonText;
    public Text ShowStatisticsButtonText;
    public Text InstructionsButtonText;
    public GameObject GameInstructionsText;
    public Text MenuButtonText;

    public GameObject PlayStats;
    public GameObject PlayStatsViewContent;
    public GameObject DayStatsPrefab; //Information line per day 
    public GameObject RoundStatsPrefab; //Information line per round

    public GameObject DebugPanel;

    private UserInformation UserInformation
    {
        get { return GameManager.UserInformation; }
    }

    public Sprite DateStatusBadImage;
    public Sprite DateStatisOkImage;

    public Button StartGameButton;

    void Awake()
    {
        ZestKit.enableBabysitter = true;
        ZestKit.removeAllTweensOnLevelLoad = true;
        StartGameButtonText.text = Utilities.LoadStringFromFile("NewGameButton");
        ShowStatisticsButtonText.text = Utilities.LoadStringFromFile("StatisticsButton");
        InstructionsButtonText.text = Utilities.LoadStringFromFile("Instructions");
        MenuButtonText.text = Utilities.LoadStringFromFile("Menu");
        UpdateStatisticsScrollView();
        DebugPanel.SetActive(Debug.isDebugBuild);
        Debug.Log(string.Format("INFO|201711211220|Running version {0}", Application.version));
    }

    void Update()
    {
        var canPlayStatus = UserInformation.CanPlay();
        if (GameManager.SentCanPlayStatus != canPlayStatus.Status)
            Debug.Log(canPlayStatus.Message);
        switch (canPlayStatus.Status)
        {
            case CanPlayStatus.HasNextTimeslot:
                StartGameButton.interactable = false;
                StartGameButtonText.text = string.Format("({1}) {0}", Utilities.LoadStringFromFile("NewGameButton", 30),
                    UserInformation.TimeToNextSession());
                break;
            case CanPlayStatus.CanPlay:
                StartGameButton.interactable = true;
                StartGameButtonText.text = string.Format("{0}", Utilities.LoadStringFromFile("NewGameButton", 30));
                break;
            case CanPlayStatus.PlayerDisabled:
                StartGameButton.interactable = false;
                StartGameButtonText.text = string.Format("{0}", Utilities.LoadStringFromFile("PlayerDisabled", 30));
                break;
            case CanPlayStatus.GameDone:
                StartGameButton.interactable = false;
                StartGameButtonText.text = string.Format("{0}", Utilities.LoadStringFromFile("GameDone", 30));
                break;
            case CanPlayStatus.None:
                break;
            case CanPlayStatus.WrongTIme:
                StartGameButton.interactable = false;
                StartGameButtonText.text = string.Format("{0}", Utilities.LoadStringFromFile("WrongTime", 30));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        GameManager.SentCanPlayStatus = canPlayStatus.Status;
    }

    public void StartGame()
    {
        GameManager.Levels.CurrentLevel = 0;
        GameManager.Score = 0;
        GameManager.TotalTimePlayed = 0;
        GameManager.UserInformation.ClearScoreReports();
        if (GameManager.UserInformation.GetToday().CurrentSession == 0)
            GameManager.UserInformation.GetToday().CurrentSession = 1;
        else
        {
            if (GameManager.UserInformation.GetToday().CurrentSessionTimeSecs >=
                GameManager.UserInformation.GetToday().SessionLength)
            {
                GameManager.UserInformation.GetToday().CurrentSessionTimeSecs = 0;
                GameManager.UserInformation.GetToday().CurrentSession++;
            }
        }
        var sessionId = UserInformation.GetToday().CurrentSession;
        GameManager.GameId = UserInformation.GetToday().GameRounds.Count(round => round.SessionInd == sessionId) + 1;
        SceneManager.LoadScene("Tutorial");
    }

    public void ShowPlayTimeStatistics()
    {
        PlayStats.SetActive(!PlayStats.activeInHierarchy);
        if (PlayStats.activeInHierarchy)
            UpdateStatisticsScrollView();
        GameInstructionsText.SetActive(false);
    }

    public void ShowGameInstructions()
    {
        PlayStats.SetActive(false);
        GameInstructionsText.SetActive(!GameInstructionsText.activeInHierarchy);
    }

    public void AddGameTime()
    {
        var minutesInputField = DebugPanel.transform.Find("AddScoreText/MinutesInputField").gameObject
            .GetComponent<InputField>();
        var secondsInputField = DebugPanel.transform.Find("AddScoreText/SecondsInputField").gameObject
            .GetComponent<InputField>();
        var scoreInputField = DebugPanel.transform.Find("AddScoreText/ScoreInputField").gameObject
            .GetComponent<InputField>();
        try
        {
            var minutesToAdd = int.Parse(minutesInputField.text);
            var secondsToAdd = int.Parse(secondsInputField.text);
            var scoreToAdd = int.Parse(scoreInputField.text);
            var timeToAdd = minutesToAdd * 60 + secondsToAdd;
            GameManager.GameStartTime = GameManager.SystemTime.Now();
            UserInformation.AddPlayTime(timeToAdd, scoreToAdd);
            UpdateStatisticsScrollView();
        }
        catch (Exception ex)
        {
            Debug.Log(string.Format("DEBUG|201712060004|{0}",ex));
        }
    }

    public void ResetTime()
    {
        GameManager.SystemTime.ResetDateTime();
        UpdateStatisticsScrollView();
    }

    public void ChangeTimeMinute(int amount)
    {
        GameManager.SystemTime.SetDateTime(GameManager.SystemTime.Now().AddMinutes(amount));
        UpdateStatisticsScrollView();
    }

    public void ChangeTimeHour(int amount)
    {
        GameManager.SystemTime.SetDateTime(GameManager.SystemTime.Now().AddHours(amount));
        UpdateStatisticsScrollView();
    }

    public void ChangeTimeDay(int amount)
    {
        GameManager.SystemTime.SetDateTime(GameManager.SystemTime.Now().AddDays(amount));
        UpdateStatisticsScrollView();
    }

    private void UpdateStatisticsScrollView()
    {
        ClearScrollView();
        foreach (PlayDate date in UserInformation)
        {
            AddDateHeaderToScrollView();
            AddDateToScrollView(date);
            if (date.GameRounds.Count > 0)
            {
                AddRoundHeaderToScrollView();
                foreach (var run in date.GameRounds)
                    AddRoundToScrollView(run);
            }
            AddEmptyLineToScrollView();
        }
    }

    private void ClearScrollView()
    {
        foreach (Transform child in PlayStatsViewContent.transform)
        {
            Destroy(child.gameObject);
        }
        PlayStatsViewContent.transform.DetachChildren();
    }

    private void AddEmptyLineToScrollView()
    {
        var go = new GameObject("EmptyRow");
        go.transform.localPosition = new Vector2(500, 100);
        go.layer = 5;
        go.transform.SetParent(PlayStatsViewContent.transform, false);
    }

    GameObject InstantiateRoundRow()
    {
        var numberOfChildren = PlayStatsViewContent.transform.childCount;
        var dateStatsRect = RoundStatsPrefab.transform as RectTransform;
        var yLoc = numberOfChildren * dateStatsRect.rect.height + dateStatsRect.rect.height / 2;
        var go = Instantiate(RoundStatsPrefab, new Vector2(500, -yLoc), Quaternion.identity);
        go.layer = 5;
        go.transform.SetParent(PlayStatsViewContent.transform, false);
        var contentRect = PlayStatsViewContent.transform as RectTransform;
        contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yLoc + dateStatsRect.rect.height);
        return go;
    }

    void AddRoundHeaderToScrollView()
    {
        var go = InstantiateRoundRow();
        var lengthText = go.transform.Find("Length").gameObject.GetComponent<Text>();
        var scoreText = go.transform.Find("Score").gameObject.GetComponent<Text>();
        var timeText = go.transform.Find("Time").gameObject.GetComponent<Text>();
        lengthText.text = Utilities.LoadStringFromFile("LengthHeader", 30);
        scoreText.text = Utilities.LoadStringFromFile("Score", 30);
        timeText.text = Utilities.LoadStringFromFile("Time", 30);
    }

    private void AddRoundToScrollView(Rounds round)
    {
        var go = InstantiateRoundRow();
        var lengthText = go.transform.Find("Length").gameObject.GetComponent<Text>();
        var scoreText = go.transform.Find("Score").gameObject.GetComponent<Text>();
        var timeText = go.transform.Find("Time").gameObject.GetComponent<Text>();
        lengthText.text = round.GetRoundLengthText();
        scoreText.text = round.RoundScore.ToString();
        timeText.text = round.RoundStartTime.ToShortTimeString();
    }

    private GameObject InstantiateDateRow()
    {
        var numberOfChildren = PlayStatsViewContent.transform.childCount;
        var dateStatsRect = DayStatsPrefab.transform as RectTransform;
        var yLoc = numberOfChildren * dateStatsRect.rect.height + dateStatsRect.rect.height / 2;
        var go = Instantiate(DayStatsPrefab, new Vector2(500, -yLoc), Quaternion.identity);
        go.layer = 5;
        go.transform.SetParent(PlayStatsViewContent.transform, false);
        return go;
    }

    private void AddDateHeaderToScrollView()
    {
        var go = InstantiateDateRow();
        var dateString = go.transform.Find("Date").gameObject.GetComponent<Text>();
        var sessionsString = go.transform.Find("Sessions").gameObject.GetComponent<Text>();
        var curSessionTime = go.transform.Find("CurrentSessionTime").gameObject.GetComponent<Text>();
        dateString.text = Utilities.LoadStringFromFile("Date", 30);
        sessionsString.text = Utilities.LoadStringFromFile("SessionsHeader", 30);
        curSessionTime.text = Utilities.LoadStringFromFile("SessionTimeHeader", 50);
    }

    private void AddDateToScrollView(PlayDate date)
    {
        var go = InstantiateDateRow();
        var dateString = go.transform.Find("Date").gameObject.GetComponent<Text>();
        var sessionsString = go.transform.Find("Sessions").gameObject.GetComponent<Text>();
        var curSessionTime = go.transform.Find("CurrentSessionTime").gameObject.GetComponent<Text>();
        var dateStatus = go.transform.Find("DateStatus").gameObject.GetComponent<Image>();
        dateString.text = date.DateObject.ToString(Constants.DateFormatOutput);
        sessionsString.text = string.Format("{0}/{1}", Math.Min(date.CurrentSession, date.NumberOfSessions),
            date.NumberOfSessions);
        curSessionTime.text = string.Format("{0}", date.GetRemainingSessionTimeText());
        var playDate = date.DateObject;
        if (GameManager.SystemTime.Now().Date == playDate)
            dateStatus.sprite = UserInformation.FinishedDay(date) ? DateStatisOkImage : null;
        if (GameManager.SystemTime.Now().Date > playDate)
            dateStatus.sprite = UserInformation.FinishedDay(date) ? DateStatisOkImage : DateStatusBadImage;
        if (GameManager.SystemTime.Now().Date < playDate)
            dateStatus.sprite = null;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
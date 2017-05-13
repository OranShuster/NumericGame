using System;
using System.Globalization;
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
    public GameObject DayStatsPrefab;         //Information line per day 
    public GameObject RoundStatsPrefab;       //Information line per round

    private UserStatistics _userStatistics
    {
        get { return ApplicationState.UserStatistics; }
    }

    public Sprite DateStatusBadImage; 
    public Sprite DateStatisOkImage;

    public Button StartGameButton;

    private string TimeToNextSession="00:00:00";

    void Awake()
    {
        StartGameButtonText.text = Utilities.LoadStringFromFile("NewGameButton");
        ShowStatisticsButtonText.text = Utilities.LoadStringFromFile("StatisticsButton");
        InstructionsButtonText.text = Utilities.LoadStringFromFile("Instructions");
        MenuButtonText.text = Utilities.LoadStringFromFile("Menu");
        ApplicationState.UserStatistics = new UserStatistics();
        foreach (PlayDate date in _userStatistics)
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

    void Update()
    {
        var canPlayStatus = _userStatistics.CanPlay();
        if (canPlayStatus <= 0)
        {
            StartGameButton.interactable = false;
            TimeToNextSession = canPlayStatus == 0 ? _userStatistics.TimeToNextSession() : "00:00:00";
            if (TimeToNextSession == "00:00:00")
            {
                StartGameButtonText.text = Utilities.LoadStringFromFile("NoMoreGames", 30);
                return;
            }
            StartGameButtonText.text = string.Format("({1}) {0})", Utilities.LoadStringFromFile("NewGameButton", 30), TimeToNextSession);
        }
        else
        {
            StartGameButton.interactable = true;
            StartGameButtonText.text = Utilities.LoadStringFromFile("NewGameButton", 30);
        }
    }

    public void StartGame()
    {
        ApplicationState.SeriesDelta = 0;
        ApplicationState.Score = 0;
        ApplicationState.TotalTimePlayed = 0;
        ApplicationState.UserStatistics.ClearScoreReports();
        SceneManager.LoadScene("Tutorial");
    }

    public void ShowPlayTimeStatistics()
    {
        PlayStats.SetActive(!PlayStats.activeInHierarchy);
        GameInstructionsText.SetActive(false);
    }

    public void ShowGameInstructions()
    {
        PlayStats.SetActive(false);
        GameInstructionsText.SetActive(!GameInstructionsText.activeInHierarchy);

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
        var ContentRect = PlayStatsViewContent.transform as RectTransform;
        ContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yLoc + dateStatsRect.rect.height);
        return go;
    }

    void AddRoundHeaderToScrollView()
    {
        var go = InstantiateRoundRow();
        var lengthText = go.transform.Find("Length").gameObject.GetComponent<Text>();
        var scoreText = go.transform.Find("Score").gameObject.GetComponent<Text>();
        var timeText = go.transform.Find("Time").gameObject.GetComponent<Text>();
        lengthText.text = Utilities.LoadStringFromFile("LengthHeader",30);
        scoreText.text = Utilities.LoadStringFromFile("Score",30);
        timeText.text = Utilities.LoadStringFromFile("Time",30);
    }
    private void AddRoundToScrollView(Rounds round)
    {
        var go = InstantiateRoundRow();
        var lengthText = go.transform.Find("Length").gameObject.GetComponent<Text>();
        var scoreText = go.transform.Find("Score").gameObject.GetComponent<Text>();
        var timeText = go.transform.Find("Time").gameObject.GetComponent<Text>();
        lengthText.text = round.GetRoundLengthText();
        scoreText.text = round.RoundScore.ToString();
        timeText.text = round.RoundTime;
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
        var go =InstantiateDateRow();
        var dateString = go.transform.Find("Date").gameObject.GetComponent<Text>();
        var sessionsString = go.transform.Find("Sessions").gameObject.GetComponent<Text>();
        var curSessionTime = go.transform.Find("CurrentSessionTime").gameObject.GetComponent<Text>();
        dateString.text = Utilities.LoadStringFromFile("Date",30);
        sessionsString.text = Utilities.LoadStringFromFile("SessionsHeader",30);
        curSessionTime.text = Utilities.LoadStringFromFile("SessionTimeHeader",30);
    }

    private void AddDateToScrollView(PlayDate date)
    {
        var go = InstantiateDateRow();
        var dateString = go.transform.Find("Date").gameObject.GetComponent<Text>();
        var sessionsString = go.transform.Find("Sessions").gameObject.GetComponent<Text>();
        var curSessionTime = go.transform.Find("CurrentSessionTime").gameObject.GetComponent<Text>();
        var dateStatus = go.transform.Find("DateStatus").gameObject.GetComponent<Image>();
        dateString.text = DateTime.ParseExact(date.Date, Constants.DateFormat, CultureInfo.InvariantCulture).ToShortDateString();
        sessionsString.text = String.Format("{0}/{1}", Math.Min(date.CurrentSession, date.NumberOfSessions), date.NumberOfSessions);
        curSessionTime.text = String.Format("{0}", date.GetRemainingSessionTimeText());
        var playDate = DateTime.ParseExact(date.Date, Constants.DateFormat, CultureInfo.InvariantCulture);
        if (DateTime.Today == playDate)
            dateStatus.sprite = (date.CurrentSession <= date.NumberOfSessions) ? null : DateStatisOkImage;
        if (DateTime.Today > playDate)
            dateStatus.sprite = (date.CurrentSession <= date.NumberOfSessions) ? DateStatusBadImage : DateStatisOkImage;
        if (DateTime.Today < playDate)
            dateStatus.sprite = null;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

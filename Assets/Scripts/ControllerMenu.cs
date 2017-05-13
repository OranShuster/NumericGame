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
    public GameObject DeleteSaveButton;
    public Text StartGameErrorText;

    private string TimeToNextSession="00:00:00";

    void Awake()
    {
        StartGameButtonText.text = Utilities.LoadStringFromFile("NewGameButton");
        ShowStatisticsButtonText.text = Utilities.LoadStringFromFile("StatisticsButton");
        InstructionsButtonText.text = Utilities.LoadStringFromFile("Instructions");

        ApplicationState.UserStatistics = new UserStatistics();
        foreach (PlayDate date in _userStatistics)
        {
            AddDateHeaderToScrollView();
            AddDateToScrollView(date);
            AddRoundHeaderToScrollView();
            foreach (var run in date.GameRounds)
                AddRoundToScrollView(run);
            AddEmptyLineToScrollView();
        }
        if (_userStatistics.IsTestUser())
            DeleteSaveButton.SetActive(true);
    }

    void Update()
    {
        var canPlayStatus = _userStatistics.CanPlay();
        if (canPlayStatus <= 0)
        {
            StartGameButton.interactable = false;
            StartGameErrorText.gameObject.SetActive(true);
            if (canPlayStatus == 0)
            {
                StartGameErrorText.text = Utilities.LoadStringFromFile("NoMoreTimeInSession", 25);
                return;
            }
            TimeToNextSession = _userStatistics.TimeToNextSession();
            StartGameErrorText.text = string.Format("{0}\n{1}", Utilities.LoadStringFromFile("TimeUntillNextSession", 30), TimeToNextSession);
        }
        else
        {
            StartGameButton.interactable = true;
            StartGameErrorText.gameObject.SetActive(false);
        }
    }

    public void StartGame()
    {
        ApplicationState.SeriesDelta = 0;
        ApplicationState.Score = 0;
        ApplicationState.TotalTimePlayed = 0;
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
        lengthText.text = Utilities.LoadStringFromFile("LengthHeader");
        scoreText.text = Utilities.LoadStringFromFile("Score");
        timeText.text = Utilities.LoadStringFromFile("Time");
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
        dateString.text = Utilities.LoadStringFromFile("Date");
        sessionsString.text = Utilities.LoadStringFromFile("SessionsHeader");
        curSessionTime.text = Utilities.LoadStringFromFile("SessionTimeHeader");
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

    public void DeleteSavesAndExit()
    {
        _userStatistics.DeleteSave();
        Application.Quit();
    }
}

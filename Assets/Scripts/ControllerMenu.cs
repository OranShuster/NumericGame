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

    public GameObject PlayStats;
    public GameObject PlayStatsViewContent;
    public GameObject DayStatsPrefab;         //Information line per day 
    public GameObject RoundStatsPrefab;       //Information line per round
    private UserStatistics _userStatistics;

    public Sprite DateStatusBadImage; 
    public Sprite DateStatisOkImage;

    public Button StartGameButton;
    public GameObject DeleteSaveButton;
    public Text StartGameErrorText;

    void Start()
    {
        StartGameButtonText.text = Utilities.LoadStringFromFile("NewGameButton");
        ShowStatisticsButtonText.text = Utilities.LoadStringFromFile("StatisticsButton");
        _userStatistics = new UserStatistics();
        foreach (PlayDate date in _userStatistics)
        {
            AddDateHeaderToScrollView();
            AddDateToScrollView(date);
            AddRoundHeaderToScrollView();
            foreach (var run in date.GameRounds)
                AddRoundToScrollView(run);
            AddEmptyLineToScrollView();
        }
        if (_userStatistics.CanPlay() == 0)
        {
            StartGameButton.interactable = false;
            StartGameErrorText.enabled = true;
            StartGameErrorText.text = Utilities.LoadStringFromFile("NoMoreTimeInSession", 25);
        }
        if (_userStatistics.IsTestUser())
        {
            DeleteSaveButton.SetActive(true);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void ShowPlayTimeStatistics()
    {
        PlayStats.SetActive(!PlayStats.activeInHierarchy);
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
        var provider = CultureInfo.InvariantCulture;
        dateString.text = DateTime.ParseExact(date.Date, Constants.DateFormat, provider).ToShortDateString();
        sessionsString.text = String.Format("{0}/{1}", Math.Min(date.CurrentSession, date.NumberOfSessions), date.NumberOfSessions);
        curSessionTime.text = String.Format("{0}", date.GetRemainingSessionTimeText());
        var playDate = DateTime.ParseExact(date.Date, Constants.DateFormat, provider);
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

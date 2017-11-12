﻿using System;
using System.Globalization;
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
    public GameObject DayStatsPrefab;         //Information line per day 
    public GameObject RoundStatsPrefab;       //Information line per round

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
    }

    void Update()
    {
        
        var canPlayStatus = UserInformation.CanPlay();
        switch (canPlayStatus)
        {
            case CanPlayStatus.NoMoreTimeSlots:
                StartGameButton.interactable = false;
                StartGameButtonText.text = Utilities.LoadStringFromFile("NoMoreGames", 30);
                return;
            case CanPlayStatus.HasNextTimeslot:
                StartGameButton.interactable = false;
                StartGameButtonText.text = string.Format("({1}) {0}", Utilities.LoadStringFromFile("NewGameButton", 30), UserInformation.TimeToNextSession());
                return;
            default:
                StartGameButton.interactable = true;
                StartGameButtonText.text = Utilities.LoadStringFromFile("NewGameButton", 30);
                return;
        }
    }

    public void StartGame()
    {
        GameManager.SeriesDelta = 0;
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
        GameManager.GameId = UserInformation.GetToday().GameRounds.Count(round => round.SessionInd == sessionId)+1;
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
            UserInformation.AddPlayTime(timeToAdd, scoreToAdd,UserInformation.SystemTime.Now().ToShortTimeString());
            UpdateStatisticsScrollView();    
        }
        catch (Exception e)
        {
            //IGNORE
        }
    }

    public void ResetTime()
    {
        UserInformation.SystemTime.ResetDateTime();
    }
    
    public void ChangeTimeMinute(int amount)
    {
        UserInformation.SystemTime.SetDateTime(UserInformation.SystemTime.Now().AddMinutes(amount));
    }
    
    public void ChangeTimeHour(int amount)
    {
        UserInformation.SystemTime.SetDateTime(UserInformation.SystemTime.Now().AddHours(amount));
    }

    public void ChangeTimeDay(int amount)
    {
        UserInformation.SystemTime.SetDateTime(UserInformation.SystemTime.Now().AddDays(amount));
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
        foreach(Transform child in PlayStatsViewContent.transform)
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
        curSessionTime.text = Utilities.LoadStringFromFile("SessionTimeHeader",50);
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

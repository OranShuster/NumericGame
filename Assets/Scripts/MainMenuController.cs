using System;
using System.Globalization;
using Prime31.ZestKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject UserCodePanel;
    public InputField UserCodeInputField;
    public GameObject PlayStatsView;
    public GameObject PlayDateStats;
    public GameObject PlayRunStats;
    public GameObject PlayStatsViewContent;

    private CanvasGroup _mainMenuGroup;
    private CanvasGroup _userCodeGroup;
    private UserInformation _userInfo;
    private Sprite _dateStatusBad; 
    private Sprite _dateStatisOk; 

    // Use this for initialization
    void Start()
    {
        _mainMenuGroup = MainMenuPanel.GetComponent<CanvasGroup>();
        _userCodeGroup = UserCodePanel.GetComponent<CanvasGroup>();
        _dateStatusBad = Resources.Load<Sprite>("cross");
        _dateStatisOk = Resources.Load<Sprite>("yes-tic");
        if (UserInformation.PlayerDataExists())
        {
            _userInfo = new UserInformation("");
            HideUserInputElements();
            ShowMainMenu();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CodeSubmitButtonClick()
    {
        if (UserCodeInputField.text != "")
        {
            if (UserCodeInputField.text == "TESTTEST")
            {
                Utilities.CreateMockUserData();
                _userInfo = new UserInformation("");
            }
            else
            _userInfo = new UserInformation(UserCodeInputField.text);
            if (!_userInfo.UserLoaded)
            {
                Debug.Log("Error getting user info from server");
                _userInfo = null;
            }
            if (_userInfo == null)
            {
                Debug.Log("Error loading user data");
                return;
            }
            HideUserInputElements();
            ShowMainMenu();
        }
    }

    private void ShowMainMenu()
    {
        _mainMenuGroup.ZKalphaTo(1f).start();
        _mainMenuGroup.interactable = true;
        _mainMenuGroup.transform.Find("StartGameButton").gameObject.GetComponent<Button>().interactable = _userInfo.CanPlay()>0;
        foreach (PlayDate date in _userInfo)
        {
            addDateToScrollView(date);
            foreach (Runs run in date.GameRuns)
                addRunToScrollView(run);
        }
    }

    private void addRunToScrollView(Runs run)
    {
        int numberOfChildren = PlayStatsViewContent.transform.childCount;
        RectTransform dateStatsRect = PlayRunStats.transform as RectTransform;
        float yLoc = numberOfChildren * dateStatsRect.rect.height + dateStatsRect.rect.height / 2;
        GameObject go = Instantiate(PlayRunStats, new Vector2(500, -yLoc), Quaternion.identity);
        go.layer = 5;
        go.transform.SetParent(PlayStatsViewContent.transform, false);
        Text LengthText = go.transform.Find("Length").gameObject.GetComponent<Text>();
        Text ScoreText = go.transform.Find("Score").gameObject.GetComponent<Text>();
        Text TimeText = go.transform.Find("Time").gameObject.GetComponent<Text>();
        LengthText.text = run.RunLength.ToString();
        ScoreText.text = run.RunScore.ToString();
        TimeText.text = run.RunTime;
    }

    private void HideUserInputElements()
    {
        _userCodeGroup.ZKalphaTo(0f).start();
        _userCodeGroup.interactable = false;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void ShowPlayTimeStatistics()
    {
        PlayStatsView.SetActive(!PlayStatsView.activeInHierarchy);
    }

    private void addDateToScrollView(PlayDate date)
    {
        int numberOfChildren = PlayStatsViewContent.transform.childCount;
        RectTransform dateStatsRect = PlayDateStats.transform as RectTransform;
        float yLoc = numberOfChildren * dateStatsRect.rect.height + dateStatsRect.rect.height / 2;
        GameObject go = Instantiate(PlayDateStats, new Vector2(500,-yLoc),Quaternion.identity);
        go.layer = 5;
        go.transform.SetParent(PlayStatsViewContent.transform,false);
        Text dateString = go.transform.Find("Date").gameObject.GetComponent<Text>();
        Text sessionsString = go.transform.Find("Sessions").gameObject.GetComponent<Text>();
        Text curSessionTime = go.transform.Find("CurrentSessionTime").gameObject.GetComponent<Text>();
        Image DateStatus = go.transform.Find("DateStatus").gameObject.GetComponent<Image>();
        dateString.text = date.SessionDate;
        sessionsString.text = String.Format("{0}/{1}", Math.Min(date.CurrentSession, date.NumOfSessions), date.NumOfSessions);
        curSessionTime.text = String.Format("{0}/{1}", date.CurrentSessionTime, date.SessionPlayTime);
        CultureInfo provider = CultureInfo.InvariantCulture;
        DateTime playDate = DateTime.ParseExact(date.SessionDate, Constants.DateFormat, provider);
        if (DateTime.Today == playDate)
            if (date.CurrentSession > date.NumOfSessions)
                DateStatus.sprite = _dateStatisOk;
            else
                DateStatus.sprite = null;
        if (DateTime.Today > playDate)
            if (date.CurrentSession > date.NumOfSessions)
                DateStatus.sprite = _dateStatisOk;
            else
                if (date.CurrentSession < date.NumOfSessions ||
                    date.CurrentSession == date.NumOfSessions && date.CurrentSessionTime < date.SessionPlayTime)
                    DateStatus.sprite = _dateStatusBad;
        if (DateTime.Today < playDate)
            DateStatus.sprite = null;
    }
}

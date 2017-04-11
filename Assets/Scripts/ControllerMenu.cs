using System;
using System.Globalization;
using Prime31.ZestKit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControllerMenu : MonoBehaviour
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
    void Update(){}

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
        AddRunToScrollView(null);
        foreach (PlayDate date in _userInfo)
        {
            foreach (var run in date.GameRuns)
                AddRunToScrollView(run);
        }
        AddEmptyLineToScrollView();
        AddDateToScrollView(null);
        foreach (PlayDate date in _userInfo)
            AddDateToScrollView(date);

    }

    private void AddEmptyLineToScrollView()
    {
        var go = Instantiate(new GameObject("EmptyRow"), new Vector2(500, 100), Quaternion.identity);
        go.layer = 5;
        go.transform.SetParent(PlayStatsViewContent.transform, false);
    }
    private void AddRunToScrollView(Runs run)
    {
        var numberOfChildren = PlayStatsViewContent.transform.childCount;
        var dateStatsRect = PlayRunStats.transform as RectTransform;
        var yLoc = numberOfChildren * dateStatsRect.rect.height + dateStatsRect.rect.height / 2;
        var go = Instantiate(PlayRunStats, new Vector2(500, -yLoc), Quaternion.identity);
        go.layer = 5;
        go.transform.SetParent(PlayStatsViewContent.transform, false);
        var lengthText = go.transform.Find("Length").gameObject.GetComponent<Text>();
        var scoreText = go.transform.Find("Score").gameObject.GetComponent<Text>();
        var timeText = go.transform.Find("Time").gameObject.GetComponent<Text>();
        lengthText.text = "ךרוא";
        scoreText.text = "דוקינ";
        timeText.text = "העש";
        if (run != null)
        {
            lengthText.text = run.RunLength.ToString();
            scoreText.text = run.RunScore.ToString();
            timeText.text = run.RunTime;
        }

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

    private void AddDateToScrollView(PlayDate date)
    {
        var numberOfChildren = PlayStatsViewContent.transform.childCount;
        var dateStatsRect = PlayDateStats.transform as RectTransform;
        var yLoc = numberOfChildren * dateStatsRect.rect.height + dateStatsRect.rect.height / 2;
        var go = Instantiate(PlayDateStats, new Vector2(500,-yLoc),Quaternion.identity);
        go.layer = 5;
        go.transform.SetParent(PlayStatsViewContent.transform,false);
        var dateString = go.transform.Find("Date").gameObject.GetComponent<Text>();
        var sessionsString = go.transform.Find("Sessions").gameObject.GetComponent<Text>();
        var curSessionTime = go.transform.Find("CurrentSessionTime").gameObject.GetComponent<Text>();
        var dateStatus = go.transform.Find("DateStatus").gameObject.GetComponent<Image>();
        dateString.text = "ךיראת";
        sessionsString.text = "םינשס";
        curSessionTime.text = "ןשסל ןמז";
        if (date != null)
        {
            dateString.text = date.date;
            sessionsString.text = String.Format("{0}/{1}", Math.Min(date.CurrentSession, date.sessions), date.sessions);
            curSessionTime.text = String.Format("{0}/{1}", date.CurrentSessionTime, date.session_length);
            var provider = CultureInfo.InvariantCulture;
            var playDate = DateTime.ParseExact(date.date, Constants.DateFormat, provider);
            if (DateTime.Today == playDate)
                if (date.CurrentSession > date.sessions)
                    dateStatus.sprite = _dateStatisOk;
                else
                    dateStatus.sprite = null;
            if (DateTime.Today > playDate)
                if (date.CurrentSession > date.sessions)
                    dateStatus.sprite = _dateStatisOk;
                else
                if (date.CurrentSession < date.sessions ||
                    date.CurrentSession == date.sessions && date.CurrentSessionTime < date.session_length)
                    dateStatus.sprite = _dateStatusBad;
            if (DateTime.Today < playDate)
                dateStatus.sprite = null;
        }

    }
}

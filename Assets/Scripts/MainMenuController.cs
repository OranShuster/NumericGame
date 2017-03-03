using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
    public GameObject PlayStatsViewContent;

    private CanvasGroup _mainMenuGroup;
    private CanvasGroup _userCodeGroup;
    private UserInformation _userInfo;
    // Use this for initialization
    void Start()
    {
        _mainMenuGroup = MainMenuPanel.GetComponent<CanvasGroup>();
        _userCodeGroup = UserCodePanel.GetComponent<CanvasGroup>();
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
        if (UserInformation.PlayerDataExists())
            _userInfo = new UserInformation();
        else
            try
            {
                _userInfo = new UserInformation(UserCodeInputField.text);
            }
            catch (Exception)
            {
                Debug.Log("Error getting user info from server");
            }
        if (_userInfo == null)
        {
            Debug.Log("Error loading user data");
            return;
        }
        HideUserInputElements();
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        _mainMenuGroup.ZKalphaTo(1f).start();
        _mainMenuGroup.interactable = true;
        foreach (PlayDate date in _userInfo)
            addDateToScrollView(date);
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
        dateString.text = date.SessionDate;
        sessionsString.text = String.Format("{0}/{1}", Math.Min(date.CurrentSession, date.NumOfSessions), date.NumOfSessions);
        curSessionTime.text = String.Format("{0}/{1}", date.CurrentSessionTime, date.SessionPlayTime);

    }
}

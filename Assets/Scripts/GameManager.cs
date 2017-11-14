using System;
using Prime31.ZestKit;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static UserInformation UserInformation;
    public static int Score = 0;
    public static float TotalTimePlayed = 0;
    public static bool ConnectionError { get; set; }
    public static int GameId;
    public static DateTime GameStartTime;
    public static Levels Levels;
    private static bool _sentCanPlayStatusValue = false;

    public static bool SentCanPlayStatus
    {
        get { return _sentCanPlayStatusValue; }
        set
        {
            _sentCanPlayStatusValue = value;
             #if UNITY_EDITOR
            _sentCanPlayStatusValue = false;
            #endif
        }
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        ZestKit.enableBabysitter = true;
        ZestKit.removeAllTweensOnLevelLoad = true;
        Application.logMessageReceived += Utilities.LoggerCallback;
        Application.targetFrameRate = 60;
        Levels = new Levels();
        DontDestroyOnLoad(gameObject);
    }

    public void SendLogs(LogMessage msg)
    {
        StartCoroutine(UserInformation.SendLogs(msg));
    }

    public void OnApplicationPause(bool pause)
    {
        Debug.Log(pause ? "INFO|20171161428|Game minimized" : "INFO|201711161429|Game maximized");
    }
}
using System;
using JetBrains.Annotations;
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

    public static CanPlayStatus SentCanPlayStatus { get; set; }

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

    public static class SystemTime
    {
        public static TimeSpan DeltaTimeSpan = TimeSpan.Zero;
        public static Func<DateTime> Now = () => DateTime.Now.Add(DeltaTimeSpan);

        public static void SetDateTime(DateTime dateTimeNow)
        {
            DeltaTimeSpan = dateTimeNow.Subtract(DateTime.Now);
            Debug.Log(string.Format("DEBUG|201712041719|Updated time to {0}",Now()));
        }

        public static void AddTimeSpanDelta(TimeSpan delta)
        {
            DeltaTimeSpan += delta;
            Debug.Log(string.Format("DEBUG|201712041719|Updated time to {0}",Now()));
        }

        public static void ResetDateTime()
        {
            DeltaTimeSpan = TimeSpan.Zero;
            Debug.Log(string.Format("DEBUG|201712041719|Updated time to {0}",Now()));
        }
    }
}
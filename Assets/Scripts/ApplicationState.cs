using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ApplicationState
{
    public static UserStatistics UserStatistics;
    public static int SeriesDelta = 0;
    public static int Score = 0;
    public static float TotalTimePlayed = 0;
    public static bool ConnectionError { get; set; }
    public static int GameId;
    public static List<LogMessage> Logs = new List<LogMessage>();

    public static void SendLogs()
    {
        var logString = JsonConvert.SerializeObject(Logs);
        var request = UnityWebRequest.Post(String.Format("{0}/{1}", Constants.LogUrl, ApplicationState.UserStatistics.UserLocalData.UserCode), logString);
        request.SendWebRequest();
        Logs.Clear();
    }
}
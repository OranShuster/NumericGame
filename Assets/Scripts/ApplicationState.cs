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
        var logUrl = string.Format("{0}{1}", Constants.LogUrl, UserStatistics.UserLocalData.UserCode);
        var request = UnityWebRequest.Post(logUrl, logString);
        request.SendWebRequest();
        Debug.Log(string.Format("201722101155|Sent log reports to {0}", logUrl));
        Logs.Clear();
    }
}
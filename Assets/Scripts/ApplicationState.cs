using System;
using Boo.Lang;
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
    public static List<LogMessage> logs = new List<LogMessage>();

    public static void SendLogs()
    {
        var logString = JsonConvert.SerializeObject(logs);
        var request = UnityWebRequest.Post(String.Format("{0}/{1}", Constants.LogUrl, ApplicationState.UserStatistics.UserLocalData.UserCode), logString);
        request.Send();
        logs.Clear();
    }
}
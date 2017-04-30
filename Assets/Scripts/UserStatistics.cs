using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

[Serializable]
public class PlayDate
{
    public string code { get; set; }
    public int session_id { get; set; }
    public string date { get; set; }
    public int sessions { get; set; }
    public float session_length { get; set; }
    public int session_interval { get; set; }
    public int CurrentSession = 1;
    public float CurrentSessionTimeSecs = 0;
    public int LastSessionsEndTime = 0;
    public List<Rounds> GameRounds = new List<Rounds>();

    public string GetRemainingSessionTimeText()
    {
        TimeSpan t = TimeSpan.FromSeconds(session_length-CurrentSessionTimeSecs);

        return string.Format("{0:D2}:{1:D2}:{2:D2}",
            t.Hours,
            t.Minutes,
            t.Seconds);
    }
}

[Serializable]
public class Rounds
{
    public int RoundLength;
    public int RoundScore;
    public string RoundTime;
    public Rounds(int length, int score, string time)
    {
        RoundLength = length;
        RoundScore = score;
        RoundTime = time;
    }
    public Rounds() { }
    public string GetRoundLengthText()
    {
        TimeSpan t = TimeSpan.FromSeconds(RoundLength);

        return string.Format("{0:D2}:{1:D2}",
            t.Minutes,
            t.Seconds);
    }
    public static string GetRoundLengthText(int time)
    {
        TimeSpan t = TimeSpan.FromSeconds(time);

        return string.Format("{0:D2}:{1:D2}",
            t.Minutes,
            t.Seconds);
    }
}

public class ScoreReports
{
    public int score;
    public int timestamp;
    public int session_id;
}

[Serializable]
public class UserLocalData
{
    public PlayDate[] PlayDates { get; set; }
    public string UserCode;

    public UserLocalData(string userDataJson, string userCode)
    {
        PlayDates = JsonConvert.DeserializeObject<PlayDate[]>(userDataJson);
        UserCode = userCode;
        foreach (var date in PlayDates)
        {
            date.session_interval *= 60;
            date.session_length *= 60;
        }
    }

    public UserLocalData(PlayDate[] dates, string userCode)
    {
        UserCode = userCode;
        PlayDates = dates;
    }

    [JsonConstructor]
    private UserLocalData()
    {
    }
}

public class UserStatistics : IEnumerable
{
    public UserLocalData UserLocalData;
    private Queue<ScoreReports> ScoreReportsToBeSent = new Queue<ScoreReports>();
    static string _userDataPath = Application.persistentDataPath + "/userData.cjd";
    public UserStatistics(string userCode)
    {
        if (Utilities.IsTestCode(userCode))
        {
            Utilities.CreateMockUserData();
            UserLocalData = Load();
        }
        else
        {
            var userDataJson = GetJsonFromServer(userCode);
            UserLocalData = new UserLocalData(userDataJson, userCode);
            Save(UserLocalData);
        }
    }

    public UserStatistics()
    {
        UserLocalData = Load();
    }

    public string GetJsonFromServer(string userCode)
    {
        var getReq = UnityWebRequest.Get(Constants.BaseUrl + String.Format("{0}", userCode));
        getReq.Send();
        while (!getReq.isDone) { }
        if (getReq.isError)
        {
            Debug.LogError(getReq.error);
        }
        var dl = getReq.downloadHandler;
        return Encoding.ASCII.GetString(dl.data);
    }

    public float CanPlay()
    {
        var todayDateEntry = GetToday();
        if (todayDateEntry != null && todayDateEntry.CurrentSession <= todayDateEntry.sessions)
        {
            var timeSinceLastCompleteSession = GetEpochTime() - todayDateEntry.LastSessionsEndTime;
            var finishedSession = Mathf.Ceil(todayDateEntry.CurrentSessionTimeSecs) == 0;
            if (finishedSession)
            {
                if (timeSinceLastCompleteSession >= todayDateEntry.session_interval)
                    return todayDateEntry.session_length - todayDateEntry.CurrentSessionTimeSecs;
                return 0;
            }
            return todayDateEntry.session_length - todayDateEntry.CurrentSessionTimeSecs;
        }
        return 0;
    }

    internal void AddPlayTime(int length,int score)
    {
        var today = GetToday();
        var thisTime = DateTime.Now.ToShortTimeString();
        today.GameRounds.Add(new Rounds(length, score,thisTime));
        today.CurrentSessionTimeSecs += length;
        if (today.CurrentSessionTimeSecs>today.session_length)
        {
            today.CurrentSessionTimeSecs = 0;
            today.LastSessionsEndTime = GetEpochTime();
            today.CurrentSession++;
        }
        Save(UserLocalData);
    }

    public static void Save(UserLocalData userLocalData)
    {
        Debug.Log(string.Format("Saving user data from {0}", _userDataPath));
        StreamWriter writer = null;
        try
        {
            writer = new StreamWriter(_userDataPath);
            writer.Write(JsonConvert.SerializeObject(userLocalData, Formatting.Indented)); 
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }

    }

    public static UserLocalData Load()
    {
        if (PlayerDataExists())
        {
            Debug.Log(string.Format("Loading user data from {0}", _userDataPath));
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(_userDataPath);
                return JsonConvert.DeserializeObject<UserLocalData>(reader.ReadToEnd());
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
        return null;
    }

    public IEnumerator GetEnumerator()
    {
        return UserLocalData.PlayDates.GetEnumerator();
    }
    public PlayDate GetToday()
    {
        var todayDate = DateTime.Today.ToString(Constants.DateFormat);
        return Array.Find(UserLocalData.PlayDates, x => x.date == todayDate);
    }
    public float GetCurrentSessionTime()
    {
        var today = GetToday();
        if (today != null)
            return today.CurrentSessionTimeSecs;
        return -1;
    }
    public static int GetEpochTime()
    {
        return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    }
    public IEnumerator SendUserInfoToServer()
    {
        if (ScoreReportsToBeSent.Count > 0)
        {
            var scoreReportsArr = ScoreReportsToBeSent.ToArray();
            var jsonString = JsonConvert.SerializeObject(scoreReportsArr);
            var request = UnityWebRequest.Post(Constants.BaseUrl + string.Format("/{0}/{1}", UserLocalData.UserCode, GetToday().session_id), jsonString);
            request.Send();
            while (!request.isDone) {
                yield return new WaitForSeconds(1f);
            }
            if (request.isError)
            {
                Debug.LogWarning(request.error);
            }
            else
            {
                ScoreReportsToBeSent.Clear();
            }
        }
    }

    public void AddScoreReport(ScoreReports scoreReport)
    {
        ScoreReportsToBeSent.Enqueue(scoreReport);
    }

    public static bool PlayerDataExists()
    {
        return File.Exists(_userDataPath);
    }
}
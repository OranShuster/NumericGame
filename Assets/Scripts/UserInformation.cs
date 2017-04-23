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
    public float CurrentSessionTime = 0;
    public int LastSessionsEndTime = 0;
    public List<Runs> GameRuns = new List<Runs>();
}

[Serializable]
public class UserLocalData
{
    public PlayDate[] PlayDates { get; set; }
    public string UserCode;
}

[Serializable]
public class Runs
{
    public int RunLength;
    public int RunScore;
    public string RunTime;
    public Runs(int length,int score,string time)
    {
        RunLength = length;
        RunScore = score;
        RunTime = time;
    }
    public Runs() { }
}

public class ScoreReports
{
    public int score;
    public int timestamp;
    public int session_id;
}

public class UserInformation : IEnumerable
{
    public UserLocalData UserLocalData;
    static string _userDataPath = Application.persistentDataPath + "/userData.cjd";
    public bool UserLoaded = false;
    public Queue<ScoreReports> ScoreReportsToBeSent = new Queue<ScoreReports>();
    public string UserCode;
    private System.Timers.Timer _sendUserScoresTimer;
    public UserInformation(string userCode)
    {
        UserLocalData = new UserLocalData() { UserCode = userCode };
        if (userCode == "")
        {
            UserLocalData = Load();
            if (UserLocalData != null)
                UserLoaded = true;
        }
        else
        {
            var json = GetJsonFromServer(userCode);
            if (json != "")
            {
                UserLocalData.PlayDates = JsonConvert.DeserializeObject<PlayDate[]>(json);
                foreach (var date in UserLocalData.PlayDates)
                {
                    date.session_interval *= 60;
                    date.session_length *= 60;
                }
                
                Save();
                UserLoaded = true;
            }
            else
                UserLoaded = false;
        }
    }
    public UserInformation(){}

    public string GetJsonFromServer(string userCode)
    {
        var getReq = UnityWebRequest.Get(Constants.BaseUrl + String.Format("{0}", userCode));
        getReq.Send();
        while (!getReq.isDone) { }
        if (getReq.isError)
        {
            Debug.Log(getReq.error);
            return "";
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
            var finishedSession = todayDateEntry.CurrentSessionTime == 0;
            if (finishedSession)
            {
                if (timeSinceLastCompleteSession >= todayDateEntry.session_interval)
                    return todayDateEntry.session_length - todayDateEntry.CurrentSessionTime;
            }
            else
                return todayDateEntry.session_length - todayDateEntry.CurrentSessionTime;

        }


        return 0;
    }

    internal void AddPlayTime(int length,int score)
    {
        var today = GetToday();
        var thisTime = DateTime.Now.ToShortTimeString();
        today.GameRuns.Add(new Runs(length, score,thisTime));
        today.CurrentSessionTime += length;
        if (today.CurrentSessionTime>today.session_length)
        {
            today.CurrentSessionTime = 0;
            today.LastSessionsEndTime = GetEpochTime();
            today.CurrentSession++;
        }
        Save();
    }

    public void Save()
    {
        Debug.Log(string.Format("Saving user data from {0}", _userDataPath));
        StreamWriter writer = null;
        try
        {
            writer = new StreamWriter(_userDataPath);
            writer.Write(JsonConvert.SerializeObject(UserLocalData,Formatting.Indented)); 
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }

    }

    public UserLocalData Load()
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

    public static bool PlayerDataExists()
    {
        return File.Exists(_userDataPath);
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
            return today.CurrentSessionTime;
        return -1;
    }
    public static int GetEpochTime()
    {
        return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    }
    public IEnumerator SendUserInfoToServer(object source = null, System.Timers.ElapsedEventArgs e = null)
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
}
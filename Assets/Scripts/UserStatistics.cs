using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json.Serialization;

[Serializable]
public class PlayDate
{
    public string Email { get; set; }
    public string Code { get; set; }
    public int session_id { get; set; }
    public string Date { get; set; }
    public int NumberOfSessions { get; set; }
    public int SessionLength { get; set; }
    public int SessionInterval { get; set; }
    public int Control { get; set; }
    public int CurrentSession = 1;
    public int CurrentSessionTimeSecs = 0;
    public int LastSessionsEndTime = 0;
    public List<Rounds> GameRounds = new List<Rounds>();

    [JsonConstructor]
    public PlayDate(string code, int session_length, string start_date, int num_of_sessions, int control, string email,
        int session_interval)
    {
        Code = code;
        SessionLength = session_length;
        SessionInterval = session_interval;
        Control = control;
        Date = start_date;
        NumberOfSessions = num_of_sessions;
        Email = email;
    }

    public PlayDate()
    {
    }

    public string GetRemainingSessionTimeText()
    {
        TimeSpan t = TimeSpan.FromSeconds(SessionLength-CurrentSessionTimeSecs);

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
            date.SessionInterval *= 60;
            date.SessionLength *= 60;
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
        var control = Utilities.IsTestCode(userCode);
        if (control >= 0)
        {
            Utilities.CreateMockUserData(control);
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
        var getReq = UnityWebRequest.Get(Constants.BaseUrl + userCode + "/");
        getReq.Send();
        while (!getReq.isDone) { System.Threading.Thread.Sleep(500); }
        if (getReq.isError)
        {
            Debug.LogError(getReq.error);
        }
        var dl = getReq.downloadHandler;
        var jsonString = Encoding.ASCII.GetString(dl.data);
        Debug.Log("Got user json " + jsonString);
        return jsonString;
    }

    public int CanPlay()
    {
        var todayDateEntry = GetToday();
        if (todayDateEntry != null && todayDateEntry.CurrentSession <= todayDateEntry.NumberOfSessions)
        {
            var timeSinceLastCompleteSession = GetEpochTime() - todayDateEntry.LastSessionsEndTime;
            var finishedSession = todayDateEntry.CurrentSessionTimeSecs == 0;
            if (finishedSession)
            {
                if (timeSinceLastCompleteSession >= todayDateEntry.SessionInterval)
                    return todayDateEntry.SessionLength - todayDateEntry.CurrentSessionTimeSecs;
                return 0;
            }
            return todayDateEntry.SessionLength - todayDateEntry.CurrentSessionTimeSecs;
        }
        return 0;
    }

    internal void AddPlayTime(int length,int score)
    {
        var today = GetToday();
        var thisTime = DateTime.Now.ToShortTimeString();
        today.GameRounds.Add(new Rounds(length, score,thisTime));
        today.CurrentSessionTimeSecs += length;
        if (today.CurrentSessionTimeSecs>today.SessionLength)
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
            var jsonString = JsonConvert.SerializeObject(userLocalData, Formatting.Indented);
            byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(jsonString);
            string encodedJson = Convert.ToBase64String(toEncodeAsBytes);
            writer.Write(encodedJson); 
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }

    }

    public static UserLocalData Load()
    {
        Debug.Log(string.Format("Loading user data from {0}", _userDataPath));
        StreamReader reader = null;
        try
        {
            reader = new StreamReader(_userDataPath);
            byte[] encodedDataAsBytes = Convert.FromBase64String(reader.ReadToEnd());
            string decodedString = Encoding.ASCII.GetString(encodedDataAsBytes);
            return JsonConvert.DeserializeObject<UserLocalData>(decodedString);
        }
        finally
        {
            if (reader != null)
                reader.Close();
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
        return Array.Find(UserLocalData.PlayDates, x => x.Date == todayDate);
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
            yield return request.Send();
            if (request.isError)
                Debug.LogWarning(request.error);
            else
                ScoreReportsToBeSent.Clear();
        }
    }

    public void AddScoreReport(ScoreReports scoreReport)
    {
        ScoreReportsToBeSent.Enqueue(scoreReport);
    }

    public static bool PlayerDataValid()
    {
        if (!File.Exists(_userDataPath))
            return false;
        try
        {
            var userStats = new UserStatistics();
            if (userStats.UserLocalData == null)
            {
                File.Delete(_userDataPath);
                return false;
            }
        }
        catch
        {
            File.Delete(_userDataPath);
            return false;
        }
        return true;
    }

    public bool IsTestUser()
    {
        var control = Utilities.IsTestCode(UserLocalData.UserCode);
        return  control>=0;
    }

    public void DeleteSave()
    {
        File.Delete(_userDataPath);
    }
}
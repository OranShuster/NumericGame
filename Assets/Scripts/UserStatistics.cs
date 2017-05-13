using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;

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
        if (UserLocalData.PlayDates.Where(day => IsPastDay(day.Date)).Any(day => !CheckPastDayValid(day)))
        {
            return 0;
        }
        var todayDateEntry = GetToday();
        return todayDateEntry == null ? 0 : CheckTodayValid(todayDateEntry);
    }

    public string TimeToNextSession()
    {
        var todayEntry = GetToday();
        var secondsToNextSession = todayEntry.SessionInterval - (GetEpochTime() - todayEntry.LastSessionsEndTime) ;
        var t = TimeSpan.FromSeconds(secondsToNextSession);
        return string.Format("{0:D2}:{1:D2}:{2:D2}",
            t.Hours,
            t.Minutes,
            t.Seconds);
    }

    private int CheckTodayValid(PlayDate todayDateEntry)
    {
        //Finished today sessions
        if (todayDateEntry.CurrentSession > todayDateEntry.NumberOfSessions) return 0;

        //Check session interval
        var timeSinceLastCompleteSession = GetEpochTime() - todayDateEntry.LastSessionsEndTime;
        if (timeSinceLastCompleteSession >= todayDateEntry.SessionInterval)
            return todayDateEntry.SessionLength - todayDateEntry.CurrentSessionTimeSecs;
        return timeSinceLastCompleteSession - todayDateEntry.SessionInterval;
    }

    private bool CheckPastDayValid(PlayDate date)
    {
        return date.CurrentSession > date.NumberOfSessions;
    }

    public bool IsPastDay(string date)
    {
        var dateObject = DateTime.ParseExact(date, Constants.DateFormat, CultureInfo.InvariantCulture);
        return dateObject.Date < DateTime.Today.Date;
    }
    internal void AddPlayTime(int length,int score)
    {
        var today = GetToday();
        var thisTime = DateTime.Now.ToShortTimeString();
        today.GameRounds.Add(new Rounds(length, Mathf.Max(0,score),thisTime));
        today.CurrentSessionTimeSecs += length;
        if (today.CurrentSessionTimeSecs>=today.SessionLength)
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
    public IEnumerator SendUserInfoToServer(bool blocking=false)
    {
        if (ScoreReportsToBeSent.Count > 0)
        {
            var scoreReportsArr = ScoreReportsToBeSent.ToArray();
            var reportsCount = scoreReportsArr.GetLength(0);
            var jsonString = JsonConvert.SerializeObject(scoreReportsArr);
            var request = new UnityWebRequest(Constants.BaseUrl + string.Format("/{0}/{1}/", UserLocalData.UserCode, GetToday().SessionId));
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonString));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.uploadHandler.contentType = "application/json";
            request.SetRequestHeader("content-type", "application/json");
            if (blocking)
            {
                request.Send();
                while (!request.isDone)
                    System.Threading.Thread.Sleep(250);
            }
            else
                yield return request.Send();
            if (request.isError)
                Debug.LogWarning(request.error);
            else
            {
                Debug.Log(string.Format("Sent {0} score reports to server", reportsCount));
                for (var msgIdx = 1; msgIdx < reportsCount; msgIdx++)
                    ScoreReportsToBeSent.Dequeue();
            }
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
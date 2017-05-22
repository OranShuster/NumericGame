using System;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Debug = UnityEngine.Debug;

public class UserStatistics : IEnumerable
{
    public UserLocalData UserLocalData;
    private List<ScoreReports> ScoreReportsToBeSent = new List<ScoreReports>();
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
            try
            {
                var userDataJson = GetJsonFromServer(userCode);
                UserLocalData = new UserLocalData(userDataJson, userCode);
                Save(UserLocalData);
            }
            catch
            {
                UserLocalData = null;
            }
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
        while (!getReq.isDone) { System.Threading.Thread.Sleep(250); }
        if (getReq.isError || getReq.responseCode > 204)
        {
            Debug.LogError(getReq.error);
        }
        var dl = getReq.downloadHandler;
        var jsonString = Encoding.ASCII.GetString(dl.data);
        Debug.Log("Got user json " + jsonString);
        return jsonString;
    }

    public CanPlayStatus CanPlay()
    {
        //Check past days
        if (UserLocalData.PlayDates.Where(day => day.DateObject.Date < DateTime.Today.Date).Any(day => !IsPlayDateValid(day)))
            return CanPlayStatus.NoMoreTimeSlots;
        //Check if you are after 8:00AM
        if (DateTime.Now < DateTime.Today.AddHours(8) && CheckDateExists(DateTime.Today))
            return CanPlayStatus.HasNextTimeslot;
        //Check if exists a future PlayDate
        if (GetNextPlayDate() == null)
            return -1;
        //Check if today is valid
        return CheckPlayDateValid(GetToday());
    }

    private bool CheckDateExists(DateTime today)
    {
        return UserLocalData.PlayDates.Count(day => day.DateObject.Date == DateTime.Today) == 1;
    }

    private PlayDate GetNextPlayDate()
    {
        return UserLocalData.PlayDates.Where(IsPlayDateValid).Min(date => date.);
    }

    public string TimeToNextSession()
    {
        if (DateTime.Now < DateTime.Today.AddHours(8) || IsPlayDateValid(GetToday()))
            return TimeTo8AmTomorrow;
        var todayEntry = GetToday();
        var t = TimeSpan.FromSeconds(todayEntry.SessionInterval - (GetEpochTime() - todayEntry.LastSessionsEndTime));
        return string.Format("{0:D2}:{1:D2}:{2:D2}",
            t.Hours,
            t.Minutes,
            t.Seconds);
    }

    private static bool IsPlayDateValid(PlayDate date)
    {
        if (date.DateObject.Date == DateTime.Today)
            return date.CurrentSession <= date.NumberOfSessions;
        if (date.DateObject.Date < DateTime.Today)
            return date.CurrentSession == date.NumberOfSessions && 
                date.CurrentSessionTimeSecs >= date.SessionLength;
        return true;
    }

    private static string TimeTo8AmTomorrow
    {
        get
        {
            var t = DateTime.Today.AddDays(1).AddHours(8).Subtract(DateTime.Now);
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                t.Hours,
                t.Minutes,
                t.Seconds);
        }
    }

    private object GetTomorrow()
    {
        var todayDate = DateTime.Today.AddDays(1);
        return Array.Find(UserLocalData.PlayDates, x => x.DateObject == todayDate);
    }

    private int CheckPlayDateValid(PlayDate todayDateEntry)
    {
        //No entries for today
        if (todayDateEntry == null)
            return 0;
        //Finished today sessions
        if (todayDateEntry.CurrentSession == todayDateEntry.NumberOfSessions && 
            todayDateEntry.CurrentSessionTimeSecs >= todayDateEntry.SessionLength)
            return 0;
        //Check session interval
        var timeSinceLastCompleteSession = GetEpochTime() - todayDateEntry.LastSessionsEndTime;
        return timeSinceLastCompleteSession >= todayDateEntry.SessionInterval ? 1 : 0;
    }
    public void AddPlayTime(int length,int score)
    {
        var today = GetToday();
        var thisTime = DateTime.Now.ToShortTimeString();
        today.GameRounds.Add(new Rounds(length, Mathf.Max(0,score),thisTime));
        today.CurrentSessionTimeSecs += length;
        if (today.CurrentSessionTimeSecs>=today.SessionLength)
        {
            today.CurrentSessionTimeSecs = today.SessionLength;
            today.LastSessionsEndTime = GetEpochTime();
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
            var encodedDataAsBytes = Convert.FromBase64String(reader.ReadToEnd());
            var decodedString = Encoding.ASCII.GetString(encodedDataAsBytes);
            return JsonConvert.DeserializeObject<UserLocalData>(decodedString);
        }
        catch
        {
            return null;
        }
        finally
        {
            if (reader != null)
                reader.Close();
        }
    }

    public IEnumerator GetEnumerator()
    {
        return UserLocalData.PlayDates.GetEnumerator();
    }
    public PlayDate GetToday()
    {
        return Array.Find(UserLocalData.PlayDates, x => x.DateObject == DateTime.Today);
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
        if (ScoreReportsToBeSent.Count <= 0) yield break;
        var reportsCount = ScoreReportsToBeSent.Count;
        var jsonString = JsonConvert.SerializeObject(ScoreReportsToBeSent);
        Debug.Log(string.Format("Sent {0} score reports to server - {1}", reportsCount, Utilities.PrintArray<ScoreReports>(ScoreReportsToBeSent.ToArray())));
        ClearScoreReports(reportsCount);
        var request = new UnityWebRequest(Constants.BaseUrl + string.Format("/{0}/{1}/", UserLocalData.UserCode, GetToday().SessionId));
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonString));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.uploadHandler.contentType = "application/json";
        request.SetRequestHeader("content-type", "application/json");
        yield return request.Send();
        if (!request.isError && (request.responseCode == 200 || IsTestUser())) yield break;
        ApplicationState.ConnectionError = true;
        Debug.LogWarning(request.error);
    }
    public void SendUserInfoToServerBlocking()
    {
        if (ScoreReportsToBeSent.Count <= 0) return;
        var reportsCount = ScoreReportsToBeSent.Count;
        var jsonString = JsonConvert.SerializeObject(ScoreReportsToBeSent);
        Debug.Log(string.Format("Sent {0} score reports to server - {1}", reportsCount, Utilities.PrintArray<ScoreReports>(ScoreReportsToBeSent.ToArray())));
        ClearScoreReports(reportsCount);
        var url = Constants.BaseUrl + string.Format("/{0}/{1}/", UserLocalData.UserCode, GetToday().SessionId);
        var request = Utilities.CreatePostUnityWebRequest(url,jsonString);

        request.Send();
        while (!request.isDone)
            System.Threading.Thread.Sleep(250);
        if (!request.isError && (request.responseCode == 200 || IsTestUser())) return;
        ApplicationState.ConnectionError = true;
        Debug.LogWarning(request.error);

    }

    public void AddScoreReport(ScoreReports scoreReport)
    {
        ScoreReportsToBeSent.Add(scoreReport);
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

    public bool IsControl()
    {
        return GetToday().Control == 1;
    }

    public void DeleteSave()
    {
        File.Delete(_userDataPath);
    }

    public void ClearScoreReports(int count)
    {
        ScoreReportsToBeSent.RemoveRange(0,count);
    }
    public void ClearScoreReports()
    {
        ScoreReportsToBeSent.Clear();
    }
}
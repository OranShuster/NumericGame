using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class UserStatistics : IEnumerable
{
    public UserLocalData UserLocalData = null;
    private List<ScoreReports> _scoreReportsToBeSent = new List<ScoreReports>();
    public static string UserDataPath = Application.persistentDataPath + "/userData.cjd";

    public UserStatistics(string userCode)
    {
        var control = Utilities.IsTestCode(userCode);
        if (control > 0)
        {
            Utilities.CreateMockUserData(control);
            UserLocalData = UserLocalData.Load();
        }
        else
        {
            try
            {
                var userDataJson = GetJsonFromServer(userCode);
                UserLocalData = new UserLocalData(userDataJson, userCode);
                UserLocalData.Save(UserLocalData);
            }
            catch
            {
                UserLocalData = null;
            }
        }
    }

    public UserStatistics()
    {
        UserLocalData = UserLocalData.Load();
    }

    public string GetJsonFromServer(string userCode)
    {
        var getReq = UnityWebRequest.Get(Constants.BaseUrl + userCode + "/");
        getReq.SendWebRequest();
        while (!getReq.isDone)
        {
            Thread.Sleep(100);
        }
        if (getReq.isNetworkError || getReq.responseCode > 204)
        {
            Debug.LogError(string.Format("1009|{0}",getReq.error));
        }
        var dl = getReq.downloadHandler;
        var jsonString = Encoding.ASCII.GetString(dl.data);
        Debug.Log("1010|Got user json " + jsonString);
        return jsonString;
    }

    public CanPlayStatus CanPlay()
    {
        //Check past days
        if (UserLocalData.PlayDates.Where(day => day.DateObject.Date < SystemTime.Now().Date)
            .Any(day => !FinishedDay(day)))
        {
            Console.WriteLine("Past days bad");
            return CanPlayStatus.NoMoreTimeSlots;
        }
        //Check Today
        if (!DateExistsAndHasSessions(SystemTime.Now().Date))
            return GetNextPlayDate() == null ? CanPlayStatus.NoMoreTimeSlots : CanPlayStatus.HasNextTimeslot;
        //Check if you are after 8:00AM
        if (SystemTime.Now() < SystemTime.Now().Date.AddHours(8))
            return CanPlayStatus.HasNextTimeslot;
        var today = GetToday();
        return Utilities.GetEpochTime() - today.LastSessionsEndTime < today.SessionInterval ? CanPlayStatus.HasNextTimeslot : CanPlayStatus.CanPlay;
    }

    private bool DateExists(DateTime datetime)
    {
        return UserLocalData.PlayDates.Count(day => day.DateObject.Date == datetime.Date) == 1;
    }

    private PlayDate GetNextPlayDate()
    {
        return UserLocalData.PlayDates.Where(playDate => DateExistsAndHasSessions(playDate.DateObject)).Min();
    }

    public string TimeToNextSession()
    {
        var nextPlayDate = GetNextPlayDate();
        if (nextPlayDate.DateObject.Date > SystemTime.Now().Date)
            return GetTimeSpanToDateTime(nextPlayDate.DateObject.Date.AddHours(8));
        if (SystemTime.Now() < SystemTime.Now().Date.AddHours(8))
            return GetTimeSpanToDateTime(SystemTime.Now().Date.AddHours(8));
        var t = TimeSpan.FromSeconds(nextPlayDate.SessionInterval -
                                     (Utilities.GetEpochTime() - nextPlayDate.LastSessionsEndTime));
        return string.Format("{0:D2}:{1:D2}:{2:D2}",
            t.Hours,
            t.Minutes,
            t.Seconds);
    }

    public static string GetTimeSpanToDateTime(DateTime to)
    {
        var t = to.Subtract(SystemTime.Now());
        return string.Format("{0:D2}:{1:D2}:{2:D2}",
            t.Hours,
            t.Minutes,
            t.Seconds);
    }

    private static bool FinishedDay(PlayDate playdate)
    {
        return playdate.CurrentSession == playdate.NumberOfSessions &&
               playdate.CurrentSessionTimeSecs >= playdate.SessionLength;
    }

    private bool DateExistsAndHasSessions(DateTime date)
    {
        return DateExists(date) && !FinishedDay(GetPlayDateByDateTime(date));
    }

    public void AddPlayTime(int length, int score)
    {
        var today = GetPlayDateByDateTime(SystemTime.Now().Date);
        var thisTime = SystemTime.Now().ToShortTimeString();
        if (today.CurrentSession == 0)
            today.CurrentSession = 1;
        if (today.CurrentSessionTimeSecs == today.SessionLength)
        {
            today.CurrentSessionTimeSecs = 0;
            today.CurrentSession++;
        }
        today.GameRounds.Add(new Rounds(length, Mathf.Max(0, score), thisTime, today.CurrentSession));
        today.CurrentSessionTimeSecs += length;
        Debug.Log(string.Format("1011|Adding {0} play time to {1}.{2} session time left", length, thisTime, today.SessionLength - today.CurrentSessionTimeSecs));
        if (today.CurrentSessionTimeSecs >= today.SessionLength)
        {
            Debug.Log(string.Format("1012|Incrementing session. seesion interval is {0}",today.SessionInterval));
            today.CurrentSessionTimeSecs = today.SessionLength;
            today.LastSessionsEndTime = Utilities.GetEpochTime();
        }
        UserLocalData.Save(UserLocalData);
    }

    public IEnumerator GetEnumerator()
    {
        return UserLocalData.PlayDates.GetEnumerator();
    }

    public PlayDate GetPlayDateByDateTime(DateTime dateTime)
    {
        return Array.Find(UserLocalData.PlayDates, x => x.DateObject == dateTime.Date);
    }

    public PlayDate GetToday()
    {
        return Array.Find(UserLocalData.PlayDates, x => x.DateObject == SystemTime.Now().Date);
    }

    public IEnumerator SendUserInfoToServer()
    {
        if (_scoreReportsToBeSent.Count <= 0) yield break;
        var reportsCount = _scoreReportsToBeSent.Count;
        var jsonString = JsonConvert.SerializeObject(_scoreReportsToBeSent);
        Debug.Log(String.Format("1013|Sent {0} score reports to server - {1}", reportsCount,
            Utilities.PrintArray<ScoreReports>(_scoreReportsToBeSent.ToArray())));
        _scoreReportsToBeSent.Clear();
        var url = Constants.BaseUrl + String.Format("/{0}/{1}/", UserLocalData.UserCode,
                      GetPlayDateByDateTime(DateTime.Today).SessionId);
        var request = Utilities.CreatePostUnityWebRequest(url, jsonString);
        yield return request.SendWebRequest();
        if (!request.isNetworkError && (request.responseCode == 200 || IsTestUser())) yield break;
        if (request.responseCode == Constants.InvalidPlayerCode)
            DisablePlayer();
        ApplicationState.ConnectionError = true;
        Debug.LogWarning(String.Format("1014|{0}",request.error));
    }

    public void DisablePlayer()
    {
        foreach (var playdate in UserLocalData.PlayDates.Where(playDate => playDate.DateObject >= DateTime.Today))
            playdate.DateObject = playdate.DateObject.Date.AddYears(-1);
    }

    public void SendUserInfoToServerBlocking()
    {
        if (_scoreReportsToBeSent.Count <= 0) return;
        var reportsCount = _scoreReportsToBeSent.Count;
        var jsonString = JsonConvert.SerializeObject(_scoreReportsToBeSent);
        Debug.Log(String.Format("1015|Sent {0} score reports to server - {1}", reportsCount,
            Utilities.PrintArray<ScoreReports>(_scoreReportsToBeSent.ToArray())));
        _scoreReportsToBeSent.Clear();
        var url = Constants.BaseUrl + String.Format("/{0}/{1}/", UserLocalData.UserCode,
                      GetPlayDateByDateTime(DateTime.Today).SessionId);
        var request = Utilities.CreatePostUnityWebRequest(url, jsonString);
        request.SendWebRequest();
        while (!request.isDone)
            Thread.Sleep(250);
        if (!request.isNetworkError && (request.responseCode == 200 || IsTestUser())) return;
        ApplicationState.ConnectionError = true;
        Debug.LogWarning(String.Format("1016|{0}",request.error));

    }

    public void AddScoreReport(ScoreReports scoreReport)
    {
        _scoreReportsToBeSent.Add(scoreReport);
    }

    public bool IsTestUser()
    {
        return Utilities.IsTestCode(UserLocalData.UserCode) > 0;
    }

    public bool IsControlSession()
    {
        return GetPlayDateByDateTime(DateTime.Today).Control == 2;
    }

    public void ClearScoreReports()
    {
        _scoreReportsToBeSent.Clear();
    }

    public static class SystemTime
    {
        public static Func<DateTime> Now = () => DateTime.Now;

        public static void SetDateTime(DateTime dateTimeNow)
        {
            Now = () => dateTimeNow;
        }

        public static void ResetDateTime()
        {
            Now = () => DateTime.Now;
        }
    }
}
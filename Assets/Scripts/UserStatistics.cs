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
    public UserLocalData UserLocalData;
    private List<ScoreReports> _scoreReportsToBeSent = new List<ScoreReports>();
    public static string UserDataPath = Application.persistentDataPath + "/userData.cjd";

    public UserStatistics(string userCode)
    {
        var control = Utilities.IsTestCode(userCode);
        if (control >= 0)
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
        getReq.Send();
        while (!getReq.isDone)
        {
            Thread.Sleep(100);
        }
        if (getReq.isError || getReq.responseCode > 204)
        {
            Debug.LogError(getReq.error);
        }
        var dl = getReq.downloadHandler;
        var jsonString = Encoding.ASCII.GetString(dl.data);
        Debug.Log("Got user json " + jsonString);
        return jsonString;
    }

    public CanPlayStatus CanPlay(DateTime now)
    {
        Console.WriteLine("Checking date {0}", now);
        //Check past days
        if (UserLocalData.PlayDates.Where(day => day.DateObject.Date < now.Date)
            .Any(day => !FinishedDay(day)))
        {
            Console.WriteLine("Past days bad");
            return CanPlayStatus.NoMoreTimeSlots;
        }
        //Check Today
        if (!DateExistsAndHasSessions(now.Date))
            return GetNextPlayDate() == null ? CanPlayStatus.NoMoreTimeSlots : CanPlayStatus.HasNextTimeslot;
        //Check if you are after 8:00AM
        return now < now.Date.AddHours(8) ? CanPlayStatus.HasNextTimeslot : CanPlayStatus.CanPlay;
    }

    private bool DateExists(DateTime datetime)
    {
        return UserLocalData.PlayDates.Count(day => day.DateObject.Date == datetime.Date) >= 1;
    }

    private PlayDate GetNextPlayDate()
    {
        return UserLocalData.PlayDates.Where(playDate => DateExistsAndHasSessions(playDate.DateObject)).Min();
    }

    public string TimeToNextSession(DateTime now)
    {
        var nextPlayDate = GetNextPlayDate();
        if (nextPlayDate.DateObject.Date > DateTime.Today)
            return GetTimeSpanToDateTime(nextPlayDate.DateObject.Date.AddHours(8), now);
        if (now < DateTime.Today.AddHours(8))
            return GetTimeSpanToDateTime(DateTime.Today.AddHours(8), now);
        var t = TimeSpan.FromSeconds(nextPlayDate.SessionInterval -
                                     (GetEpochTime() - nextPlayDate.LastSessionsEndTime));
        return String.Format("{0:D2}:{1:D2}:{2:D2}",
            t.Hours,
            t.Minutes,
            t.Seconds);
    }

    private static string GetTimeSpanToDateTime(DateTime dateTime,DateTime now)
    {
        var t = dateTime.Subtract(now);
        return String.Format("{0:D2}:{1:D2}:{2:D2}",
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

    public void AddPlayTime(int length, int score,DateTime date)
    {
        var today = GetPlayDateByDateTime(date.Date);
        var thisTime = DateTime.Now.ToShortTimeString();
        if (today.CurrentSession == 0)
            today.CurrentSession = 1;
        if (today.CurrentSessionTimeSecs == today.SessionLength)
        {
            today.CurrentSessionTimeSecs = 0;
            today.CurrentSession++;
        }
        today.GameRounds.Add(new Rounds(length, Mathf.Max(0, score), thisTime,today.CurrentSession));
        today.CurrentSessionTimeSecs += length;
        if (today.CurrentSessionTimeSecs >= today.SessionLength)
        {
            today.CurrentSessionTimeSecs = today.SessionLength;
            today.LastSessionsEndTime = GetEpochTime();
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
        return Array.Find(UserLocalData.PlayDates, x => x.DateObject == DateTime.Today);
    }

    public static int GetEpochTime()
    {
        return (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public IEnumerator SendUserInfoToServer()
    {
        if (_scoreReportsToBeSent.Count <= 0) yield break;
        var reportsCount = _scoreReportsToBeSent.Count;
        var jsonString = JsonConvert.SerializeObject(_scoreReportsToBeSent);
        Debug.Log(String.Format("Sent {0} score reports to server - {1}", reportsCount,
            Utilities.PrintArray<ScoreReports>(_scoreReportsToBeSent.ToArray())));
        _scoreReportsToBeSent.Clear();
        var url = Constants.BaseUrl + String.Format("/{0}/{1}/", UserLocalData.UserCode, GetPlayDateByDateTime(DateTime.Today).SessionId);
        var request = Utilities.CreatePostUnityWebRequest(url, jsonString);
        yield return request.Send();
        if (!request.isError && (request.responseCode == 200 || IsTestUser())) yield break;
        if (request.responseCode == Constants.InvalidPlayerCode)
            DisablePlayer();
        ApplicationState.ConnectionError = true;
        Debug.LogWarning(request.error);
    }

    private void DisablePlayer()
    {
        foreach (var playdate in UserLocalData.PlayDates.Where(playDate => playDate.DateObject >= DateTime.Today))
            playdate.DateObject = playdate.DateObject.Date.AddYears(-1);
    }

    public void SendUserInfoToServerBlocking()
    {
        if (_scoreReportsToBeSent.Count <= 0) return;
        var reportsCount = _scoreReportsToBeSent.Count;
        var jsonString = JsonConvert.SerializeObject(_scoreReportsToBeSent);
        Debug.Log(String.Format("Sent {0} score reports to server - {1}", reportsCount,
            Utilities.PrintArray<ScoreReports>(_scoreReportsToBeSent.ToArray())));
        _scoreReportsToBeSent.Clear();
        var url = Constants.BaseUrl + String.Format("/{0}/{1}/", UserLocalData.UserCode, GetPlayDateByDateTime(DateTime.Today).SessionId);
        var request = Utilities.CreatePostUnityWebRequest(url, jsonString);
        request.Send();
        while (!request.isDone)
            Thread.Sleep(250);
        if (!request.isError && (request.responseCode == 200 || IsTestUser())) return;
        ApplicationState.ConnectionError = true;
        Debug.LogWarning(request.error);

    }

    public void AddScoreReport(ScoreReports scoreReport)
    {
        _scoreReportsToBeSent.Add(scoreReport);
    }

    public bool IsTestUser()
    {
        return Utilities.IsTestCode(UserLocalData.UserCode) >= 0;
    }

    public bool IsControlSession()
    {
        return GetPlayDateByDateTime(DateTime.Today).Control == 1;
    }

    public void ClearScoreReports()
    {
        _scoreReportsToBeSent.Clear();
    }
}
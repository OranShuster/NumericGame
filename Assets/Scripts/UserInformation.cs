﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class UserInformation : IEnumerable
{
    public UserLocalData UserLocalData;
    private List<ScoreReports> _scoreReportsToBeSent = new List<ScoreReports>();
    public static string UserDataPath = Application.persistentDataPath + "/userData.cjd";

    public UserInformation(string userCode)
    {
        var control = Utilities.IsControlCode(userCode);
        var test = Utilities.IsTestCode(userCode);
        if (test)
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

    public UserInformation()
    {
        Debug.Log(string.Format("DEBUG|201710221544|Loading user data from {0}", UserDataPath));
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
            Debug.LogError(string.Format("ERROR|201711021017|{0}", getReq.error));
        }
        var dl = getReq.downloadHandler;
        var jsonString = Encoding.ASCII.GetString(dl.data);
        Debug.Log("DEBUG|201710221542|Got user json " + jsonString);
        return jsonString;
    }

    public int CalculateRemainingPlayTime()
    {
        var today = GetToday();
        var elapsedTime = (today.CurrentSession - 1) * today.SessionLength + today.CurrentSessionTimeSecs;
        elapsedTime += (today.CurrentSession - 1) * today.SessionInterval;
        var totalPlayTime = today.NumberOfSessions * today.SessionLength +
                            (today.NumberOfSessions - 1) * today.SessionInterval;
        var timeLeft = totalPlayTime - elapsedTime;
//        Debug.Log(string.Format("DEBUG|201711131017| {0} = {1} - {2}",timeLeft,totalPlayTime,elapsedTime));
//        Debug.Log("DEBUG|201711072029|" + today);
        return timeLeft;
    }

    public CanPlayStatus CanPlay()
    {
        //Debug.Log("DEBUG|201711021421|Checking CanPlay status for " + SystemTime.Now());
        //Check past days
        if (UserLocalData.PlayDates.Where(day => day.DateObject.Date < SystemTime.Now().Date)
            .Any(day => !FinishedDay(day)))
        {
            return CanPlayStatus.NoMoreTimeSlots;
        }

        //Check Today
        if (!DateExistsAndHasSessions(SystemTime.Now().Date))
            return GetNextPlayDate() == null ? CanPlayStatus.NoMoreTimeSlots : CanPlayStatus.HasNextTimeslot;
        var remainingGameTime = CalculateRemainingPlayTime();
        var timeRemainingInDay = (int) (SystemTime.Now().Date.AddDays(1) - SystemTime.Now()).TotalSeconds;
//        Debug.Log("DEBUG|201711021423|timeRemainingInDay = " + timeRemainingInDay);
        //Check if the sessions can be finished
        if (timeRemainingInDay < remainingGameTime)
            return CanPlayStatus.NoMoreTimeSlots;
        //Check if you are after 8:00AM
        if (SystemTime.Now() < SystemTime.Now().Date.AddHours(8))
            return CanPlayStatus.HasNextTimeslot;
        var today = GetToday();
        return Utilities.GetEpochTime() - today.LastSessionsEndTime < today.SessionInterval
            ? CanPlayStatus.HasNextTimeslot
            : CanPlayStatus.CanPlay;
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
        if (today.CurrentSession == 0)
            today.CurrentSession = 1;
        if (today.CurrentSessionTimeSecs == today.SessionLength)
        {
            today.CurrentSessionTimeSecs = 0;
            today.CurrentSession++;
        }
        today.GameRounds.Add(new Rounds(length, Mathf.Max(0, score), Utilities.SecondsToTime(GameManager.GameStartTime),
            today.CurrentSession));
        today.CurrentSessionTimeSecs += length;
        Debug.Log(string.Format(
            "INFO|201710221545|{{started_time: {0}, total_time:{1}, left_time:{2}, total_points:{3}, game: {4}, session: {5}}}",
            GameManager.GameStartTime, length, today.SessionLength - today.CurrentSessionTimeSecs, score,
            GameManager.GameId, today.CurrentSession));
        if (today.CurrentSessionTimeSecs >= today.SessionLength)
        {
            Debug.Log(string.Format("INFO|201710221546|Incrementing session. seesion interval is {0}",
                today.SessionInterval));
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
        _scoreReportsToBeSent.Clear();
        var url = Constants.BaseUrl + string.Format("{0}/{1}/", UserLocalData.UserCode,
                      GetPlayDateByDateTime(DateTime.Today).SessionId);
        var request = Utilities.CreatePostUnityWebRequest(url, jsonString);
        yield return request.SendWebRequest();
        Debug.Log(string.Format("DEBUG|201710221547|Sent {0} score reports to {1}:\n{2}", reportsCount, url,
            Utilities.PrintArray(_scoreReportsToBeSent.ToArray())));
        if (request.responseCode == Constants.InvalidPlayerCode)
            DisablePlayer();
        if (!request.isNetworkError && (request.responseCode == 200 || IsTestUser())) yield break;
        GameManager.ConnectionError = true;
        Debug.Log(string.Format("ERROR|201710221548|{0}", request.error));
    }

    public void SendUserInfoToServerBlocking()
    {
        if (_scoreReportsToBeSent.Count <= 0) return;
        var reportsCount = _scoreReportsToBeSent.Count;
        var jsonString = JsonConvert.SerializeObject(_scoreReportsToBeSent);

        _scoreReportsToBeSent.Clear();
        var url = Constants.BaseUrl + string.Format("{0}/{1}/", UserLocalData.UserCode,
                      GetPlayDateByDateTime(DateTime.Today).SessionId);
        var request = Utilities.CreatePostUnityWebRequest(url, jsonString);
        request.SendWebRequest();
        Debug.Log(string.Format("DEBUG|201710221549|Sent {0} score reports to {1} - {2}", reportsCount, url,
            Utilities.PrintArray(_scoreReportsToBeSent.ToArray())));
        while (!request.isDone)
            Thread.Sleep(250);
        if (!request.isNetworkError && (request.responseCode == 200 || IsTestUser())) return;
        GameManager.ConnectionError = true;
        Debug.Log(string.Format("ERROR|201710221550|{0}", request.error));
    }

    public IEnumerator SendLogs(LogMessage msg)
    {
        var logString = JsonConvert.SerializeObject(new[] {msg});
        var logUrl = string.Format("{0}{1}", Constants.LogUrl, UserLocalData.UserCode);
        var response = Utilities.CreatePostUnityWebRequest(logUrl, logString);
        yield return response.SendWebRequest();
        if (response.isNetworkError)
            Debug.LogError(string.Format("ERROR|201722101453|Log POST request to {0} failed with error -\n{1}", logUrl,
                response.error));
        if (response.isHttpError)
            Debug.LogError(string.Format("ERROR|201722101454|Log POST request to {0} failed with response code -\n{1}",
                logUrl, response.responseCode));
        if (response.isHttpError || response.isNetworkError)
            yield break;
        Debug.Log(string.Format("DEBUG|201722101155|Sent log reports to {0}", logUrl));
    }

    public void DisablePlayer()
    {
        foreach (var playdate in UserLocalData.PlayDates.Where(playDate => playDate.DateObject >= DateTime.Today))
            playdate.DateObject = playdate.DateObject.Date.AddYears(-1);
        UserLocalData.Save(UserLocalData);
    }

    public void AddScoreReport(ScoreReports scoreReport)
    {
        if (GameManager.UserInformation.IsTestUser())
            return;
        _scoreReportsToBeSent.Add(scoreReport);
    }

    public bool IsTestUser()
    {
        return Utilities.IsTestCode(UserLocalData.UserCode);
    }

    public bool IsControlSession()
    {
        return GetPlayDateByDateTime(DateTime.Today).Control == Constants.ControlSessionFlag;
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
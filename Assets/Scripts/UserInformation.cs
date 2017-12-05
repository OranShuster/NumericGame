using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;
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

    DateTime GetLastRoundTime()
    {
        var lastDayWithRounds = Array.FindLast(UserLocalData.PlayDates, x => x.GameRounds.Count > 0);
        if (lastDayWithRounds == null)
            return GameManager.SystemTime.Now();
        var rounds = lastDayWithRounds.GameRounds;
        if (rounds == null)
            return GameManager.SystemTime.Now();
        var lastRoundTime = rounds.Max(x => x.RoundStartTime.AddSeconds(x.RoundLength));
        return lastRoundTime;
    }

    public int CalculateRemainingPlayTime()
    {
        var today = GetToday();
        var elapsedTime = (today.CurrentSession - 1) * today.SessionLength + today.CurrentSessionTimeSecs;
        elapsedTime += (today.CurrentSession - 1) * today.SessionInterval;
        if (today.LastSessionsEndTime !=0)
            elapsedTime += Mathf.Min(Utilities.GetEpochTime() - today.LastSessionsEndTime, today.SessionInterval) ;
        var totalPlayTime = today.NumberOfSessions * today.SessionLength +
                            (today.NumberOfSessions - 1) * today.SessionInterval;
        var timeLeft = totalPlayTime - elapsedTime;
        return timeLeft;
    }

    public CanPlayResult CanPlay()
    {
        var now = GameManager.SystemTime.Now();
        //Check if diabled
        if (UserLocalData.PlayerDisabled)
        {
            return new CanPlayResult(CanPlayStatus.PlayerDisabled, "INFO|201711202241|Player is disabled");
        }
        //Check past days are finished
        if (UserLocalData.PlayDates.Where(day => day.DateObject.Date < now.Date)
            .Any(day => !FinishedDay(day)))
        {
            DisablePlayer();
            return new CanPlayResult(CanPlayStatus.PlayerDisabled, "INFO|201711202241|Past days are not finished");
        }
        //Check Today
        if (DateExistsAndHasSessions(now.Date))
        {
            //Check if you are after 8:00AM
            if (now < now.Date.AddHours(8))
            {
                return new CanPlayResult(CanPlayStatus.HasNextTimeslot, "INFO|201711202245|Before 8AM");
            }

            //Check if the sessions can be finished
            var remainingGameTime = CalculateRemainingPlayTime();
            var tomorrow = now.Date.AddDays(1);
            var timeRemainingInDay = (int) (tomorrow - now).TotalSeconds;
            if (timeRemainingInDay < remainingGameTime)
            {
                DisablePlayer();
                return new CanPlayResult(CanPlayStatus.PlayerDisabled, string.Format(
                    "INFO|201711202244|Not enough remaining time {0}/{1}", remainingGameTime,
                    timeRemainingInDay));
            }

            //Check time since last session ended
            var today = GetToday();
            if (Utilities.GetEpochTime() - today.LastSessionsEndTime < today.SessionInterval)
            {
                return new CanPlayResult(CanPlayStatus.HasNextTimeslot,
                    "INFO|201711202247|Session interval time not passed");
            }

            //Check last rounds is before now
            var lastRoundTime = GetLastRoundTime();
            if (now.AddSeconds(1) <= lastRoundTime)
            {
                return new CanPlayResult(CanPlayStatus.WrongTIme,
                    string.Format("INFO|20171204|Wrong Clock now = {0} last round time={1}",
                        now.ToShortTimeString(), lastRoundTime.ToShortTimeString()));
            }
            return new CanPlayResult(CanPlayStatus.CanPlay, "INFO|201711202248|Can play");
        }
        return GetNextPlayDate() == null
            ? new CanPlayResult(CanPlayStatus.GameDone, "INFO|201711202242|Finished all time slots")
            : new CanPlayResult(CanPlayStatus.HasNextTimeslot, "INFO|201711202242|Has future time slot");
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
        if (nextPlayDate.DateObject.Date > GameManager.SystemTime.Now().Date)
            return GetTimeSpanToDateTime(nextPlayDate.DateObject.Date.AddHours(8));
        if (GameManager.SystemTime.Now() < GameManager.SystemTime.Now().Date.AddHours(8))
            return GetTimeSpanToDateTime(GameManager.SystemTime.Now().Date.AddHours(8));
        var t = TimeSpan.FromSeconds(nextPlayDate.SessionInterval -
                                     (Utilities.GetEpochTime() - nextPlayDate.LastSessionsEndTime));
        return string.Format("{0:D2}:{1:D2}:{2:D2}",
            t.Hours,
            t.Minutes,
            t.Seconds);
    }

    public static string GetTimeSpanToDateTime(DateTime to)
    {
        var t = to.Subtract(GameManager.SystemTime.Now());
        return string.Format("{0:D2}:{1:D2}:{2:D2}",
            t.Hours,
            t.Minutes,
            t.Seconds);
    }

    public static bool FinishedDay(PlayDate playdate)
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
        var today = GetPlayDateByDateTime(GameManager.GameStartTime);
        if (today == null) return;
        if (today.CurrentSession == 0)
            today.CurrentSession = 1;
        if (today.CurrentSessionTimeSecs == today.SessionLength)
        {
            today.CurrentSessionTimeSecs = 0;
            today.CurrentSession++;
            today.LastSessionsEndTime = 0;
        }
        today.GameRounds.Add(new Rounds(length, Mathf.Max(0, score), GameManager.GameStartTime,
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
        return Array.Find(UserLocalData.PlayDates, x => x.DateObject == GameManager.SystemTime.Now().Date);
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
        if (request.isHttpError && !IsTestUser())
            Debug.Log(string.Format("INFO|201711241158|Player recieved response {0} from server after score report",
                request.responseCode));
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
        if (request.responseCode == Constants.InvalidPlayerCode)
            DisablePlayer();
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
        var responseDateHeader = response.GetResponseHeader("Date");
        var serverDateTime = DateTime.ParseExact(responseDateHeader, "R", CultureInfo.InvariantCulture);
        if (!GameManager.SystemTime.ServerTimeSet)
        {
            GameManager.SystemTime.SetDateTime(
                serverDateTime.Add(TimeZone.CurrentTimeZone.GetUtcOffset(serverDateTime)));
            GameManager.SystemTime.ServerTimeSet = true;
            Debug.Log(string.Format("DEBUG|201712041636|Current time from server is {0}",
                GameManager.SystemTime.Now()));
        }

        Debug.Log(string.Format("DEBUG|201722101155|Sent log reports to {0}", logUrl));
    }

    public void DisablePlayer()
    {
        Debug.Log(string.Format("INFO|201711241150|Player {0} is disabled", UserLocalData.UserCode));
        UserLocalData.PlayerDisabled = true;
        UserLocalData.Save(UserLocalData);
        GameManager.ConnectionError = true;
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
}

public class CanPlayResult
{
    public CanPlayStatus Status;
    public string Message;

    public CanPlayResult(CanPlayStatus status, string msg)
    {
        Status = status;
        Message = msg;
    }
}
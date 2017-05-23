using System;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Debug = UnityEngine.Debug;

namespace NumericalGame
{
    public class UserStatistics : IEnumerable
    {
        public UserLocalData UserLocalData;
        private List<ScoreReports> _scoreReportsToBeSent = new List<ScoreReports>();

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
                System.Threading.Thread.Sleep(250);
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

        public CanPlayStatus CanPlay()
        {
            //Check past days
            if (UserLocalData.PlayDates.Where(day => day.DateObject.Date < DateTime.Today.Date)
                .Any(day => !FinishedDay(day)))
                return CanPlayStatus.NoMoreTimeSlots;
            //Check Today
            //Check if you have sessions today
            if (DateExistsAndHasSessions(GetToday()))
                return CanPlayStatus.CanPlay;
            //Check if you are after 8:00AM
            if (DateTime.Now < DateTime.Today.AddHours(8) && DateExists(DateTime.Today))
                return CanPlayStatus.HasNextTimeslot;

            //Check if exists a future PlayDate
            return GetNextPlayDate() == null ? CanPlayStatus.NoMoreTimeSlots : CanPlayStatus.HasNextTimeslot;
        }

        private bool DateExists(DateTime datetime)
        {
            return UserLocalData.PlayDates.Count(day => day.DateObject.Date == datetime) == 1;
        }

        private PlayDate GetNextPlayDate()
        {
            return UserLocalData.PlayDates.Where(DateExistsAndHasSessions).Min();
        }

        public string TimeToNextSession()
        {
            var nextPlayDate = GetNextPlayDate();
            if (nextPlayDate.DateObject.Date > DateTime.Today)
                return GetTimeSpanToDateTime(nextPlayDate.DateObject.Date.AddHours(8));
            if (DateTime.Now < DateTime.Today.AddHours(8))
                return GetTimeSpanToDateTime(DateTime.Today.AddHours(8));
            var t = TimeSpan.FromSeconds(nextPlayDate.SessionInterval -
                                         (GetEpochTime() - nextPlayDate.LastSessionsEndTime));
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                t.Hours,
                t.Minutes,
                t.Seconds);
        }

        private static string GetTimeSpanToDateTime(DateTime dateTime)
        {
            var t = dateTime.Subtract(DateTime.Now);
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

        private bool DateExistsAndHasSessions(PlayDate date)
        {
            return DateExists(date.DateObject) && !FinishedDay(date);
        }

        public void AddPlayTime(int length, int score)
        {
            var today = GetToday();
            var thisTime = DateTime.Now.ToShortTimeString();
            today.GameRounds.Add(new Rounds(length, Mathf.Max(0, score), thisTime));
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
            Debug.Log(string.Format("Sent {0} score reports to server - {1}", reportsCount,
                Utilities.PrintArray<ScoreReports>(_scoreReportsToBeSent.ToArray())));
            _scoreReportsToBeSent.Clear();
            var url = Constants.BaseUrl + string.Format("/{0}/{1}/", UserLocalData.UserCode, GetToday().SessionId);
            var request = Utilities.CreatePostUnityWebRequest(url, jsonString);
            yield return request.Send();
            if (!request.isError && (request.responseCode == 200 || IsTestUser())) yield break;
            ApplicationState.ConnectionError = true;
            Debug.LogWarning(request.error);
        }

        public void SendUserInfoToServerBlocking()
        {
            if (_scoreReportsToBeSent.Count <= 0) return;
            var reportsCount = _scoreReportsToBeSent.Count;
            var jsonString = JsonConvert.SerializeObject(_scoreReportsToBeSent);
            Debug.Log(string.Format("Sent {0} score reports to server - {1}", reportsCount,
                Utilities.PrintArray<ScoreReports>(_scoreReportsToBeSent.ToArray())));
            _scoreReportsToBeSent.Clear();
            var url = Constants.BaseUrl + string.Format("/{0}/{1}/", UserLocalData.UserCode, GetToday().SessionId);
            var request = Utilities.CreatePostUnityWebRequest(url, jsonString);
            request.Send();
            while (!request.isDone)
                System.Threading.Thread.Sleep(250);
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
            return GetToday().Control == 1;
        }

        public void ClearScoreReports()
        {
            _scoreReportsToBeSent.Clear();
        }
    }
}
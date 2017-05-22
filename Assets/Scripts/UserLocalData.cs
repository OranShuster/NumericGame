using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using Newtonsoft.Json;

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
    public int game_id;
    public override string ToString()
    {
        return string.Format("score = {0}, timestamp = {1}, session_id = {2}, game_id = {3}", score, timestamp,
            session_id, game_id);
    }
}

[Serializable]
public class PlayDate : IComparable<PlayDate>
{
    public string Email { get; set; }
    public string Code { get; set; }
    public int SessionId { get; set; }
    public DateTime DateObject { get; set; }
    public int NumberOfSessions { get; set; }
    public int SessionLength { get; set; }
    public float SessionInterval { get; set; }
    public int Control { get; set; }
    public int CurrentSession = 0;
    public int CurrentSessionTimeSecs = 0;
    public int LastSessionsEndTime = 0;
    public List<Rounds> GameRounds = new List<Rounds>();

    [JsonConstructor]
    public PlayDate(string code, int session_length, string start_date, int num_of_sessions, int control, string email,
        float session_interval, int id)
    {
        Code = code;
        SessionLength = session_length;
        SessionInterval = session_interval;
        Control = control;
        DateObject = DateTime.ParseExact(start_date,Constants.DateFormat,CultureInfo.InvariantCulture);
        NumberOfSessions = num_of_sessions;
        Email = email;
        SessionId = id;
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
    int IComparable<PlayDate>.CompareTo(PlayDate other)
    {
        if (other.DateObject > DateObject)
            return -1;
        return other.DateObject == DateObject ? 0 : 1;
    }
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
            date.SessionInterval *= 60 * 60; //to hours
            date.SessionLength *= 60; // to minutes
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
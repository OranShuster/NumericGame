using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class Rounds
{
    public int RoundLength;
    public int RoundScore;
    public string RoundTime;
    public int SessionInd;
    public Rounds(int length, int score, string time,int sessionInd)
    {
        RoundLength = length;
        RoundScore = score;
        RoundTime = time;
        SessionInd = sessionInd;
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
    public int SessionInterval { get; set; }
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
        SessionLength = session_length * 60;
        SessionInterval = (int)(session_interval * 60 * 60);
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

    public static void Save(UserLocalData userLocalData)
    {
        Debug.Log(String.Format("Saving user data from {0}", UserStatistics.UserDataPath));
        StreamWriter writer = null;
        try
        {
            writer = new StreamWriter(UserStatistics.UserDataPath);
            var jsonString = JsonConvert.SerializeObject(userLocalData, Formatting.Indented);
            byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(jsonString);
            string encodedJson = Convert.ToBase64String(toEncodeAsBytes);
            writer.Write(encodedJson);
        }
        catch(Exception e)
        {
            Debug.LogError(e.StackTrace);
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }

    }

    public static UserLocalData Load()
    {
        Debug.Log(String.Format("Loading user data from {0}", UserStatistics.UserDataPath));
        StreamReader reader = null;
        try
        {
            reader = new StreamReader(UserStatistics.UserDataPath);
            var encodedDataAsBytes = Convert.FromBase64String(reader.ReadToEnd());
            var decodedString = Encoding.ASCII.GetString(encodedDataAsBytes);
            return JsonConvert.DeserializeObject<UserLocalData>(decodedString);
        }
        catch(Exception e)
        {
            Debug.LogError(e.StackTrace);
            return null;
        }
        finally
        {
            if (reader != null)
                reader.Close();
        }
    }

    public static bool PlayerDataValid()
    {
        if (!File.Exists(UserStatistics.UserDataPath))
            return false;
        try
        {
            var userStats = new UserStatistics();
            if (userStats.UserLocalData == null)
            {
                File.Delete(UserStatistics.UserDataPath);
                return false;
            }
        }
        catch
        {
            File.Delete(UserStatistics.UserDataPath);
            return false;
        }
        return true;
    }
}
#if UNIT_TEST
public class Debug
{
public static void Log(string s) { Console.WriteLine(s); }
public static void LogWarning(string s) { Console.WriteLine(s); }
public static void LogError(string s) { Console.WriteLine(s); }
}

public class MonoBehaviour
{

}
#endif
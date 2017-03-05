using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Xml.Serialization;
using System.Collections;

[Serializable]
public class PlayDate
{
    public string SessionDate { get; set; }
    public int NumOfSessions { get; set; }
    public float SessionPlayTime { get; set; }
    public int CurrentSession = 1;
    public float CurrentSessionTime = 0;
    public List<Runs> GameRuns = new List<Runs>();
}

public class Runs
{
    public int RunLength;
    public int RunScore;
    public string RunTime;
    public Runs(int length,int score,string Time)
    {
        RunLength = length;
        RunScore = score;
        RunTime = Time;
    }
    public Runs() { }
}

public class UserInformation : IEnumerable
{
    public PlayDate[] UserPlayDates;
    static string userDataPath = Application.persistentDataPath + "/userData.cjd";
    public bool UserLoaded = false;
    public UserInformation(string userCode)
    {
        #if UNITY_EDITOR
        Utilities.CreateMockUserData();
        #endif
        if (userCode == "")
        {
            UserPlayDates = Load();
            if (UserPlayDates!=null)
                UserLoaded = true;
        }
        else
        {
            string json = GetJsonFromServer(userCode);
            if (json != "")
            {
                UserPlayDates = JsonConvert.DeserializeObject<PlayDate[]>(json);
                Save();
                UserLoaded = true;
            }
            else
                UserLoaded = false;
        }
    }
    public UserInformation(){}

    public string GetJsonFromServer(string userCode)
    {
        UnityWebRequest getReq = UnityWebRequest.Get(Constants.BaseURL + String.Format("?code={0}", userCode));
        getReq.Send();
        while (!getReq.isDone) { }
        if (getReq.isError)
        {
            Debug.Log(getReq.error);
            return "";
        }
        DownloadHandler dl = getReq.downloadHandler;
        return dl.data.ToString();
    }

    public float CanPlay()
    {
        PlayDate todayDateEntry = GetToday();
        if (todayDateEntry != null)
            if (todayDateEntry.CurrentSession <= todayDateEntry.NumOfSessions)
                return todayDateEntry.SessionPlayTime-todayDateEntry.CurrentSessionTime;
        return 0;
    }

    internal void AddPlayTime(int length,int score)
    {
        var today = GetToday();
        var thisTime = DateTime.Now.ToShortTimeString();
        today.GameRuns.Add(new Runs(length, score,thisTime));
        today.CurrentSessionTime += length;
        if (today.CurrentSessionTime>today.SessionPlayTime)
        {
            today.CurrentSessionTime = 0;
            today.CurrentSession++;
        }
    }

    public void Save()
    {
        Debug.Log(string.Format("Saving user data from {0}", userDataPath));
        TextWriter writer = null;
        try
        {
            var serializer = new XmlSerializer(typeof(PlayDate[]));
            writer = new StreamWriter(userDataPath);
            serializer.Serialize(writer, UserPlayDates);
        }
        finally
        {
            if (writer != null)
                writer.Close();
        }

    }

    public PlayDate[] Load()
    {
        if (PlayerDataExists())
        {
            Debug.Log(string.Format("Loading user data from {0}", userDataPath));
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(PlayDate[]));
                reader = new StreamReader(userDataPath);
                return (PlayDate[])serializer.Deserialize(reader);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
        return null;
    }

    public static bool PlayerDataExists()
    {
        return File.Exists(userDataPath);
    }

    public IEnumerator GetEnumerator()
    {
        return UserPlayDates.GetEnumerator();
    }
    public PlayDate GetToday()
    {
        var todayDate = DateTime.Today.ToString(Constants.DateFormat);
        return Array.Find(UserPlayDates, x => x.SessionDate == todayDate);
    }
    public float GetCurrentSessionTime()
    {
        var today = GetToday();
        if (today != null)
            return today.CurrentSessionTime;
        return -1;
    }
}
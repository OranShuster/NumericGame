using System;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Newtonsoft.Json;

public class PlayDate
{
    public string SessionDate { get; set; }
    public int NumOfSessions { get; set; }
    public int SessionPlayTime { get; set; }
    public int CurrentSession = 1;
    public int CurrentSessionTime = 0;
}

public class UserInformation : IEnumerable
{
    public PlayDate[] UserPlayDates=new PlayDate[0];
    static string userDataPath = Application.persistentDataPath + "/userData.cjd";
    public UserInformation(string userCode)
    {
        if (userCode == "")
            UserPlayDates = Load();
        else
        {
            string json = GetJsonFromServer(userCode);
            UserPlayDates = JsonConvert.DeserializeObject<PlayDate[]>(json);
        }
    }
    public UserInformation(){}

    public static string GetJsonFromServer(string userCode)
    {
        throw new NotImplementedException("GetJsonFromServer");
    }

    public int CanPlay(DateTime playTime)
    {
        var todayDate = DateTime.Today.ToString(Constants.DateFormat);
        PlayDate todayDateEntry = Array.Find(UserPlayDates, x => x.SessionDate == todayDate);
        if (todayDateEntry != null)
            if (todayDateEntry.CurrentSession <= todayDateEntry.NumOfSessions)
                return todayDateEntry.SessionPlayTime-todayDateEntry.CurrentSessionTime;
        return 0;
    }

    public void Save()
    {
        Debug.Log(String.Format("Saving user data from {0}", userDataPath));
        XmlSerializer saver = new XmlSerializer(UserPlayDates.GetType());
        TextWriter tw = new StreamWriter(new FileStream(userDataPath, FileMode.OpenOrCreate));
        saver.Serialize(tw,UserPlayDates);
    }

    public PlayDate[] Load()
    {
        Debug.Log(String.Format("Loading user data from {0}",userDataPath));
        if (PlayerDataExists())
        {
            XmlSerializer loader = new XmlSerializer(UserPlayDates.GetType());
            TextReader tr = new StreamReader(new FileStream(userDataPath, FileMode.Open));
            return (PlayDate[])loader.Deserialize(tr);
        }
        Debug.Log("User data not found. wrong constructor used");
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
}
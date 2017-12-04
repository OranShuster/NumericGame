using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class Utilities
{
    public static int GetEpochTime()
    {
        return (int) GameManager.SystemTime.Now().ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public static string LoadStringFromFile(string key, int lineLength = 15)
    {
        INIParser ini = new INIParser();
        TextAsset asset = Resources.Load("StringsFile") as TextAsset;
        ini.Open(asset);
        return ReverseText(ini.ReadValue("Translation", key, key), lineLength);
    }

    public static string PrintArray<T>(T[] arr)
    {
        var ret = string.Join(",", Array.ConvertAll(arr, i => i.ToString()));
        return ret;
    }

    public static void CreateMockUserData(bool control)
    {
        const int SessionLength = 10 * 60;
        const int SessionInterval = 1 * 60 * 60;
        const int SessionNum = 3;
        var mockDates = new PlayDate[3];
        mockDates[0] = new PlayDate
        {
            SessionLength = SessionLength,
            NumberOfSessions = SessionNum,
            DateObject = DateTime.Today,
            Control = control ? 1 : 0,
            Code = control ? "desiree2" : "desiree",
            SessionInterval = SessionInterval
        };
        mockDates[1] = new PlayDate
        {
            SessionLength = SessionLength,
            NumberOfSessions = SessionNum,
            DateObject = DateTime.Today.AddDays(1),
            Control = control ? 1 : 0,
            Code = control ? "desiree2" : "desiree",
            SessionInterval = SessionInterval
        };
        mockDates[2] = new PlayDate
        {
            SessionLength = SessionLength,
            NumberOfSessions = SessionNum,
            DateObject = DateTime.Today.AddDays(2),
            Control = control ? 1 : 0,
            Code = control ? "desiree2" : "desiree",
            SessionInterval = SessionInterval
        };
        var userLocalData = new UserLocalData(mockDates, control ? "desiree2" : "desiree");
        UserLocalData.Save(userLocalData);
    }

    public static bool IsTestCode(string usercode)
    {
        return Constants.TestCode == usercode || Constants.TestCodeControl == usercode;
    }

    public static void LoggerCallback(string logString, string stackTrace, LogType type)
    {
        if (GameManager.UserInformation == null)
            return;
        if (GameManager.UserInformation.IsTestUser())
            return;
        try
        {
            var priority = logString.Split('|')[0];
            if (priority == "DEBUG")
                return;
            GameManager.Instance.SendLogs(new LogMessage
            {
                priority = priority,
                timestamp = GetEpochTime(),
                log_id = logString.Split('|')[1],
                raw_data = logString.Split('|')[2]
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static UnityWebRequest CreatePostUnityWebRequest(string url, string body)
    {
        var request = new UnityWebRequest(url)
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer(),
            method = UnityWebRequest.kHttpVerbPOST
        };
        request.uploadHandler.contentType = "application/json";
        request.SetRequestHeader("content-type", "application/json");
        return request;
    }

    public static string ReverseText(string str, int lineLength = 15)
    {
        string individualLine = ""; //Control individual line in the multi-line text component.
        var reversedString = "";
        var listofWords = str.Split(' ').ToList(); //Extract words from the sentence

        foreach (var s in listofWords)
        {
            if (individualLine.Length >= lineLength)
            {
                reversedString +=
                    ReverseLine(individualLine) +
                    "\n"; //Add a new line feed at the end, since we cannot accomodate more characters here.
                individualLine = ""; //Reset this string for new line.
            }
            individualLine += s + " ";
        }
        individualLine = individualLine.Substring(0, individualLine.Length - 1);
        if (individualLine != "")
            reversedString += ReverseLine(individualLine);
        return reversedString;
    }

    private static string ReverseLine(string s)
    {
        var charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public static bool IsControlCode(string userCode)
    {
        return userCode == Constants.TestCodeControl;
    }

    public static string SecondsToTime(long seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        if (t.Hours > 0)
            return string.Format("{0:D2}:{1:D2}:{2:D2}",
                t.Hours,
                t.Minutes,
                t.Seconds);
        return string.Format("{0:D2}:{1:D2}",
            t.Minutes,
            t.Seconds);
    }
    public static T[] SubArray<T>(this T[] data, int index, int length)
    {
        T[] result = new T[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }

    public static int ToInt(this int[] data)
    {
        return data.Select((t, i) => t * Convert.ToInt32(Math.Pow(10, data.Length - i - 1))).Sum();
    }
}

public class LogMessage
{
    public int timestamp;
    public string log_id;
    public string priority;
    public string location;
    public string raw_data;
}

public static class DebugUtilities
{
    public static string DebugPositions(GameObject hitGo, GameObject hitGo2)
    {
        return hitGo.GetComponent<NumberCell>().Row + "-"
               + hitGo.GetComponent<NumberCell>().Column + "-"
               + hitGo2.GetComponent<NumberCell>().Row + "-"
               + hitGo2.GetComponent<NumberCell>().Column;
    }

    public static void ShowArray(ShapesMatrix shapes, int size)
    {
        Debug.Log(string.Format("DEBUG|201710221545|{0}", GetArrayContents(shapes, size)));
    }

    private static string GetArrayContents(ShapesMatrix shapes, int size)
    {
        var x = string.Empty;
        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                if (shapes[row, column] == null)
                    x += "NULL  |";
                else
                {
                    var shape = shapes[row, column].GetComponent<NumberCell>();
                    x += shape.Value.ToString("D2") + " | ";
                }
            }
            x += Environment.NewLine;
        }
        return x;
    }
}
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
        return (int) UserInformation.SystemTime.Now().ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public static bool AreNeighbors(NumberCell s1, NumberCell s2)
    {
        return (s1.Column == s2.Column ||
                s1.Row == s2.Row)
               && Mathf.Abs(s1.Column - s2.Column) <= 1
               && Mathf.Abs(s1.Row - s2.Row) <= 1;
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
        var ret = string.Join(",", Array.ConvertAll<T, string>(arr, i => i.ToString()));
        return ret;
    }

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        var ret = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        return Mathf.Clamp(ret, from2, to2);
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
            Control = control ? 1 : 0 ,
            Code = control  ? "desiree2" : "desiree",
            SessionInterval = SessionInterval
        };
        mockDates[1] = new PlayDate
        {
            SessionLength = SessionLength,
            NumberOfSessions = SessionNum,
            DateObject = DateTime.Today.AddDays(1),
            Control = control ? 1 : 0 ,
            Code = control  ? "desiree2" : "desiree",
            SessionInterval = SessionInterval
        };
        mockDates[2] = new PlayDate
        {
            SessionLength = SessionLength,
            NumberOfSessions = SessionNum,
            DateObject = DateTime.Today.AddDays(2),
            Control = control ? 1 : 0 ,
            Code = control  ? "desiree2" : "desiree",
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
        if (ApplicationState.UserInformation == null)
            return;
        if (ApplicationState.UserInformation.IsTestUser())
            return;
        var priority = logString.Split('|')[0];
        if (priority == "DEBUG")
            return;
        ApplicationState.UserInformation.Logs.Add(new LogMessage
        {
            priority = priority,
            timestamp = GetEpochTime(),
            log_id = logString.Split('|')[1],
            raw_data = logString.Split('|')[2]
        });
        if (ApplicationState.UserInformation.Logs.Count >= 2)
            ApplicationState.UserInformation.SendLogs(); 
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
}

public class LogMessage
{
    public int timestamp;
    public string log_id;
    public string priority;
    public string location;
    public string raw_data;
}

public class CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;

    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}

public static class DebugUtilities
{
    public static void DebugPositions(GameObject hitGo, GameObject hitGo2)
    {
        var lala =
            hitGo.GetComponent<NumberCell>().Row + "-"
            + hitGo.GetComponent<NumberCell>().Column + "-"
            + hitGo2.GetComponent<NumberCell>().Row + "-"
            + hitGo2.GetComponent<NumberCell>().Column;
        Debug.Log(string.Format("DEBUG|2017102215|{0}",lala));

    }

    public static void ShowArray(ShapesMatrix shapes, int size)
    {

        Debug.Log(string.Format("DEBUG|201710221545|{0}", GetArrayContents(shapes, size)));
    }

    public static string GetArrayContents(ShapesMatrix shapes, int size)
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

    public static Color HexToRgb(string hex)
    {
        var bigint = Convert.ToUInt32(hex, 16);
        var r = (bigint >> 16) & 255;
        var g = (bigint >> 8) & 255;
        var b = bigint & 255;

        return new Color(r, g, b);
    }
}